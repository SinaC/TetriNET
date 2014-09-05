using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;

namespace TetriNET.Common.BlockingActionQueue
{
    public class BlockingActionQueue : IActionQueue
    {
        private readonly BlockingCollection<Action> _gameActionBlockingCollection = new BlockingCollection<Action>(new ConcurrentQueue<Action>());

        private CancellationTokenSource _cancellationTokenSource;
        private Task _gameActionTask;

        public int ActionCount { get { return _gameActionBlockingCollection.Count; } }

        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _gameActionTask = Task.Factory.StartNew(GameActionsTask);
        }

        public void Wait(int milliseconds)
        {
            _gameActionTask.Wait(milliseconds);
        }

        public void AddAction(Action action)
        {
            _gameActionBlockingCollection.Add(action);
        }

        public void ResetActions()
        {
            while (_gameActionBlockingCollection.Count > 0)
            {
                Action item;
                _gameActionBlockingCollection.TryTake(out item);
            }
        }

        private void GameActionsTask()
        {
            Log.Default.WriteLine(LogLevels.Info, "GameActionsTask started");

            try
            {
                while (true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "Stop background task event raised");
                        break;
                    }
                    try
                    {
                        Action action;
                        bool taken = _gameActionBlockingCollection.TryTake(out action, 10, _cancellationTokenSource.Token);
                        if (taken)
                        {
                            try
                            {
                                Log.Default.WriteLine(LogLevels.Debug, "Dequeue, item in queue {0}", _gameActionBlockingCollection.Count);
                                action();
                            }
                            catch (Exception ex)
                            {
                                Log.Default.WriteLine(LogLevels.Error, "Exception raised in GameActionsTask. Exception:{0}", ex);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "Taking cancelled");
                        break;
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "GameActionsTask cancelled exception. Exception: {0}", ex);
            }


            Log.Default.WriteLine(LogLevels.Info, "GameActionsTask stopped");
        }
    }
}
