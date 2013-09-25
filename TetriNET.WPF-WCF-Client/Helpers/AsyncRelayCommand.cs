using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Action _action;

        public AsyncRelayCommand(Action action)
        {
            _action = action;
        }

        #region ICommand

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        #endregion

        protected virtual async Task ExecuteAsync(object parameter)
        {
            await Task.Run(() => _action());
        }
    }

    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Action<T> _action;

        public AsyncRelayCommand(Action<T> action)
        {
            _action = action;
        }

        #region ICommand

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        #endregion

        protected virtual async Task ExecuteAsync(object parameter)
        {
            await Task.Run(() => _action((T)parameter));
        }
    }

    //public class AsyncRelayCommand2 : ICommand
    //{
    //    protected Func<object, Task> AsyncExecute;

    //    public AsyncRelayCommand2(Func<object, Task> execute)
    //    {
    //        AsyncExecute = execute;
    //    }

    //    #region ICommand

    //    public event EventHandler CanExecuteChanged;

    //    public bool CanExecute(object parameter)
    //    {
    //        return true;
    //    }

    //    public async void Execute(object parameter)
    //    {
    //        await ExecuteAsync(parameter);
    //    }

    //    #endregion

    //    protected virtual async Task ExecuteAsync(object parameter)
    //    {
    //        await AsyncExecute(parameter);
    //    }
    //}
}
