using System;

namespace Common {

    public class NodeDataEventArgs: System.EventArgs {
        public string LastGoodVersion { get; }
        public string Sig { get; }
        public bool Opennet { get; }
        public string Identity { get; }
        public string Version { get; }
        public double Location { get; }

        /// <summary>
        /// NodeDataEventArgs Constructor
        /// </summary>
        internal NodeDataEventArgs(string LastGoodVersion,
                                   string Sig,
                                   bool Opennet,
                                   string Identity,
                                   string Version,
                                   double Location) {
            this.LastGoodVersion = LastGoodVersion;
            this.Sig = Sig;
            this.Opennet = Opennet;
            this.Identity = Identity;
            this.Version = Version;
            this.Location = Location;
        }
    }
}
