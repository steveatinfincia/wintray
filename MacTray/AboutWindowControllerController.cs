using System;

using Foundation;
using AppKit;
using CoreFoundation;
using CoreAnimation;
using CoreGraphics;

using Markdig;

using Common;


namespace MacTray {
    [Register("AboutWindowController")]
    public partial class AboutWindowController: NSWindowController {

        FreenetNode Node { get; set; }


        [Outlet("mainView")]
        NSView MainView { get; set; }

        [Outlet("name")]
        NSString Name { get; set; }

        [Outlet("nameView")]
        NSTextField NameView { get; set; }

        [Outlet("versionView")]
        NSTextField VersionView { get; set; }

        [Outlet("version")]
        NSString Version { get; set; }

        [Outlet("license")]
        NSAttributedString License { get; set; }

        [Outlet("licenseTextView")]
        WebKit.WebView LicenseView { get; set; }

        [Outlet("openWebsiteButton")]
        NSButton VisitWebsiteButton { get; set; }

        public AboutWindowController(IntPtr handle) : base(handle) {
            Console.Out.Write("19");
        }

        [Export("initWithCoder:")]
        public AboutWindowController(NSCoder coder) : base(coder) {
            Console.Out.Write("9");
        }

        public AboutWindowController(FreenetNode Node) : base("AboutWindow") {
            this.Node = Node;
        }

        public override void AwakeFromNib() {
            base.AwakeFromNib();


            // Enable layer backing and change the background color
            MainView.WantsLayer = true;
            MainView.Layer.BackgroundColor = NSColor.White.CGColor;

            // Add bottom border
            CALayer bottomBorder = new CALayer();
            bottomBorder.BorderColor = NSColor.Gray.CGColor;
            bottomBorder.BorderWidth = 1;
            bottomBorder.Frame = new CGRect(-1.0, 0.0, MainView.Frame.Width + 2.0, MainView.Frame.Height + 1.0);
            bottomBorder.AutoresizingMask = CAAutoresizingMask.HeightSizable | CAAutoresizingMask.WidthSizable;


            MainView.Layer.AddSublayer(bottomBorder);

            NSDictionary BundleDict = NSBundle.MainBundle.InfoDictionary;

            NSString shortVersion = (Foundation.NSString)BundleDict.ObjectForKey(new NSString("CFBundleShortVersionString"));
            NSString version = (Foundation.NSString)BundleDict.ObjectForKey(new NSString("CFBundleVersion"));

            Name = new NSString("FreenetTray for Mac");
            NameView.StringValue = Name;

            Version = new NSString($"{shortVersion} ({version})");
            VersionView.StringValue = Version;

            VisitWebsiteButton.Title = NSBundle.MainBundle.LocalizedString(@"Open Freenet Website", "button title");

            string LicensePath = NSBundle.MainBundle.PathForResource("license", "md");

            if (LicensePath != null) {
                var licenseData = System.IO.File.ReadAllText(LicensePath);

                var licenseHtml = Markdown.ToHtml(licenseData);

                var styleString = "" +
                    "<style>body { " +
                    "" +
                    "    font-family: system-ui, HelveticaNeue, Sans Serif;" +
                    "}" +
                    "</style>";

                Console.WriteLine(licenseHtml);   // prints: <p>This is a text with some <em>emphasis</em></p>


                LicenseView.MainFrame.LoadHtmlString(styleString + licenseHtml, new NSUrl("https://127.0.0.1:8888"));
            }

            EventRouter.ShowAboutEvent += OnShowAboutEvent;

            Console.Out.WriteLine("About window configured");
        }

        [Action("openWebsite:")]
        public void OpenWebsite(NSObject sender) {
            NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl($"https://{Constants.FNWebDomain}"));
        }

        void OnShowAboutEvent(object sender, Common.ShowAboutEventArgs e) {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                ShowWindow(this);
                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            });
        }
    }
}
