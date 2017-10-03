using AppKit;
using Foundation;
using CoreFoundation;
using CoreServices;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Common;





namespace MacTray {
    public static class MacHelpers {

        public static FreenetNode Node { get; set; }

        public static void Initialize(FreenetNode Node) {
            MacHelpers.Node = Node;

            EventRouter.ShowFProxyEvent += OnShowFProxyEvent;
            EventRouter.ShowDownloadsEvent += OnShowDownloadsEvent;
            EventRouter.NodeLocationFoundEvent += OnNodeLocationFoundEvent;
            EventRouter.ShowDesktopNotificationEvent += OnShowDesktopNotificationEvent;

            //NSUserNotificationCenter.DefaultUserNotificationCenter.WeakDelegate = this;
        }

        public static void InstallOracleJRE() {
            var oracleJREPath = NSBundle.MainBundle.PathForResource("jre-9_osx-x64_bin", "dmg");
            NSWorkspace.SharedWorkspace.OpenFile(oracleJREPath);
        }

        public static bool IsEmptyDirectoryAt(string path) {
            try {
                var contents = NSFileManager.DefaultManager.GetDirectoryContent(path, out NSError outError);
                if (contents != null) {
                    return (contents.Count() <= 1);
                }
            } catch {
                return false;
            }
            return false;
        }

        public static void ConfigInvalidAlert(string message, Action continuation) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                var alert = new NSAlert() {
                    MessageText = NSBundle.MainBundle.LocalizedString("Freenet configuration invalid", "Title of window"),
                    InformativeText = message
                };

                alert.AddButton(NSBundle.MainBundle.LocalizedString("OK", "Button title"));

                var result = (NSAlertButtonReturn)(int)alert.RunModal();

                if (result == NSAlertButtonReturn.First) {
                    continuation();
                }
            });
        }

        public static NSUrl FindNodeInstallation() {

            var fileManager = NSFileManager.DefaultManager;

            var applicationSupportURL = fileManager.GetUrls(NSSearchPathDirectory.ApplicationSupportDirectory, NSSearchPathDomain.User)[0];


            var applicationsURL = fileManager.GetUrls(NSSearchPathDirectory.ApplicationDirectory, NSSearchPathDomain.System)[0];

            // existing or user-defined location
            NSUrl customInstallationURL = null;

            var customPath = NSUserDefaults.StandardUserDefaults.StringForKey(Constants.FNNodeInstallationDirectoryKey);
            if (customPath != null) {
                var s = new NSString(customPath).StandarizePath();
                var u = new NSUrl(s, true);
                customInstallationURL = u.StandardizedUrl;

            }

            // new default ~/Library/Application Support/Freenet
            var defaultInstallationURL = applicationSupportURL.Append(Constants.FNNodeInstallationPathname, true);

            // old default /Applications/Freenet
            var deprecatedInstallationURL = applicationsURL.Append(Constants.FNNodeInstallationPathname, true);

            if (customInstallationURL != null && FreenetNode.ValidateNodeInstallationAt(customInstallationURL.Path)) {
                return customInstallationURL;
            } else if (FreenetNode.ValidateNodeInstallationAt(defaultInstallationURL.Path)) {
                return defaultInstallationURL;
            } else if (FreenetNode.ValidateNodeInstallationAt(deprecatedInstallationURL.Path)) {
                return deprecatedInstallationURL;
            }

            return null;
        }

        public static bool ValidatePossibleInstallationTaget(string installationPath, out NSError outError) {
            var fileManager = NSFileManager.DefaultManager;

            // check if the candidate installation path already has a node installed
            if (FreenetNode.ValidateNodeInstallationAt(installationPath)) {
                var errorInfo = new NSMutableDictionary {
                    [NSError.LocalizedDescriptionKey] = new NSString(NSBundle.MainBundle.LocalizedString("Freenet is already installed here", "String informing the user that the selected location is an existing Freenet installation"))
                };
                outError = new NSError(new NSString("org.freenetproject"), 0x1000, errorInfo);
                return false;
            }

            // check if the candidate installation path is actually writable
            if (!fileManager.IsWritableFile(installationPath)) {
                var errorInfo = new NSMutableDictionary {
                    [NSError.LocalizedDescriptionKey] = new NSString(NSBundle.MainBundle.LocalizedString("Cannot install to this directory, write permission denied", "String informing the user that they do not have permission to write to the selected directory"))
                };
                outError = new NSError(new NSString("org.freenetproject"), 0x1001, errorInfo);
                return false;
            }

            // make sure the directory is empty, protects against users accidentally picking their home folder etc
            if (!IsEmptyDirectoryAt(installationPath)) {
                var s = NSBundle.MainBundle.LocalizedString("Directory is not empty", "String informing the user that the selected directory is not empty");

                var errorInfo = new NSMutableDictionary();

                errorInfo[NSError.LocalizedDescriptionKey] = new NSString(s);

                outError = new NSError(new NSString("org.freenetproject"), 0x1002, errorInfo);
                return false;
            }

            outError = null;
            return true;
        }

        public static void DisplayNodeMissingAlert() {
            // no installation found, tell the user to pick a location or start the installer

            DispatchQueue.MainQueue.DispatchAsync(() => {
                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);

                var alert = new NSAlert() {
                    MessageText = NSBundle.MainBundle.LocalizedString("A Freenet installation could not be found.", comment: "String informing the user that no Freenet installation could be found"),
                    InformativeText = NSBundle.MainBundle.LocalizedString("Would you like to install Freenet now, or locate an existing Freenet installation?", comment: "String asking the user whether they would like to install freenet or locate an existing installation")
                };

                alert.AddButton(NSBundle.MainBundle.LocalizedString("Install Freenet", comment: "Button title"));
                alert.AddButton(NSBundle.MainBundle.LocalizedString("Find Installation", comment: "Button title"));
                alert.AddButton(NSBundle.MainBundle.LocalizedString("Quit", comment: ""));

                var response = (NSAlertButtonReturn)(int)alert.RunModal();

                switch (response) {
                    case NSAlertButtonReturn.First:
                        EventRouter.ShowInstaller(new ShowInstallerEventArgs());
                        break;
                    case NSAlertButtonReturn.Second:
                        EventRouter.FindNodeLocation(new FindNodeLocationEventArgs());
                        break;
                    case NSAlertButtonReturn.Third:
                        NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);
                        break;
                    default:
                        break;
                }
            });
        }

        public static void DisplayUninstallAlert() {
            // ask the user if they really do want to uninstall Freenet
            DispatchQueue.MainQueue.DispatchAsync(() => {
                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);

                var alert = new NSAlert() {
                    MessageText = NSBundle.MainBundle.LocalizedString("Uninstall Freenet now?", comment: "Title of window"),
                    InformativeText = NSBundle.MainBundle.LocalizedString("Uninstalling Freenet is immediate and irreversible, are you sure you want to uninstall Freenet now?", comment: "String asking the user whether they would like to uninstall freenet")
                };

                alert.AddButton(NSBundle.MainBundle.LocalizedString("Uninstall Freenet", comment: "Button title"));
                alert.AddButton(NSBundle.MainBundle.LocalizedString("Cancel", comment: "Button title"));

                var response = (NSAlertButtonReturn)(int)alert.RunModal();

                switch (response) {
                    case NSAlertButtonReturn.First:
                        EventRouter.ShowUninstaller(new ShowUninstallerEventArgs());
                        break;
                    default:
                        break;
                }
            });
        }

        public static List<Browser> GetInstalledWebBrowsers() {
            var url = new NSUrl("https://");

            var appUrls = LaunchServices.GetApplicationUrlsForUrl(url, LSRoles.Viewer);

            if (appUrls != null) {
                // Extract the app names and sort them for prettiness.
                var appNames = new List<Browser>();

                var urls = appUrls.GetEnumerator();

                List<NSUrl> urlList = new List<NSUrl>(appUrls);

                foreach (var nurl in urlList) {
                    appNames.Add(new Browser(url.Path));
                }

                return appNames;
            }
            return null;
        }

        static void OnShowFProxyEvent(object sender, ShowFProxyEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                var fproxyLocation = Node.FProxyLocation;
                if (fproxyLocation != null) {
                    // Open the fproxy page in users default browser
                    NSWorkspace.SharedWorkspace.OpenUrl(fproxyLocation);
                }
            });
        }

        static void OnShowDownloadsEvent(object sender, ShowDownloadsEventArgs e) {
            if (Node.DownloadsFolder != null) {
                var u = new NSUrl(Node.DownloadsFolder, true);

                NSWorkspace.SharedWorkspace.OpenUrl(u);
            }
        }


        static void OnShowDesktopNotificationEvent(object sender, DesktopNotificationEventArgs e) {
            if (!NSUserDefaults.StandardUserDefaults.BoolForKey(Constants.FNEnableNotificationsKey)) {
                return;
            }


            // N2N messages are handled differently, we want to grab the substring of the response that contains the
            // actual message since display space is limited in the notification popups

            /*
            if (nodeUserAlert.Headers.ContainsKey("TextFeed")) {
                notification.Title = nodeUserAlert.Headers["ShortText"] as string;

                var textLength = nodeUserAlert.Headers["TextLength"];

                var messageLength = nodeUserAlert.Headers["MessageTextLength"] as string;

                var messageData = nodeUserAlert.Headers["Data"] as string;

                var start = Convert.ToInt16(textLength);
                var length = Convert.ToInt16(messageLength);

                var message = messageData.Substring((start - length - 1), (length + 1));    // play

                notification.InformativeText = message;
            } else {
                notification.Title = nodeUserAlert.Headers["Header"] as string;
                notification.InformativeText = nodeUserAlert.Headers["Data"] as string;
            }
            */
            var notification = new NSUserNotification() {
                Title = e.Title,
                InformativeText = e.Body,
                SoundName = NSUserNotification.NSUserNotificationDefaultSoundName
            };
            NSUserNotificationCenter.DefaultUserNotificationCenter.DeliverNotification(notification);
        }

        static void OnNodeLocationFoundEvent(object sender, NodeLocationFoundEventArgs e) {
            NSUserDefaults.StandardUserDefaults.SetString(e.Location, Constants.FNNodeInstallationDirectoryKey);
        }

        public static bool IsJavaInstalled() {

            var processInfo = new ProcessStartInfo() {
                FileName = "/usr/sbin/pkgutil",
                Arguments = "--pkgs=com.oracle.jre",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(processInfo);
            process.EnableRaisingEvents = true;
            process.WaitForExit();

            if (process.ExitCode == 0) {
                return true;
            }
            return false;

        }
    }
}
