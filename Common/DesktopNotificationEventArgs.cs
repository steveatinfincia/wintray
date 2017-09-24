using System;

namespace Common {

    public class DesktopNotificationEventArgs: System.EventArgs {
        public string Title { get; }
        public string Body { get; }

        /// <summary>
        /// DesktopNotificationEventArgs Constructor
        /// </summary>
        internal DesktopNotificationEventArgs(string Title, string Body) {
            this.Title = Title;
            this.Body = Body;
        }
    }
}
