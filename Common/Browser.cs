using AppKit;
using Foundation;
using CoreFoundation;

using System;

namespace MacTray {
#if __MACOS__
    public class Browser: NSObject {
#else
    public class Browser {
#endif
        #region Properties
        string BrowserPath { get; set; }

        string ExecutablePath { get; set; }

        string Name { get; set; }

#if __MACOS__
        NSImage Icon { get; set; }
#else
#endif

        #endregion

        #region Private Browsing Flags
#if __MACOS__
        string PrivateBrowsingFlag() {
            switch (Name) {
                case "Firefox":
                    return "--private";
                case "Chrome:":
                    return "--incognito";
                case "Opera":
                    return "--newprivatetab";
                case "Safari":
                    return "";
                default:
                    return "";
            }
        }

#else
#endif


        #endregion


        #region Constructors

        public Browser(string browserPath) {
            this.BrowserPath = browserPath;
#if __MACOS__
            NSBundle bundle = new NSBundle(browserPath);
            ExecutablePath = bundle.ExecutablePath;
            Name = bundle.ObjectForInfoDictionary(new NSString("CFBundleDisplayName")) as NSString;
            Icon = NSWorkspace.SharedWorkspace.IconForFile(BrowserPath);
#else
#endif
        }
        #endregion

#if __MACOS__
        public override string Description {
            get {
                return Name;
            }
        }

        public override string DebugDescription {
            get {
                return $"<{Name}>: {ExecutablePath}";
            }
        }
#else
#endif
    }
}
