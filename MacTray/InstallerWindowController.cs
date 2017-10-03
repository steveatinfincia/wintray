using AppKit;
using Foundation;

using System;
using System.Collections.Generic;

using Common;

using CoreFoundation;

namespace MacTray {
    [Register("InstallerWindowController")]
    public partial class InstallerWindowController: NSWindowController {
        FreenetNode Node;

        [Outlet("backButton")]
        NSButton BackButton { get; set; }

        [Outlet("nextButton")]
        NSButton NextButton { get; set; }

        [Outlet("pageController")]
        NSPageController PageController { get; set; }

        [Outlet("installationProgressIndicator")]
        NSProgressIndicator InstallationProgressIndicator { get; set; }

        InstallerDestinationViewController DestinationViewControler;
        InstallerProgressViewController ProgressViewController;

        string SelectedInstallPath;

        bool InstallationInProgress;
        bool InstallationFinished;

        public InstallerWindowController(FreenetNode Node) : base("InstallerWindow") {
            this.Node = Node;

            SelectedInstallPath = new NSString(Constants.FNInstallDefaultLocation).StandarizePath();
        }

        public override void AwakeFromNib() {
            base.AwakeFromNib();
        }

        public override void WindowDidLoad() {
            base.WindowDidLoad();

            Window.Delegate = this;

            DestinationViewControler = new InstallerDestinationViewController(this);
            ProgressViewController = new InstallerProgressViewController(this);

            PageController.Delegate = this;

            var pageIdentifiers = new List<NSString> {
                new NSString("InstallerDestinationViewController"),
                new NSString("InstallerProgressViewController")
            };

            PageController.ArrangedObjects = pageIdentifiers.ToArray();

            PageController.SelectedIndex = (int)Constants.FNInstallerPage.destination;

            ConfigureMainWindow();

            EventRouter.ShowInstallerEvent += OnShowInstallerEvent;
        }

        // MARK: FNInstallerNotification

        void OnShowInstallerEvent(object sender, Common.ShowInstallerEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                ShowWindow(this);
                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            });
        }

        [Action("next:")]
        public void Next(NSObject sender) {
            if (InstallationFinished) {
                var fproxyLocation = Node.FProxyLocation;
                if (fproxyLocation != null) {
                    NSWorkspace.SharedWorkspace.OpenUrl(fproxyLocation);
                }
                Window.Close();
                return;
            }

            if (!MacHelpers.ValidatePossibleInstallationTaget(SelectedInstallPath, out NSError targetError)) {
                var alert = new NSAlert() {
                    MessageText = targetError.LocalizedDescription,
                    AlertStyle = NSAlertStyle.Critical
                };

                alert.AddButton(NSBundle.MainBundle.LocalizedString("OK", comment: "Button title"));

                var response = (NSAlertButtonReturn)(int)alert.RunModal();

                switch (response) {
                    case NSAlertButtonReturn.First:
                        return;
                    default:
                        return;
                }
            }



            PageController.NavigateForward(sender);

            ConfigureMainWindow();
        }

        [Action("previous:")]
        public void Previous(NSObject sender) {
            //assert(Thread.current == Thread.main, "NOT MAIN THREAD")

            PageController.NavigateBack(sender);

            ConfigureMainWindow();
        }

        public void ConfigureMainWindow() {
            //assert(Thread.current == Thread.main, "NOT MAIN THREAD")

            if (PageController.SelectedIndex == (int)Constants.FNInstallerPage.progress) {
                if (InstallationInProgress) {
                    NextButton.Enabled = false;
                    BackButton.Enabled = false;
                } else if (this.InstallationFinished) {
                    NextButton.Enabled = true;
                    BackButton.Enabled = false;
                    InstallationProgressIndicator.DoubleValue = InstallationProgressIndicator.MaxValue;
                    return;
                }
            } else {
                NextButton.Enabled = PageController.SelectedIndex < PageController.ArrangedObjects.Length - 1 ? true : false;

                BackButton.Enabled = PageController.SelectedIndex > 0 ? true : false;
            }

            InstallationProgressIndicator.MinValue = 0;

            InstallationProgressIndicator.MaxValue = PageController.ArrangedObjects.Length;

            InstallationProgressIndicator.DoubleValue = PageController.SelectedIndex;
        }
    }

    public partial class InstallerWindowController: INSWindowDelegate {
        [Action("windowShouldClose:")]
        public bool WindowShouldClose(NSObject sender) {
            if (InstallationInProgress) {
                var installInProgressAlert = new NSAlert() {
                    MessageText = NSBundle.MainBundle.LocalizedString("Installation in progress", "String informing the user that an installation is in progress"),
                    InformativeText = NSBundle.MainBundle.LocalizedString("Are you sure you want to cancel?", "String asking the user if they want to cancel the installation")
                };

                installInProgressAlert.AddButton(NSBundle.MainBundle.LocalizedString("Yes", "Button title"));

                installInProgressAlert.AddButton(NSBundle.MainBundle.LocalizedString("No", "Button title"));

                var response = (NSAlertButtonReturn)(int)installInProgressAlert.RunModal();

                switch (response) {
                    case NSAlertButtonReturn.First: {
                            NSApplication.SharedApplication.Terminate(this);
                            break;
                        }
                    default:
                        break;
                }
            }
            return !InstallationInProgress;
        }
    }

    public partial class InstallerWindowController: INSPageControllerDelegate {
        [Action("pageController:identifierForObject:")]
        public string GetIdentifier(NSPageController pageController, NSObject o) {
            var s = o as NSString;

            return s.ToString();
        }



        [Action("pageController:viewControllerForIdentifier:")]
        public NSViewController GetViewController(NSPageController pageController, NSString identifier) {
            if (identifier == "InstallerDestinationViewController") {
                return DestinationViewControler;
            } else if (identifier == "InstallerProgressViewController") {
                return ProgressViewController;
            }
            return new NSViewController(); // should never reach this point, silencing compiler 
        }

        [Action("pageController:prepareViewController:withObject:")]
        public void PrepareViewController(NSPageController pageController, NSViewController viewController, NSObject o) {
            //viewController.RepresentedObject = o;
        }

        [Action("pageController:didTransitionToObject:")]
        public void DidTransition(NSPageController pageController, NSObject o) {
            ConfigureMainWindow();
        }

        [Action("pageControllerDidEndLiveTransition:")]
        public void DidEndLiveTransition(NSPageController pageController) {
            PageController.CompleteTransition();


            if (PageController.SelectedIndex == (int)Constants.FNInstallerPage.progress) {

                if (PageController.SelectedViewController is InstallerProgressViewController vc) {
                    vc.InstallNodeAt(SelectedInstallPath);
                }

                InstallationInProgress = true;
                ConfigureMainWindow();
            }
        }
    }

    public partial class InstallerWindowController: InstallationProgressDelegate {
        public void UserDidSelectInstallLocation(string path) {
            SelectedInstallPath = path;
            ConfigureMainWindow();
        }

        public void InstallerDidCopyFiles() {
            InstallationFinished = false;
            InstallationInProgress = false;
            ConfigureMainWindow();
        }

        public void InstallerDidFinish() {
            InstallationFinished = true;
            InstallationInProgress = false;
            ConfigureMainWindow();
            EventRouter.InstallFinished(new InstallFinishedEventArgs(SelectedInstallPath));
        }

        public void InstallerDidFailWithLog(string log) {
            InstallationFinished = false;
            InstallationInProgress = false;
            ConfigureMainWindow();

            EventRouter.InstallFailed(new InstallFailedEventArgs(log));


            var installFailedAlert = new NSAlert() {
                MessageText = NSBundle.MainBundle.LocalizedString("Installation failed", "String informing the user that the installation failed"),
                InformativeText = NSBundle.MainBundle.LocalizedString("The installation log can be automatically uploaded to GitHub. Please report this failure to the Freenet developers and provide the GitHub link to them.", "String asking the user to provide the Gist link to the Freenet developers")
            };

            installFailedAlert.AddButton(NSBundle.MainBundle.LocalizedString("Upload", "Button title"));

            installFailedAlert.AddButton(NSBundle.MainBundle.LocalizedString("Quit", ""));


            var response = (NSAlertButtonReturn)(int)installFailedAlert.RunModal();

            switch (response) {
                case NSAlertButtonReturn.First:
                    Common.Github.Gist.Create(log, "Installation Log", (Common.Github.Gist gist) => {
                        var pasteBoard = NSPasteboard.GeneralPasteboard;

                        var types = new List<string>() {
                            NSPasteboard.NSPasteboardTypeString
                        };

                        pasteBoard.DeclareTypes(types.ToArray(), null);

                        pasteBoard.SetStringForType(gist.Location, NSPasteboard.NSPasteboardTypeString);

                        NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(gist.Location));


                        NSApplication.SharedApplication.Terminate(this);
                    }, (string error) => {
                        var fileManager = NSFileManager.DefaultManager;

                        var desktop = fileManager.GetUrls(NSSearchPathDirectory.DesktopDirectory, NSSearchPathDomain.User)[0];

                        var url = desktop.Append("FreenetTray - Installation Log.txt", false);

                        var logBuffer = NSData.FromString(log, NSStringEncoding.UTF8);

                        if (logBuffer != null) {

                            try {
                                System.IO.File.WriteAllText(url.Path, log);
                            } catch (Exception ex) {
                                // best effort, if we can't write to the log file there's nothing else we can do
                                var e = ex;
                            }
                        }

                        var uploadFailedAlert = new NSAlert() {
                            MessageText = NSBundle.MainBundle.LocalizedString($"Upload failed: {error}", "String informing the user that the upload failed"),
                            InformativeText = NSBundle.MainBundle.LocalizedString("The installation log could not be uploaded to GitHub, it has been placed on your desktop instead. Please report this failure to the Freenet developers and provide the file to them.", "String informing the user that the log upload failed")
                        };
                        var rr = uploadFailedAlert.RunModal();
                    });
                    break;
                default:
                    NSApplication.SharedApplication.Terminate(this);
                    break;
            }
        }
    }
}
