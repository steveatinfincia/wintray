using System;

namespace Common {
    public static class Constants {

        // MARK: - General constants

        public static string FNWebDomain = "freenetproject.org";
        public static string FNNodeInstallationPathname = "Freenet";
        public static string FNNodeRunscriptPathname = "run.sh";
        public static string FNNodeAnchorFilePathname = "Freenet.anchor";
        public static string FNNodePIDFilePathname = "Freenet.pid";
        public static string FNNodeWrapperConfigFilePathname = "wrapper.conf";
        public static string FNNodeFreenetConfigFilePathname = "freenet.ini";

        public static int FNNodeCheckTimeInterval = 1;

        public static string FNGithubAPI = "api.github.com";

        // MARK: - Deprecated functionality keys

        public static string FNNodeLaunchAgentPathname = "com.freenet.startup.plist";

        // MARK: - Node configuration keys

        public static string FNNodeFreenetConfigFCPBindAddressesKey = "fcp.bindTo";
        public static string FNNodeFreenetConfigFCPPortKey = "fcp.port";
        public static string FNNodeFreenetConfigFProxyBindAddressesKey = "fproxy.bindTo";
        public static string FNNodeFreenetConfigFProxyPortKey = "fproxy.port";

        public static string FNNodeFreenetConfigDownloadsDirKey = "node.downloadsDir";

        // MARK: - NSUserDefaults keys

        public static string FNStartAtLaunchKey = "startatlaunch";

        public static string FNNodeFProxyURLKey = "nodeurl";
        public static string FNNodeFCPURLKey = "nodefcpurl";
        public static string FNNodeInstallationDirectoryKey = "nodepath";
        public static string FNNodeFirstLaunchKey = "firstlaunch";

        public static string FNBrowserPreferenceKey = "FNBrowserPreferenceKey";

        public static string FNEnableNotificationsKey = "FNEnableNotificationsKey";

        // MARK: - Installer

        public static string FNInstallDefaultLocation = "~/Library/Application Support/Freenet";
        public static int FNInstallDefaultFProxyPort = 8888;
        public static int FNInstallDefaultFCPPort = 9481;

        // MARK: - Node state

        public enum FNNodeState {
            unknown = -1,
            notRunning = 0,
            running = 1,
        }

        // MARK: - Installer page

        public enum FNInstallerPage {
            unknown = -1,
            destination = 0,
            progress = 1,
        }

        public enum FNInstallerProgress {
            unknown = -1,
            javaInstalling = 0,
            javaFound = 1,
            copyingFiles = 2,
            copiedFiles = 3,
            configuringNode = 4,
            configuredNode = 5,
            startingNode = 6,
            startedNode = 7,
            finished = 8,
        }

        // MARK: - Blocks

        public delegate void FNGistSuccessBlock(Uri url);
        public delegate void FNGistFailureBlock(string error);
    }
}
