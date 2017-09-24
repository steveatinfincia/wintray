using System;
using System.Threading;

namespace Common {
    public static class EventRouter {

        static bool isMultiThreaded = true;

        #region Events
        public static event EventHandler<NodeStateRunningEventArgs> NodeStateRunningEvent;
        public static event EventHandler<NodeStateNotRunningEventArgs> NodeStateNotRunningEvent;
        public static event EventHandler<NodeStateUnknownEventArgs> NodeStateUnknownEvent;

        public static event EventHandler<NodeDataEventArgs> NodeDataEvent;
        public static event EventHandler<NodeHelloEventArgs> NodeHelloEvent;

        public static event EventHandler<NodeConfiguredEventArgs> NodeConfiguredEvent;

        public static event EventHandler<StartNodeEventArgs> StartNodeEvent;
        public static event EventHandler<StopNodeEventArgs> StopNodeEvent;

        public static event EventHandler<InstallFinishedEventArgs> InstallFinishedEvent;
        public static event EventHandler<InstallFailedEventArgs> InstallFailedEvent;

        public static event EventHandler<ShowFProxyEventArgs> ShowFProxyEvent;
        public static event EventHandler<ShowDownloadsEventArgs> ShowDownloadsEvent;

        public static event EventHandler<ShowSettingsEventArgs> ShowSettingsEvent;
        public static event EventHandler<ShowAboutEventArgs> ShowAboutEvent;
        public static event EventHandler<ShowInstallerEventArgs> ShowInstallerEvent;
        public static event EventHandler<ShowUninstallerEventArgs> ShowUninstallerEvent;

        public static event EventHandler<DesktopNotificationEventArgs> ShowDesktopNotificationEvent;

        public static event EventHandler<FindNodeLocationEventArgs> FindNodeLocationEvent;
        public static event EventHandler<NodeLocationFoundEventArgs> NodeLocationFoundEvent;
        public static event EventHandler<NodeLocationNotFoundEventArgs> NodeLocationNotFoundEvent;


        #endregion

        public static void DispatchEvent<T>(EventHandler<T> handler, T eventArgs) where T : System.EventArgs {
            if (handler == null) return;

            if (isMultiThreaded) {
                ThreadPool.QueueUserWorkItem(state => handler.Invoke(null, eventArgs));
            } else {
                handler(null, eventArgs);
            }
        }

        public static void NodeStateRunning(NodeStateRunningEventArgs args) {
            DispatchEvent(NodeStateRunningEvent, args);
        }

        public static void NodeStateNotRunning(NodeStateNotRunningEventArgs args) {
            DispatchEvent(NodeStateNotRunningEvent, args);
        }

        public static void NodeStateUnknown(NodeStateUnknownEventArgs args) {
            DispatchEvent(NodeStateUnknownEvent, args);
        }

        public static void NodeHello(NodeHelloEventArgs args) {
            DispatchEvent(NodeHelloEvent, args);
        }

        public static void NodeData(NodeDataEventArgs args) {
            DispatchEvent(NodeDataEvent, args);
        }

        public static void StartNode(StartNodeEventArgs args) {
            DispatchEvent(StartNodeEvent, args);
        }

        public static void StopNode(StopNodeEventArgs args) {
            DispatchEvent(StopNodeEvent, args);
        }

        public static void NodeConfigured(NodeConfiguredEventArgs args) {
            DispatchEvent(NodeConfiguredEvent, args);
        }

        public static void InstallFinished(InstallFinishedEventArgs args) {
            DispatchEvent(InstallFinishedEvent, args);
        }

        public static void InstallFailed(InstallFailedEventArgs args) {
            DispatchEvent(InstallFailedEvent, args);
        }

        public static void ShowSettings(ShowSettingsEventArgs args) {
            DispatchEvent(ShowSettingsEvent, args);
        }

        public static void ShowFProxy(ShowFProxyEventArgs args) {
            DispatchEvent(ShowFProxyEvent, args);
        }

        public static void ShowDownloads(ShowDownloadsEventArgs args) {
            DispatchEvent(ShowDownloadsEvent, args);
        }

        public static void ShowAbout(ShowAboutEventArgs args) {
            DispatchEvent(ShowAboutEvent, args);
        }

        public static void ShowInstaller(ShowInstallerEventArgs args) {
            DispatchEvent(ShowInstallerEvent, args);
        }

        public static void ShowUninstaller(ShowUninstallerEventArgs args) {
            DispatchEvent(ShowUninstallerEvent, args);
        }

        public static void ShowDesktopNotification(DesktopNotificationEventArgs args) {
            DispatchEvent(ShowDesktopNotificationEvent, args);
        }

        public static void FindNodeLocation(FindNodeLocationEventArgs args) {
            DispatchEvent(FindNodeLocationEvent, args);
        }

        public static void NodeLocationFound(NodeLocationFoundEventArgs args) {
            DispatchEvent(NodeLocationFoundEvent, args);
        }

        public static void NodeLocationNotFound(NodeLocationNotFoundEventArgs args) {
            DispatchEvent(NodeLocationNotFoundEvent, args);
        }
    }
}
