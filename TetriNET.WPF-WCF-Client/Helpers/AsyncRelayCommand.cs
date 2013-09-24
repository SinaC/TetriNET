using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    public class AsyncRelayCommand : ICommand
    {
        protected readonly Predicate<object> CanExecutePredicate;
        private readonly Action _action;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncRelayCommand(Action action)
        {
            _action = action;
        }

        public AsyncRelayCommand(Action action,
                       Predicate<object> canExecutePredicate)
        {
            _action = action;
            CanExecutePredicate = canExecutePredicate;
        }

        public bool CanExecute(object parameter)
        {
            if (CanExecutePredicate == null)
            {
                return true;
            }

            return CanExecutePredicate(parameter);
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        protected virtual async Task ExecuteAsync(object parameter)
        {
            await Task.Run(() => _action());
        }
    }

    //public class AsyncRelayCommand : ICommand
    //{
    //    protected readonly Predicate<object> CanExecutePredicate;
    //    protected Func<object, Task> AsyncExecute;

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //    public AsyncRelayCommand(Func<object, Task> execute)
    //        : this(execute, null)
    //    {
    //    }

    //    public AsyncRelayCommand(Func<object, Task> asyncExecute,
    //                   Predicate<object> canExecutePredicate)
    //    {
    //        AsyncExecute = asyncExecute;
    //        CanExecutePredicate = canExecutePredicate;
    //    }

    //    public bool CanExecute(object parameter)
    //    {
    //        if (CanExecutePredicate == null)
    //        {
    //            return true;
    //        }

    //        return CanExecutePredicate(parameter);
    //    }

    //    public async void Execute(object parameter)
    //    {
    //        await ExecuteAsync(parameter);
    //    }

    //    protected virtual async Task ExecuteAsync(object parameter)
    //    {
    //        await AsyncExecute(parameter);
    //    }
    //}
}
