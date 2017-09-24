using System;

namespace Common {

    public class NodeLocationFoundEventArgs: System.EventArgs {
        public string Location { get; }

        /// <summary>
        /// NodeLocationFoundEventArgs Constructor
        /// </summary>
        internal NodeLocationFoundEventArgs(string Location) {
            this.Location = Location;
        }
    }
}
