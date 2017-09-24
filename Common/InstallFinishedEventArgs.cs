using System;

namespace Common {

    public class InstallFinishedEventArgs: System.EventArgs {
        public string Location { get; }

        /// <summary>
        /// InstallFinishedEventArgs Constructor
        /// </summary>
        internal InstallFinishedEventArgs(string Location) {
            this.Location = Location;
        }
    }
}
