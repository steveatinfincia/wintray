using AppKit;
using Foundation;
using CoreFoundation;

using System;

using Common;

namespace MacTray {
    [Register("SettingsWindowController")]
    public partial class SettingsWindowController: NSWindowController {

        FreenetNode Node { get; set; }

        [Outlet]
        NSImageView NodeRunningStatusView { get; set; }

        [Outlet]
        NSImageView WebInterfaceStatusView { get; set; }

        [Outlet("fcpStatusView")]
        NSImageView FCPStatusView { get; set; }

        [Outlet]
        NSTextField NodeBuildField { get; set; }

        [Outlet("nodePathDisplay")]
        NSPathControl NodePathDisplay { get; set; }

        [Outlet("findNodeButton")]
        NSButton FindNodeButton { get; set; }

        [Outlet("nodeLocation")]
        NSUrl NodeLocation { get; set; }

        [Outlet("nodeMissingField")]
        NSTextField NodeMissingField { get; set; }

        [Outlet("nodeMissingImage")]
        NSImageView NodeMissingImage { get; set; }

        [Outlet("uninstallButton")]
        NSButton UninstallButton { get; set; }

        [Outlet("validNodeFound")]
        public bool ValidNodeFound {
            get {
                if (NodeLocation == null) {
                    return false;
                }


                var path = NodeLocation.Path;

                return FreenetNode.ValidateNodeInstallationAt(path);
            }
        }

        [Outlet("loginItem")]
        bool LoginItem {
            set {
                //Helpers.EnableLoginItem(value);
                NSUserDefaults.StandardUserDefaults.SetBool(value, Constants.FNStartAtLaunchKey);
            }
            get {
                return NSUserDefaults.StandardUserDefaults.BoolForKey(Constants.FNStartAtLaunchKey);
            }
        }

        public SettingsWindowController(FreenetNode Node) : base("SettingsWindow") {
            this.Node = Node;
        }

        public override void WindowDidLoad() {
            base.WindowDidLoad();
        }

        public override void AwakeFromNib() {
            base.AwakeFromNib();

            EventRouter.NodeStateNotRunningEvent += OnNodeStateNotRunningEvent;
            EventRouter.NodeStateRunningEvent += OnNodeStateRunningEvent;

            EventRouter.NodeDataEvent += OnNodeDataEvent;
            EventRouter.NodeHelloEvent += OnNodeHelloEvent;

            EventRouter.ShowSettingsEvent += OnShowSettingsEvent;

            EventRouter.FindNodeLocationEvent += OnFindNodeLocationEvent;
            EventRouter.NodeLocationFoundEvent += OnNodeLocationFoundEvent;
            EventRouter.NodeLocationNotFoundEvent += OnNodeLocationNotFoundEvent;

        }

        // MARK: - Interface actions


        [Action("uninstallFreenet:")]
        public void UninstallFreenet(NSObject sender) {
            MacHelpers.DisplayUninstallAlert();
        }

        [Action("openWebInterface:")]
        public void OpenWebInterface(NSObject sender) {
            EventRouter.ShowFProxy(new ShowFProxyEventArgs());
        }

        [Action("selectNodeLocation:")]
        public void SelectNodeLocation(NSObject sender) {
            EventRouter.FindNodeLocation(new FindNodeLocationEventArgs());
        }

        #region App Events
        void OnNodeStateRunningEvent(object sender, Common.NodeStateRunningEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                var n = NSImageName.StatusAvailable;
                var i = NSImage.ImageNamed(n);
                NodeRunningStatusView.Image = i;
            });
        }

        void OnNodeStateNotRunningEvent(object sender, Common.NodeStateNotRunningEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                var n = NSImageName.StatusUnavailable;
                var i = NSImage.ImageNamed(n);
                NodeRunningStatusView.Image = i;
                FCPStatusView.Image = i;

                NodeBuildField.StringValue = "";
            });
        }

        void OnNodeHelloEvent(object sender, Common.NodeHelloEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                NodeBuildField.StringValue = e.Build.ToString();
            });
        }

        void OnNodeDataEvent(object sender, Common.NodeDataEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                var n = NSImageName.StatusAvailable;
                var i = NSImage.ImageNamed(n);
                FCPStatusView.Image = i;
            });
        }

        void OnShowSettingsEvent(object sender, Common.ShowSettingsEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                ShowWindow(this);
                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            });
        }

        void OnFindNodeLocationEvent(object sender, Common.FindNodeLocationEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                var openpanel = new NSOpenPanel() {
                    WeakDelegate = this,
                    CanChooseFiles = false,
                    AllowsMultipleSelection = false,
                    CanChooseDirectories = true,
                    Title = NSBundle.MainBundle.LocalizedString("Find your Freenet installation", "Title of window"),
                    Prompt = NSBundle.MainBundle.LocalizedString("Select Freenet installation", "Button title")
                };

                openpanel.Begin((result) => {
                    switch ((NSModalResponse)(int)result) {
                        case NSModalResponse.OK:
                            var u = openpanel.Url.Path;
                            EventRouter.NodeLocationFound(new NodeLocationFoundEventArgs(u));
                            ShowWindow(this);
                            break;
                        default:
                            break;
                    }
                });
            });
        }

        void OnNodeLocationFoundEvent(object sender, Common.NodeLocationFoundEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                NodeLocation = new NSUrl(e.Location, true);
                NodePathDisplay.Url = NodeLocation;
                NodePathDisplay.Hidden = false;

                UninstallButton.Enabled = true;
                FindNodeButton.Enabled = false;
                NodeMissingImage.Hidden = true;
                NodeMissingField.Hidden = true;

                var n = NSImageName.StatusUnavailable;
                var i = NSImage.ImageNamed(n);
                NodeRunningStatusView.Image = i;
                FCPStatusView.Image = i;

                DidChangeValue("NodeLocation");
            });
        }

        void OnNodeLocationNotFoundEvent(object sender, Common.NodeLocationNotFoundEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                //NodePathDisplay.Url = null;
                NodePathDisplay.Hidden = true;

                UninstallButton.Enabled = false;
                FindNodeButton.Enabled = true;
                NodeMissingImage.Hidden = false;
                NodeMissingField.Hidden = false;

                DidChangeValue("NodeLocation");
            });
        }
        #endregion

        // NSOpenSavePanelDelegate

        [Export("panel:validateURL:error:")]
        public bool ValidateUrl(NSSavePanel panel, NSUrl url, out NSError outError) {
            if (FreenetNode.ValidateNodeInstallationAt(url.Path)) {
                // silencing compiler, error isn't used in this code path
                outError = null;
                return true;
            }

            var errorInfo = new NSMutableDictionary {
                [NSError.LocalizedDescriptionKey] = new NSString(NSBundle.MainBundle.LocalizedString("Not a valid Freenet installation", "String informing the user that the selected location is not a Freenet installation"))
            };
            outError = new NSError(new NSString("org.freenetproject"), 0x1000, errorInfo);
            return false;
        }
    }
}
