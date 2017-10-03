using AppKit;
using Foundation;

using Common;

using LetsMove;

namespace MacTray {

    [Register("AppDelegate")]
    public class AppDelegate: NSApplicationDelegate {


        FreenetNode Node;

        Dropdown dropdownMenu;

        SettingsWindowController settingsWindow;
        InstallerWindowController installerWindow;
        AboutWindowController aboutWindow;

        public AppDelegate() {

        }

        public override void DidFinishLaunching(NSNotification notification) {

            var CFBundleVersion = (NSBundle.MainBundle.InfoDictionary["CFBundleVersion"]);

            var defaults = NSUserDefaults.StandardUserDefaults;

            //var markdownURL = NSBundle.MainBundle.GetUrlForResource("Changelog", "md");


            //var data = NSFileManager.DefaultManager.Contents(markdownURL.Path);

            //var markdown = new NSString(data, NSStringEncoding.UTF8);

            LetsMove.LetsMove.PFMoveToApplicationsFolderIfNecessary();

            Node = new FreenetNode();

            MacHelpers.Initialize(Node);

            dropdownMenu = new Dropdown(Node);


            settingsWindow = new SettingsWindowController(Node);

            var a = settingsWindow.Window;

            installerWindow = new InstallerWindowController(Node);

            var b = installerWindow.Window;

            aboutWindow = new AboutWindowController(Node);

            var c = aboutWindow.Window;

            NSUrl nodeURL = MacHelpers.FindNodeInstallation();

            if (nodeURL != null) {
                var standardized = nodeURL.StandardizedUrl;

                var nodePath = standardized.Path;

                defaults.SetString(nodePath, Constants.FNNodeInstallationDirectoryKey);

                EventRouter.NodeLocationFound(new NodeLocationFoundEventArgs(nodePath));

                if (defaults.BoolForKey(Constants.FNStartAtLaunchKey)) {
                    Node.Start();
                }
            } else {
                // no freenet installation found, ask the user what to do
                MacHelpers.DisplayNodeMissingAlert();
            }
        }
    }
}
