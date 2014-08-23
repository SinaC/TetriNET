using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TetriNET.WPF_WCF_Client.MVVM
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

    //http://jake.ginnivan.net/awaitable-delegatecommand/
    //public interface IAsyncCommand : IAsyncCommand<object>
    //{
    //}

    //public interface IAsyncCommand<in T>
    //{
    //    Task ExecuteAsync(T obj);
    //    bool CanExecute(object obj);
    //    ICommand Command { get; }
    //}

    //public class AsyncRelayCommand : AsyncRelayCommand<object>, IAsyncCommand
    //{
    //    public AsyncRelayCommand(Func<Task> executeMethod)
    //        : base(o => executeMethod())
    //    {
    //    }
    //}

    //public class AsyncRelayCommand<T> : IAsyncCommand<T>, ICommand
    //{
    //    private readonly Func<T, Task> _executeMethod;
    //    private bool _isExecuting;

    //    public AsyncRelayCommand(Func<T, Task> executeMethod)
    //    {
    //        _executeMethod = executeMethod;
    //    }

    //    public async Task ExecuteAsync(T obj)
    //    {
    //        try
    //        {
    //            _isExecuting = true;
    //            await _executeMethod(obj);
    //        }
    //        finally
    //        {
    //            _isExecuting = false;
    //        }
    //    }

    //    public ICommand Command { get { return this; } }

    //    public bool CanExecute(object parameter)
    //    {
    //        return !_isExecuting;
    //    }

    //    public event EventHandler CanExecuteChanged;

    //    public async void Execute(object parameter)
    //    {
    //        await ExecuteAsync((T)parameter);
    //    }
    //}
}
