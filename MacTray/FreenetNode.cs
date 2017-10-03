using System;
using System.IO;
using System.Net;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using FCP2.Protocol;

using Common;

namespace MacTray {
    enum FCPConnectionState {
        disconnected,
        connecting,
        connected,
        ready,
    }

    public class FreenetNode {
        #region Properties
        //FileSystemWatcher ConfigWatcher;

        public Uri FProxyLocation { get; set; }
        public Uri FCPLocation { get; set; }

        public string DownloadsFolder { get; set; }

        FCPConnectionState ConnectionState = FCPConnectionState.disconnected;

        Dictionary<string, string> WrapperConfig { get; set; }
        Dictionary<string, string> FreenetConfig { get; set; }

        FCP2Protocol Client { get; set; }

        string Location { get; set; }

        #endregion

        #region Initialization
        public FreenetNode() {
            // spawn a thread to monitor fcp availability
            Task.Factory.StartNew(() => {
                NodeConnectionLoop();
            });

            // spawn a thread to monitor node installation
            Task.Factory.StartNew(() => {
                CheckNodeInstallation();
            });

            EventRouter.InstallFinishedEvent += OnInstallFinishedEvent;
            EventRouter.InstallFailedEvent += OnInstallFailedEvent;
            EventRouter.StartNodeEvent += OnStartNodeEvent;
            EventRouter.StopNodeEvent += OnStopNodeEvent;
            EventRouter.NodeLocationFoundEvent += OnNodeLocationFoundEvent;
        }
        #endregion

        #region Node handling

        public void Start() {
            if (Location != null && ValidateNodeInstallationAt(Location)) {
                var runScript = Path.Combine(Location, Constants.FNNodeRunscriptPathname);

                var processInfo = new ProcessStartInfo() {
                    FileName = runScript,
                    Arguments = "start",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(processInfo);
                process.WaitForExit();

                Console.WriteLine("start script exited");

            } else {
                MacHelpers.DisplayNodeMissingAlert();
            }
        }

        public void Stop() {
            if (Location != null && ValidateNodeInstallationAt(Location)) {
                Task.Factory.StartNew(() => {
                    var runScript = Path.Combine(Location, Constants.FNNodeRunscriptPathname);

                    var processInfo = new ProcessStartInfo() {
                        FileName = runScript,
                        Arguments = "stop",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    var process = Process.Start(processInfo);
                    process.WaitForExit();

                    Console.WriteLine("stop script exited");

                    // once run.sh returns, we ensure the wrapper state is cleaned up
                    // this fixes issues where Freenet.anchor is still around but the wrapper crashed, so the node
                    // isn't actually running but the tray app thinks it is, preventing users from using start/stop
                    // in the dropdown menu until things go back to a sane state
                    CleanupAfterShutdown(Location);
                });
            } else {
                MacHelpers.DisplayNodeMissingAlert();
            }
        }

        void CleanupAfterShutdown(string nodeLocation) {
            var anchorFile = Path.Combine(nodeLocation, Constants.FNNodeAnchorFilePathname);
            var pidFile = Path.Combine(nodeLocation, Constants.FNNodePIDFilePathname);

            try {
                File.Delete(anchorFile);
                File.Delete(pidFile);
            } catch {
                // these are best effort cleanup attempts, we don't care if they fail or why
            }
        }

        void CheckNodeInstallation() {
            // start a continuous loop to monitor installation directory
            while (true) {
                Thread.Sleep(Constants.FNNodeCheckTimeInterval * 1000);

                // silence compiler warning
                var f = false;

                if (f) {
                    return;
                }

                if (Location == null || !ValidateNodeInstallationAt(Location)) {
                    EventRouter.NodeLocationNotFound(new NodeLocationNotFoundEventArgs());
                }
            }
        }


        public static bool ValidateNodeInstallationAt(string nodePath) {
            if (nodePath == null) {
                return false;
            }

            var runScript = Path.Combine(nodePath, Constants.FNNodeRunscriptPathname);

            if (File.Exists(runScript)) {
                return true;
            }

            return false;
        }

        public void NodeConnectionLoop() {
            while (true) {
                // silence compiler warning
                var f = false;

                if (f) {
                    return;
                }

                System.Threading.Thread.Sleep(Constants.FNNodeCheckTimeInterval * 1000);

                try {
                    switch (ConnectionState) {
                        case FCPConnectionState.disconnected:
                            if (FCPLocation == null) {
                                continue;
                            }

                            var host = FCPLocation.Host;
                            var port = FCPLocation.Port;

                            if (host == null) {
                                continue;
                            }

                            IPAddress ipAddress = IPAddress.Parse(host);
                            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                            ConnectionState = FCPConnectionState.connecting;

                            Client = new FCP2Protocol(remoteEP, "MacTray");

                            Client.NodeHelloEvent += OnNodeHello;
                            Client.NodeDataEvent += OnNodeData;

                            //Client.CloseConnectionUnavailableEvent += OnCloseConnectionUnavailable;

                            Client.ClientHello();

                            continue;
                        case FCPConnectionState.connecting:
                            continue;
                        case FCPConnectionState.connected:
                            Client.GetNode(false, false, true);
                            continue;
                        case FCPConnectionState.ready:
                            continue;
                    }
                } catch {
                    // any exception that makes it up to this level is likely to be
                    // a network error, which effectively means the node is not running

                    ConnectionState = FCPConnectionState.disconnected;
                    EventRouter.NodeStateNotRunning(new NodeStateNotRunningEventArgs());
                }
            }
        }
        #endregion


        #region Configuration handlers

        void ReadFreenetConfig() {
            if (Location == null) {
                return;
            }

            if (ValidateNodeInstallationAt(Location)) {
                var wrapperConfigFile = Path.Combine(Location, Constants.FNNodeWrapperConfigFilePathname);

                var freenetConfigFile = Path.Combine(Location, Constants.FNNodeFreenetConfigFilePathname);

                try {
                    WrapperConfig = Common.Node.Config.From(wrapperConfigFile);
                    if (WrapperConfig == null) {
                        throw new Exception("Can't find wrapper.conf");
                    }

                    FreenetConfig = Common.Node.Config.From(freenetConfigFile);
                    if (FreenetConfig == null) {
                        throw new Exception("Can't find freenet.ini");
                    }

                    FreenetConfig.TryGetValue(Constants.FNNodeFreenetConfigFCPBindAddressesKey, out string fcpBindings);
                    if (fcpBindings == null) {
                        throw new Exception("FCP binding missing in freenet.ini");
                    }

                    FreenetConfig.TryGetValue(Constants.FNNodeFreenetConfigFProxyBindAddressesKey, out string fproxyBindings);
                    if (fproxyBindings == null) {
                        throw new Exception("FProxy binding missing in freenet.ini");
                    }

                    FreenetConfig.TryGetValue(Constants.FNNodeFreenetConfigDownloadsDirKey, out string downloadsPath);
                    if (downloadsPath == null) {
                        throw new Exception("Downloads path missing in freenet.ini");
                    }

                    if (fcpBindings.Length > 0) {
                        // first one should be ipv4
                        var fcpBindTo = fcpBindings.Split(',')[0];
                        var fcpPort = FreenetConfig[Constants.FNNodeFreenetConfigFCPPortKey];
                        if (fcpPort != null) {
                            var s = String.Format("tcp://{0}:{1}", fcpBindTo, fcpPort);

                            FCPLocation = new Uri(s);
                        } else {
                            throw new Exception("FCP port missing in freenet.ini");
                        }
                    }

                    if (fproxyBindings.Length > 0) {
                        var fproxyBindTo = fproxyBindings.Split(',')[0]; // first one should be ipv4
                        var fproxyPort = FreenetConfig[Constants.FNNodeFreenetConfigFProxyPortKey];

                        if (fproxyPort != null) {
                            var s = String.Format("http://{0}:{1}", fproxyBindTo, fproxyPort);

                            FProxyLocation = new Uri(s);
                        } else {
                            throw new Exception("FProxy port missing in freenet.ini");
                        }
                    }

                    if (File.Exists(downloadsPath)) {
                        DownloadsFolder = downloadsPath;
                    } else {
                        // node.downloadsDir isn't a full path, so probably relative to the node files
                        DownloadsFolder = Path.Combine(Location, downloadsPath);
                    }

                    EventRouter.NodeConfigured(new NodeConfiguredEventArgs());

                } catch (Exception ex) {
                    MacHelpers.ConfigInvalidAlert(ex.Message, () => {
                        Environment.Exit(1);
                    });
                    return;
                }
            }
        }
        #endregion

        #region Config monitoring
        // Define the event handlers.
        void OnChanged(object source, FileSystemEventArgs e) {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            switch (e.ChangeType) {
                case WatcherChangeTypes.Changed:
                    ReadFreenetConfig();
                    break;
                case WatcherChangeTypes.Created:
                    ReadFreenetConfig();
                    break;
                case WatcherChangeTypes.Deleted:
                    ReadFreenetConfig();
                    break;
                default:
                    break;
            }
        }

        void OnRenamed(object source, RenamedEventArgs e) {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
        #endregion



        #region fcp2lib callback events
        void OnNodeHello(object sender, FCP2.EventArgs.NodeHelloEventArgs e) {

            ConnectionState = FCPConnectionState.connected;

            EventRouter.NodeHello(new NodeHelloEventArgs(e.ConnectionIdentifier,
                                                         e.FcpVersion,
                                                         e.Version,
                                                         e.Node,
                                                         e.NodeLanguage,
                                                         e.ExtBuild,
                                                         e.ExtRevision,
                                                         e.Build,
                                                         e.Revision,
                                                         e.Testnet,
                                                         e.CompressionCodecs));

        }

        void OnNodeData(object sender, FCP2.EventArgs.NodeDataEventArgs e) {
            EventRouter.NodeStateRunning(new NodeStateRunningEventArgs());
            EventRouter.NodeData(new NodeDataEventArgs(e.LastGoodVersion,
                                                       e.Sig,
                                                       e.Opennet,
                                                       e.Identity,
                                                       e.Version,
                                                       e.Location));

        }

        void OnInstallFinishedEvent(object sender, Common.InstallFinishedEventArgs e) {
            Location = e.Location;
        }

        void OnInstallFailedEvent(object sender, Common.InstallFailedEventArgs e) {
            Location = null;
        }

        void OnStartNodeEvent(object sender, Common.StartNodeEventArgs e) {
            Start();
        }

        void OnStopNodeEvent(object sender, Common.StopNodeEventArgs e) {
            Stop();
        }

        void OnNodeLocationFoundEvent(object sender, Common.NodeLocationFoundEventArgs e) {
            Location = e.Location;
            ReadFreenetConfig();

            //if (ConfigWatcher != null) {
            //ConfigWatcher.EnableRaisingEvents = false;
            //}

            //ConfigWatcher = new FileSystemWatcher(path, "freenet.ini") {

            /* Watch for changes in LastAccess and LastWrite times, and
			   the renaming of files or directories. */
            // NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            //};

            // Add event handlers.
            //ConfigWatcher.Changed += new FileSystemEventHandler(this.OnChanged);
            //ConfigWatcher.Created += new FileSystemEventHandler(this.OnChanged);
            //ConfigWatcher.Deleted += new FileSystemEventHandler(this.OnChanged);
            //ConfigWatcher.Renamed += new RenamedEventHandler(this.OnRenamed);

            // Begin watching.
            //ConfigWatcher.EnableRaisingEvents = true;
        }
        #endregion
    }
}
