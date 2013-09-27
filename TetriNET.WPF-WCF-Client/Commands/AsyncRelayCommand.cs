using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TetriNET.WPF_WCF_Client.Commands
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
            //await ExecuteAsync(parameter);
            await Task.Run(() => _action());
        }

        #endregion

        //protected virtual async Task ExecuteAsync(object parameter)
        //{
        //    await Task.Run(() => _action());
        //}
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
            //await ExecuteAsync(parameter);
            await Task.Run(() => _action((T)parameter));
        }

        #endregion

        //protected virtual async Task ExecuteAsync(object parameter)
        //{
        //    await Task.Run(() => _action((T)parameter));
        //}
    }
}
