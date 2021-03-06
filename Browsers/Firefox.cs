﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;

namespace FreenetTray.Browsers
{
    class Firefox : IBrowser
    {
        /*
         * https://developer.mozilla.org/en-US/docs/Adding_Extensions_using_the_Windows_Registry
         * is out of date as of this writing - it uses "Mozilla Firefox" instead of "Firefox".
         * Earlier versions use HKEY_LOCAL_MACHINE but current ones use HKEY_CURRENT_USER.
         */
        private static readonly string[] VersionRegistryKeys =
        {
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Mozilla\Mozilla Firefox",
            @"HKEY_CURRENT_USER\SOFTWARE\Mozilla\Mozilla Firefox",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Mozilla\Mozilla Firefox",
        };

        private static readonly string[] PathRegistryKeys =
        {
            // CurrentVersion is {VersionNumber} {Locale}. In these keys {VersionNumber}
            // and {CurrentVersion} are replaced before lookup.
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Mozilla\Mozilla Firefox\{VersionNumber}\Main",
            @"HKEY_CURRENT_USER\SOFTWARE\Mozilla\Mozilla Firefox\{CurrentVersion}\Main",
            @"HKEY_CURRENT_USER\SOFTWARE\Mozilla\Mozilla Firefox {VersionNumber}\bin",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Mozilla\Mozilla Firefox {VersionNumber}\bin",
        };

        private readonly bool _isInstalled;
        private readonly Version _version;
        private readonly string _path;

        public Firefox()
        {
            var currentVersion = GetCurrentVersion();
            _version = GetVersion(currentVersion);

            _path = GetPath(currentVersion, _version);
            _isInstalled = _path != null;
        }

        public bool Open(Uri target)
        {
            if (!IsAvailable())
            {
                return false;
            }
            /*
             * Firefox 29 and later support -private-window <URL>:
             *      "Open URL in a new private browsing window."
             *
             * See https://developer.mozilla.org/en-US/docs/Mozilla/Command_Line_Options?redirectlocale=en-US&redirectslug=Command_Line_Options#-private
             */
            Process.Start(_path, "-private-window " + target);
            return true;
        }

        public bool IsAvailable()
        {
            return _isInstalled && _version >= new Version(29, 0);
        }

        public string GetName()
        {
            return "Firefox";
        }

        // Return null if the version cannot be determined.
        private static Version GetVersion(string currentVersion)
        {
            // TODO: Version.TryParse(), added in .NET 4, could make this the only null return.
            if (currentVersion == null)
            {
                return null;
            }

            try
            {
                // CurrentVersion contains "version.number (locale)"
                return new Version(currentVersion.Split(new[] { ' ' }, 2)[0]);
            }
            catch (OverflowException)
            {
            }
            catch (FormatException)
            {
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            return null;
        }

        private static string GetPath(string currentVersion, Version version)
        {
            if (currentVersion == null || version == null)
            {
                return null;
            }

            return PathRegistryKeys
                .Select(key => Registry.GetValue(
                    string.Format(key
                        .Replace("{CurrentVersion}", "{0}")
                        .Replace("{VersionNumber}", "{1}"),
                    currentVersion, version), "PathToExe", null))
                .Where(path => path != null)
                .Cast<string>().FirstOrDefault();
        }

        private static string GetCurrentVersion()
        {
            return VersionRegistryKeys
                .Select(key => Registry.GetValue(key, "CurrentVersion", null))
                .Where(currentVersion => currentVersion != null)
                .Cast<string>().FirstOrDefault();
        }
    }
}
