using System;
using System.Windows.Threading;
using TetriNET.Common.Logger;

namespace TetriNET.WPF_WCF_Client.Helpers
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
            try
            {
                _uiDispatcher.Invoke(action, priority);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Exception raised in ExecuteOnUIThread. {0}", ex);
            }
        }

        public static void InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Render)
        {
            try
            {
                _uiDispatcher.InvokeAsync(action, priority);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Exception raised in ExecuteOnUIThread. {0}", ex);
            }
        }
    }
}
