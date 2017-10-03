using System;
using CoreFoundation;
using Common;
using Foundation;

using System.Net;
using System.IO;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using System.Linq;

namespace MacTray {
    public class Installer {
        InstallerDelegate InstallationDelegate;
        DispatchQueue LogQueue = new DispatchQueue("org.freenetproject.mactray.log");
        string InstallLog { get; set; }

        public Installer(InstallerDelegate installationDelegate) {
            InstallationDelegate = installationDelegate;
            InstallLog = "";
        }

        #region Step 1 - entry point
        public void InstallNodeAt(string installLocation) {
            Task.Factory.StartNew(() => {
                while (!MacHelpers.IsJavaInstalled()) {
                    InstallationDelegate.InstallerNeedsJava();
                    Thread.Sleep(1000);
                    continue;
                }

                InstallationDelegate.InstallerFoundJava();

                // Java is now installed, continue installation
                CopyNodeTo(installLocation);
            });
        }
        #endregion

        #region Step 2 - copy files
        void CopyNodeTo(string installLocation) {
            InstallationDelegate.InstallerStartedCopyFiles();

            AppendToInstallLog("Starting installation");

            var bundledNode = NSBundle.MainBundle.GetUrlForResource("Bundled Node", "");

            if (bundledNode == null) {
                AppendToInstallLog($"Bundled Freenet node missing");
                InstallationDelegate.InstallerDidFailWithLog(GetInstallLog());
                return;
            }

            var fileManager = new NSFileManager();


            try {
                if (Directory.Exists(installLocation)) {
                    AppendToInstallLog($"Removing existing directory at: {installLocation}");
                    Directory.Delete(installLocation, true);
                }
                if (File.Exists(installLocation)) {
                    AppendToInstallLog($"Removing existing file at: {installLocation}");
                    File.Delete(installLocation);
                }
            } catch (Exception ex) {
                AppendToInstallLog($"Error removing files at installation directory: {ex.Message}");
                InstallationDelegate.InstallerDidFailWithLog(GetInstallLog());
                return;
            }

            AppendToInstallLog($"Copying files to {installLocation}");

            try {
                fileManager.Copy(bundledNode.Path, installLocation, out NSError copyError);

                if (copyError != null) {
                    AppendToInstallLog($"File copy error: {copyError.LocalizedDescription}");
                    InstallationDelegate.InstallerDidFailWithLog(GetInstallLog());
                    return;
                }
            } catch (NSErrorException ex) {
                AppendToInstallLog($"File copy error: {ex.Message}");
                InstallationDelegate.InstallerDidFailWithLog(GetInstallLog());
                return;
            }

            AppendToInstallLog("Copy finished");

            Thread.Sleep(1000);

            InstallationDelegate.InstallerDidCopyFiles();

            ConfigureNodeAt(installLocation);
        }
        #endregion

        #region Set up node


        // MARK: - Step 3: Set up node and find available ports

        void ConfigureNodeAt(string installLocation) {
            InstallationDelegate.InstallerStartedConfigureNode();

            AppendToInstallLog("Running setup script");


            var processInfo = new ProcessStartInfo() {
                FileName = $"{installLocation}/bin/setup.sh",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            processInfo.EnvironmentVariables["INSTALL_PATH"] = new NSString(installLocation);
            processInfo.EnvironmentVariables["LANG_SHORTCODE"] = new NSString((NSLocale.CurrentLocale as NSLocale).LanguageCode);
            processInfo.EnvironmentVariables["LANG_SHORTCODE"] = new NSString((NSLocale.CurrentLocale as NSLocale).LanguageCode);
            processInfo.EnvironmentVariables["FPROXY_PORT"] = new NSString(AvailableFProxyPort());
            processInfo.EnvironmentVariables["FCP_PORT"] = new NSString(AvailableFCPPort());

            try {
                var process = Process.Start(processInfo);

                process.WaitForExit();

                var exitStatus = process.ExitCode;

                Console.WriteLine("Node configuration task exited with code: {0}", exitStatus);

                if (exitStatus != 0) {
                    InstallationDelegate.InstallerDidFailWithLog(GetInstallLog());
                    return;
                }
            } catch (Exception ex) {
                Console.WriteLine("exception: {0}", ex.Message);
                InstallationDelegate.InstallerDidFailWithLog(GetInstallLog());
                return;
            }

            Thread.Sleep(1000);

            InstallationDelegate.InstallerDidConfigureNode();

            // from this point on, everything is event based.

            Console.WriteLine("Installer registering OnNodeConfiguredEvent");

            EventRouter.NodeConfiguredEvent += OnNodeConfiguredEvent;

            // notify the node controller that a node is ready to be configured for use.
            // control flow continues at OnNodeConfiguredEvent
            Console.WriteLine("Installer sending NodeLocationFoundEvent");

            EventRouter.NodeLocationFound(new NodeLocationFoundEventArgs(installLocation));
        }
        #endregion

        #region Events
        void OnNodeConfiguredEvent(object sender, Common.NodeConfiguredEventArgs e) {
            Console.WriteLine("Installer received NodeConfiguredEvent");

            Console.WriteLine("Installer unregistering OnNodeConfiguredEvent");
            EventRouter.NodeConfiguredEvent -= OnNodeConfiguredEvent;

            InstallationDelegate.InstallerStartingNode();

            Console.WriteLine("Installer registering OnNodeStateRunningEvent");
            EventRouter.NodeStateRunningEvent += OnNodeStateRunningEvent;

            // control flow continues at OnNodeStateRunningEvent
            Console.WriteLine("Installer sending StartNodeEvent");
            EventRouter.StartNode(new StartNodeEventArgs());
        }

        void OnNodeStateRunningEvent(object sender, Common.NodeStateRunningEventArgs e) {
            Console.WriteLine("Installer received NodeStateRunningEvent");

            Console.WriteLine("Installer unregistering OnNodeStateRunningEvent");
            EventRouter.NodeStateRunningEvent -= OnNodeStateRunningEvent;

            InstallationDelegate.InstallerStartedNode();

            Thread.Sleep(1000);

            InstallationDelegate.InstallerDidFinish();

            AppendToInstallLog("Installation finished");

            Console.WriteLine("Installer sending InstallFinishedEvent");

            EventRouter.InstallFinished(new InstallFinishedEventArgs(GetInstallLog()));


        }
        #endregion

        #region Logging
        // MARK: - Logging

        void AppendToInstallLog(string line) {
            LogQueue.DispatchAsync(() => {
                InstallLog += line;
                Console.WriteLine(line);
            });
        }

        string GetInstallLog() {
            var log = "";
            LogQueue.DispatchSync(() => {
                log = InstallLog;
            });
            return log;
        }
        #endregion


        #region Port handling
        // MARK: - Port test helpers

        string AvailableFCPPort() {
            var start = Constants.FNInstallDefaultFCPPort;
            var end = start + 256;

            var port = start;

            while (port < end) {
                if (TestListenPort(port)) {
                    return port.ToString();
                }
                port += 1;
            }
            return null;
        }

        string AvailableFProxyPort() {
            var start = Constants.FNInstallDefaultFProxyPort;
            var end = start + 256;

            var port = start;

            while (port < end) {
                if (TestListenPort(port)) {
                    return port.ToString();
                }
                port += 1;
            }
            return null;
        }

        bool TestListenPort(int port) {
            IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            try {
                TcpListener tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();
                tcpListener.Stop();
                AppendToInstallLog($"Port {port} available");
                return true;
            } catch (SocketException ex) {
                AppendToInstallLog($"Port {port} unavailable: {ex.Message}");
                return false;
            }
        }
        #endregion
    }
}
