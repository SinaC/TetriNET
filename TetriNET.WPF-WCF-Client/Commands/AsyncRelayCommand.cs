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
            if (_action != null)
                await Task.Run(() => _action());
        }

        #endregion
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
            if (_action != null)
                await Task.Run(() => _action((T)parameter));
        }

        #endregion
    }
}
