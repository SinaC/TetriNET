using System;
using System.Windows.Threading;

namespace TetriNET.WPF_WCF_Client
{
    public static class ExecuteOnUIThread
    {
        private static Dispatcher _uiDispatcher;

        public static void Initialize()
        {
            _uiDispatcher = Dispatcher.CurrentDispatcher;
        }

        public static void Invoke(Action action, DispatcherPriority priority = DispatcherPriority.Render)
        {
            _uiDispatcher.Invoke(action, priority);
        }

        public static void InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Render)
        {
            _uiDispatcher.InvokeAsync(action, priority);
        }
    }
}
