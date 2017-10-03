using System;


using Foundation;
using AppKit;
using CoreFoundation;

using Common;

using FontAwesomeIconFactory;

namespace MacTray {

    [Register("InstallerProgressViewController")]
    public partial class InstallerProgressViewController: NSViewController {
        #region properties

        [Outlet("javaInstallationTitle")]
        NSTextField JavaInstallationTitle { get; set; }

        [Outlet("javaInstallationStatus")]
        NIKFontAwesomeImageView JavaInstallationStatus { get; set; }

        [Outlet("fileCopyTitle")]
        NSTextField FileCopyTitle { get; set; }

        [Outlet("fileCopyStatus")]
        NIKFontAwesomeImageView FileCopyStatus { get; set; }

        [Outlet("configurationTitle")]
        NSTextField ConfigurationTitle { get; set; }

        [Outlet("configurationStatus")]
        NIKFontAwesomeImageView ConfigurationStatus { get; set; }

        [Outlet("startNodeTitle")]
        NSTextField StartNodeTitle { get; set; }

        [Outlet("startNodeStatus")]
        NIKFontAwesomeImageView StartNodeStatus { get; set; }

        [Outlet("finishedTitle")]
        NSTextField FinishedTitle { get; set; }

        [Outlet("finishedStatus")]
        NIKFontAwesomeImageView FinishedStatus { get; set; }

        bool JavaPromptShown { get; set; }

        Installer FreenetInstaller { get; set; }

        public InstallationProgressDelegate ProgressDelegate { get; set; }
        #endregion

        #region Constructors

        // Called when created from unmanaged code
        public InstallerProgressViewController(IntPtr handle) : base(handle) {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public InstallerProgressViewController(NSCoder coder) : base(coder) {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public InstallerProgressViewController(InstallationProgressDelegate ProgressDelegate) : base("InstallerProgressView", NSBundle.MainBundle) {
            Initialize();
            this.ProgressDelegate = ProgressDelegate;
        }

        // Shared initialization code
        void Initialize() {
            JavaPromptShown = false;

            FreenetInstaller = new Installer(this);
        }

        #endregion

        public override void AwakeFromNib() {
            base.AwakeFromNib();

            UpdateProgress(Constants.FNInstallerProgress.unknown);
        }

        public void InstallNodeAt(string installLocation) {
            FreenetInstaller.InstallNodeAt(installLocation);
        }

        #region Internal state
        // MARK: - Internal state

        void UpdateProgress(Constants.FNInstallerProgress progress) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                var factory = new NIKFontAwesomeIconFactory();

                if (progress >= Constants.FNInstallerProgress.finished) {
                    FinishedStatus.Image = factory.CreateImageForIcon(NIKFontAwesomeIcon.NIKFontAwesomeIconCheckCircle);
                    FinishedStatus.Hidden = false;
                    FinishedTitle.Hidden = false;
                }

                if (progress >= Constants.FNInstallerProgress.startedNode) {
                    StartNodeStatus.Image = factory.CreateImageForIcon(NIKFontAwesomeIcon.NIKFontAwesomeIconCheckCircle);
                    StartNodeStatus.Hidden = false;
                    StartNodeTitle.Hidden = false;
                }


                if (progress >= Constants.FNInstallerProgress.startingNode) {
                    StartNodeStatus.Image = factory.CreateImageForIcon(NIKFontAwesomeIcon.NIKFontAwesomeIconClockO);
                    StartNodeStatus.Hidden = false;
                    StartNodeTitle.Hidden = false;
                }

                if (progress >= Constants.FNInstallerProgress.configuredNode) {
                    ConfigurationStatus.Image = factory.CreateImageForIcon(NIKFontAwesomeIcon.NIKFontAwesomeIconCheckCircle);
                    ConfigurationStatus.Hidden = false;
                    ConfigurationTitle.Hidden = false;
                }

                if (progress >= Constants.FNInstallerProgress.configuringNode) {
                    ConfigurationStatus.Image = factory.CreateImageForIcon(NIKFontAwesomeIcon.NIKFontAwesomeIconClockO);
                    ConfigurationStatus.Hidden = false;
                    ConfigurationTitle.Hidden = false;
                }

                if (progress >= Constants.FNInstallerProgress.copiedFiles) {
                    FileCopyStatus.Image = factory.CreateImageForIcon(NIKFontAwesomeIcon.NIKFontAwesomeIconCheckCircle);
                    FileCopyStatus.Hidden = false;
                    FileCopyTitle.Hidden = false;
                }

                if (progress >= Constants.FNInstallerProgress.copyingFiles) {
                    FileCopyStatus.Image = factory.CreateImageForIcon(NIKFontAwesomeIcon.NIKFontAwesomeIconClockO);
                    FileCopyStatus.Hidden = false;
                    FileCopyTitle.Hidden = false;
                }

                if (progress >= Constants.FNInstallerProgress.javaFound) {
                    JavaInstallationStatus.Image = factory.CreateImageForIcon(NIKFontAwesomeIcon.NIKFontAwesomeIconCheckCircle);
                    JavaInstallationStatus.Hidden = false;
                    JavaInstallationTitle.Hidden = false;
                }

                if (progress >= Constants.FNInstallerProgress.javaInstalling) {
                    JavaInstallationStatus.Image = factory.CreateImageForIcon(NIKFontAwesomeIcon.NIKFontAwesomeIconCheckCircle);
                    JavaInstallationStatus.Hidden = false;
                    JavaInstallationTitle.Hidden = false;
                }
            });
        }
        #endregion


        #region Java handling

        void PromptForJavaInstallation() {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                var installJavaAlert = new NSAlert() {
                    MessageText = NSBundle.MainBundle.LocalizedString("Java not found", "String informing the user that Java was not found"),
                    InformativeText = NSBundle.MainBundle.LocalizedString("Freenet requires Java, would you like to install it now?", "String asking the user if they would like to install Java")
                };

                installJavaAlert.AddButton(NSBundle.MainBundle.LocalizedString("Install Java", "Button title"));
                installJavaAlert.AddButton(NSBundle.MainBundle.LocalizedString("Quit", ""));

                var response = (NSAlertButtonReturn)(int)installJavaAlert.RunModal();

                switch (response) {
                    case NSAlertButtonReturn.First:
                        MacHelpers.InstallOracleJRE();
                        break;
                    default:
                        NSApplication.SharedApplication.Terminate(this);
                        break;
                }
            });
        }
        #endregion
    }

    partial class InstallerProgressViewController: InstallerDelegate {

        public void InstallerNeedsJava() {
            // don't repeatedly prompt the user to install Java
            if (!JavaPromptShown) {
                JavaPromptShown = true;

                PromptForJavaInstallation();

                UpdateProgress(Constants.FNInstallerProgress.javaInstalling);
            }
        }

        public void InstallerFoundJava() {
            UpdateProgress(Constants.FNInstallerProgress.javaFound);
        }

        public void InstallerStartedCopyFiles() {
            UpdateProgress(Constants.FNInstallerProgress.copyingFiles);
        }

        public void InstallerDidCopyFiles() {
            UpdateProgress(Constants.FNInstallerProgress.copiedFiles);
        }

        public void InstallerStartedConfigureNode() {
            UpdateProgress(Constants.FNInstallerProgress.configuringNode);
        }

        public void InstallerDidConfigureNode() {
            UpdateProgress(Constants.FNInstallerProgress.configuredNode);
        }

        public void InstallerStartingNode() {
            UpdateProgress(Constants.FNInstallerProgress.startingNode);
        }

        public void InstallerStartedNode() {
            UpdateProgress(Constants.FNInstallerProgress.startedNode);
        }

        public void InstallerDidFinish() {
            UpdateProgress(Constants.FNInstallerProgress.finished);

            DispatchQueue.MainQueue.DispatchAsync(() => {
                ProgressDelegate.InstallerDidFinish();
            });
        }

        public void InstallerDidFailWithLog(string log) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                ProgressDelegate.InstallerDidFailWithLog(log);
            });
        }
    }
}
