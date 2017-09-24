using System;

namespace Common {

    public class NodeHelloEventArgs: System.EventArgs {

        public string ConnectionIdentifier { get; }
        public string FcpVersion { get; }
        public string Version { get; }
        public string Node { get; }
        public string NodeLanguage { get; }
        public long ExtBuild { get; }
        public long ExtRevision { get; }
        public long Build { get; }
        public string Revision { get; }
        public bool Testnet { get; }
        public string CompressionCodecs { get; }

        /// <summary>
        /// NodeHelloEventArgs Constructor
        /// </summary>
        public NodeHelloEventArgs(string ConnectionIdentifier,
                                    string FcpVersion,
                                    string Version,
                                    string Node,
                                    string NodeLanguage,
                                    long ExtBuild,
                                    long ExtRevision,
                                    long Build,
                                    string Revision,
                                    bool Testnet,
                                    string CompressionCodecs) {
            this.ConnectionIdentifier = ConnectionIdentifier;
            this.FcpVersion = FcpVersion;
            this.Version = Version;
            this.Node = Node;
            this.NodeLanguage = NodeLanguage;
            this.ExtBuild = ExtBuild;
            this.ExtRevision = ExtRevision;
            this.Build = Build;
            this.Revision = Revision;
            this.Testnet = Testnet;
            this.CompressionCodecs = CompressionCodecs;
        }
    }
}
