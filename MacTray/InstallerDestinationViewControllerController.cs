using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

using Common;

namespace MacTray {

    [Register("InstallerDestinationViewController")]
    public class InstallerDestinationViewController: NSViewController {

        [Outlet("installPathIndicator")]
        NSPathControl InstallPathIndicator { get; set; }

        NSUrl DefaultLocation { get; set; }

        public InstallationProgressDelegate ProgressDelegate;

        #region Constructors

        // Called when created from unmanaged code
        public InstallerDestinationViewController(IntPtr handle) : base(handle) {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public InstallerDestinationViewController(NSCoder coder) : base(coder) {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public InstallerDestinationViewController(InstallationProgressDelegate ProgressDelegate) : base("InstallerDestinationView", NSBundle.MainBundle) {
            Initialize();
            this.ProgressDelegate = ProgressDelegate;
        }

        // Shared initialization code
        void Initialize() {
            var s = new NSString(Constants.FNInstallDefaultLocation).StandarizePath();

            DefaultLocation = new NSUrl(s, true);
        }

        #endregion

        public override void AwakeFromNib() {
            base.AwakeFromNib();
            InstallPathIndicator.Url = DefaultLocation;
        }


        [Action("selectInstallLocation:")]
        public void SelectInstallLocation(NSObject sender) {

            var panel = new NSOpenPanel() {
                WeakDelegate = this,
                CanChooseFiles = false,
                AllowsMultipleSelection = false,
                CanChooseDirectories = true,
                CanCreateDirectories = true,
                DirectoryUrl = DefaultLocation
            };

            var panelTitle = NSBundle.MainBundle.LocalizedString("Select a location to install Freenet", "Title of window");


            panel.Title = panelTitle;


            var promptString = NSBundle.MainBundle.LocalizedString("Install here", "Button title");


            panel.Prompt = promptString;


            panel.Begin((result) => {
                switch ((NSModalResponse)(int)result) {
                    case NSModalResponse.OK:
                        InstallPathIndicator.Url = panel.Url;
                        ProgressDelegate.UserDidSelectInstallLocation(panel.Url.Path);
                        break;
                    default:
                        break;
                }
            });
        }



        // NSOpenSavePanelDelegate

        [Export("panel:validateURL:error:")]
        public bool ValidateUrl(NSOpenPanel panel, NSUrl url, out NSError outError) {

            if (!MacHelpers.ValidatePossibleInstallationTaget(url.Path, out NSError targetError)) {
                outError = targetError;
                return false;
            }

            outError = new NSError();
            return true;
        }
    }
}
