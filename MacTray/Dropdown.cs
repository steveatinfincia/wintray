using System;

using AppKit;
using Foundation;
using CoreFoundation;
//using ObjCRuntime;
using System.Runtime.CompilerServices;

using Common;

namespace MacTray {

    [Register("Dropdown")]
    public class Dropdown: NSObject {
        NSStatusItem StatusItem;

        FreenetNode Node;

        Constants.FNNodeState NodeState = Constants.FNNodeState.notRunning;

        NSMenu dropdownMenu { get; set; }

        NSMenuItem ToggleNodeStateMenuItem { get; set; }

        NSMenuItem OpenWebInterfaceMenuItem { get; set; }

        NSMenuItem OpenDownloadsMenuItem { get; set; }

        NSMenuItem SettingsMenuItem { get; set; }

        NSMenuItem AboutMenuItem { get; set; }

        NSMenuItem QuitMenuItem { get; set; }

        NSMenuItem InstallerMenuItem { get; set; }

        NSImage RunningIcon { get; set; }
        NSImage NotRunningIcon { get; set; }
        NSImage HighlightedIcon { get; set; }

        string StartTitle { get; set; }
        string StopTitle { get; set; }
        string OpenWebInterfaceTitle { get; set; }
        string OpenDownloadsTitle { get; set; }
        string SettingsTitle { get; set; }
        string InstallTitle { get; set; }
        string AboutTitle { get; set; }
        string QuitTitle { get; set; }


        NSImage MenuBarImage {
            get {
                return StatusItem.Image;

            }
            set {
                StatusItem.Image = value;
            }
        }

        public override void AwakeFromNib() {
            base.AwakeFromNib();
        }

        public Dropdown(FreenetNode Node) {
            this.Node = Node;

            dropdownMenu = new NSMenu();

            StopTitle = NSBundle.MainBundle.LocalizedString("Stop Freenet", "Button title");
            StartTitle = NSBundle.MainBundle.LocalizedString("Start Freenet", "Button title");
            OpenWebInterfaceTitle = NSBundle.MainBundle.LocalizedString("Open Web Interface", "Button title");
            OpenDownloadsTitle = NSBundle.MainBundle.LocalizedString("Open Downloads", "Button title");
            SettingsTitle = NSBundle.MainBundle.LocalizedString("Settings", "Button title");
            InstallTitle = NSBundle.MainBundle.LocalizedString("Install Freenet", "Button title");
            AboutTitle = NSBundle.MainBundle.LocalizedString("About", "Button title");
            QuitTitle = NSBundle.MainBundle.LocalizedString("Quit", "Button title");

            ToggleNodeStateMenuItem = new NSMenuItem(StartTitle, (sender, e) => {
                ToggleNodeState();
            });

            InstallerMenuItem = new NSMenuItem(InstallTitle, (sender, e) => {
                EventRouter.ShowInstaller(new ShowInstallerEventArgs());
            });

            OpenWebInterfaceMenuItem = new NSMenuItem(OpenWebInterfaceTitle, (sender, e) => {
                EventRouter.ShowFProxy(new ShowFProxyEventArgs());
            });

            OpenDownloadsMenuItem = new NSMenuItem(OpenDownloadsTitle, (sender, e) => {
                EventRouter.ShowDownloads(new ShowDownloadsEventArgs());
            });

            SettingsMenuItem = new NSMenuItem(SettingsTitle, (sender, e) => {
                EventRouter.ShowSettings(new ShowSettingsEventArgs());
            });

            AboutMenuItem = new NSMenuItem(AboutTitle, (sender, e) => {
                EventRouter.ShowAbout(new ShowAboutEventArgs());
            });

            QuitMenuItem = new NSMenuItem(QuitTitle, (sender, e) => {
                NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);
            });

            dropdownMenu.AddItem(ToggleNodeStateMenuItem);
            dropdownMenu.AddItem(OpenWebInterfaceMenuItem);
            dropdownMenu.AddItem(OpenDownloadsMenuItem);
            dropdownMenu.AddItem(NSMenuItem.SeparatorItem);
            dropdownMenu.AddItem(SettingsMenuItem);
            dropdownMenu.AddItem(AboutMenuItem);
            dropdownMenu.AddItem(QuitMenuItem);


            StatusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(30);

            StatusItem.Menu = dropdownMenu;

            HighlightedIcon = NSImage.FromStream(System.IO.File.OpenRead(NSBundle.MainBundle.ResourcePath + @"/highlightedIcon.png"));
            HighlightedIcon.Template = true;

            RunningIcon = NSImage.FromStream(System.IO.File.OpenRead(NSBundle.MainBundle.ResourcePath + @"/runningIcon.png"));
            NotRunningIcon = NSImage.FromStream(System.IO.File.OpenRead(NSBundle.MainBundle.ResourcePath + @"/notRunningIcon.png"));


            StatusItem.Image = NotRunningIcon;
            StatusItem.AlternateImage = HighlightedIcon;

            StatusItem.HighlightMode = true;

            StatusItem.ToolTip = NSBundle.MainBundle.LocalizedString("Freenet", "");

            EventRouter.NodeStateNotRunningEvent += OnNodeStateNotRunningEvent;
            EventRouter.NodeStateRunningEvent += OnNodeStateRunningEvent;

            EventRouter.NodeDataEvent += OnNodeDataEvent;
            EventRouter.NodeHelloEvent += OnNodeHelloEvent;

            EventRouter.NodeLocationFoundEvent += OnNodeLocationFoundEvent;
            EventRouter.NodeLocationNotFoundEvent += OnNodeLocationNotFoundEvent;
        }


        public void EnableMenuItems(bool state) {
            ToggleNodeStateMenuItem.Enabled = state;
            OpenDownloadsMenuItem.Enabled = state;
            OpenWebInterfaceMenuItem.Enabled = state;
        }


        void ToggleNodeState() {
            switch (NodeState) {
                case Constants.FNNodeState.running:
                    EventRouter.StopNode(new StopNodeEventArgs());
                    break;
                case Constants.FNNodeState.notRunning:
                    EventRouter.StartNode(new StartNodeEventArgs());
                    break;
            }
        }

        // MARK: - FNNodeStateProtocol methods

        #region App Events
        void OnNodeStateRunningEvent(object sender, Common.NodeStateRunningEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                NodeState = Constants.FNNodeState.running;
                ToggleNodeStateMenuItem.Title = StopTitle;
                MenuBarImage = RunningIcon;
                EnableMenuItems(true);
            });
        }

        void OnNodeStateNotRunningEvent(object sender, Common.NodeStateNotRunningEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                NodeState = Constants.FNNodeState.notRunning;
                ToggleNodeStateMenuItem.Title = StartTitle;
                MenuBarImage = NotRunningIcon;
                EnableMenuItems(true);
            });
        }

        void OnNodeHelloEvent(object sender, Common.NodeHelloEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {

            });
        }

        void OnNodeDataEvent(object sender, Common.NodeDataEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {

            });
        }

        void OnNodeLocationFoundEvent(object sender, Common.NodeLocationFoundEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                if (dropdownMenu.IndexOfItem(InstallerMenuItem) >= 0) {
                    //dropdownMenu.RemoveItem(InstallerMenuItem);
                }
            });
        }

        void OnNodeLocationNotFoundEvent(object sender, Common.NodeLocationNotFoundEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                if (!(dropdownMenu.IndexOfItem(InstallerMenuItem) >= 0)) {
                    //dropdownMenu.AddItem(InstallerMenuItem);
                }
            });
        }
        #endregion

    }
}
