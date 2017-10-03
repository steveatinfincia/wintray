using Foundation;
using System.Collections.Generic;

using System;
namespace MacTray {
    public interface FCPDataSource {
        Uri NodeFCPUri();
    }

    public interface InstallationProgressDelegate {
        void UserDidSelectInstallLocation(string path);
        void InstallerDidCopyFiles();
        void InstallerDidFinish();
        void InstallerDidFailWithLog(string log);
    }

    public interface InstallerDelegate {
        void InstallerNeedsJava();
        void InstallerFoundJava();

        void InstallerStartedCopyFiles();
        void InstallerDidCopyFiles();

        void InstallerStartedConfigureNode();
        void InstallerDidConfigureNode();

        void InstallerStartingNode();
        void InstallerStartedNode();

        void InstallerDidFinish();
        void InstallerDidFailWithLog(string log);
    }
}
