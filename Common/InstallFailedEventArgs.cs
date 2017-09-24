using System;

namespace Common {

    public class InstallFailedEventArgs: System.EventArgs {
        public string Error { get; }

        /// <summary>
        /// InstallFailedEventArgs Constructor
        /// </summary>
        internal InstallFailedEventArgs(string Error) {
            this.Error = Error;
        }
    }
}
