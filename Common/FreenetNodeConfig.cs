using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Common.Node {
    public static class Config {

        public static Dictionary<string, string> From(string path) {
            try {
                var configContents = System.IO.File.ReadAllText(path);
                if (configContents == null) {
                    return null;
                }
                return ParseKeyValueString(configContents);
            } catch {
                // best effort, if we can't write to the log file there's nothing else we can do
                return null;
            }
        }

        static Dictionary<string, string> ParseKeyValueString(string s) {
            var config = new Dictionary<string, string>();
            Regex rx = new Regex(@"^\s*(.+?)\s*=\s*(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


            foreach (string line in s.ToString().Split()) {
                MatchCollection matches = rx.Matches(line);

                foreach (Match match in matches) {
                    GroupCollection groups = match.Groups;

                    if (groups.Count == 0) {
                        continue;
                    }

                    if (groups.Count < 3) {
                        continue;
                    }

                    var key = groups[1].Value;
                    var val = groups[2].Value;

                    config[key] = val;
                }
            }

            return config;
        }
    }
}
