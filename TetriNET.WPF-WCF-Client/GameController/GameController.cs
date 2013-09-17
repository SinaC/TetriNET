using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using TetriNET.Common.Interfaces;
using TetriNET.Logger;

namespace TetriNET.WPF_WCF_Client.GameController
{
    public enum Commands
    {
        Invalid,

        Drop,
        Down,
        Left,
        Right,
        RotateClockwise,
        RotateCounterclockwise,

        DiscardFirstSpecial,
        UseSpecialOn1,
        UseSpecialOn2,
        UseSpecialOn3,
        UseSpecialOn4,
        UseSpecialOn5,
        UseSpecialOn6,
    }

    public class GameController
    {
        private static readonly Commands[] CommandsAvailableForConfusion =
            {
                Commands.Drop,
                Commands.Down,
                Commands.Left,
                Commands.Right,
                Commands.RotateClockwise,
                Commands.RotateCounterclockwise
            };

        private readonly Random _random;
        private readonly IClient _client;
        private readonly Dictionary<Commands, DispatcherTimer> _timers = new Dictionary<Commands, DispatcherTimer>();
        private readonly Dictionary<Commands, Commands> _confusionMapping = new Dictionary<Commands, Commands>();

        private bool _isConfusionActive;

        public GameController(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            _random = new Random();
            _client = client;
            _isConfusionActive = false;

            client.OnGamePaused += OnGamePaused;
            client.OnGameFinished += OnGameFinished;
            client.OnConfusionToggled += OnConfusionToggled;
        }

        public void SubscribeToTickHandler()
        {
            AddTimer(Commands.Drop, 100, DropTickHandler);
            AddTimer(Commands.Down, 50, DownTickHandler);
            AddTimer(Commands.Left, 100, LeftTickHandler);
            AddTimer(Commands.Right, 100, RightTickHandler);
        }

        public void UnsubscribeToTickHandler()
        {
            RemoveTimer(Commands.Drop);
            RemoveTimer(Commands.Down);
            RemoveTimer(Commands.Left);
            RemoveTimer(Commands.Right);
        }

        public void UnsubscribeFromClientEvents()
        {
            _client.OnGamePaused -= OnGamePaused;
            _client.OnGameFinished -= OnGameFinished;
            _client.OnConfusionToggled -= OnConfusionToggled;
        }

        public void KeyDown(Commands cmd)
        {
            if (_client.IsPlaying)
            {
                if (_isConfusionActive)
                {
                    Commands confusedCmd;
                    if (_confusionMapping.TryGetValue(cmd, out confusedCmd))
                        cmd = confusedCmd;
                }
                if (_timers.ContainsKey(cmd) && _timers[cmd].IsEnabled)
                    return;
                switch (cmd)
                {
                    case Commands.Drop:
                        _client.Drop();
                        break;
                    case Commands.Down:
                        _client.MoveDown();
                        break;
                    case Commands.Left:
                        _client.MoveLeft();
                        break;
                    case Commands.Right:
                        _client.MoveRight();
                        break;
                    case Commands.RotateClockwise:
                        _client.RotateClockwise();
                        break;
                    case Commands.RotateCounterclockwise:
                        _client.RotateCounterClockwise();
                        break;

                    case Commands.DiscardFirstSpecial:
                        _client.DiscardFirstSpecial();
                        break;
                    case Commands.UseSpecialOn1:
                        _client.UseSpecial(0);
                        break;
                    case Commands.UseSpecialOn2:
                        _client.UseSpecial(1);
                        break;
                    case Commands.UseSpecialOn3:
                        _client.UseSpecial(2);
                        break;
                    case Commands.UseSpecialOn4:
                        _client.UseSpecial(3);
                        break;
                    case Commands.UseSpecialOn5:
                        _client.UseSpecial(4);
                        break;
                    case Commands.UseSpecialOn6:
                        _client.UseSpecial(5);
                        break;
                }
                if (_timers.ContainsKey(cmd))
                    _timers[cmd].Start();
            }
        }

        public void KeyUp(Commands cmd)
        {
            if (_timers.ContainsKey(cmd))
                _timers[cmd].Stop();
        }

        #region IClient event handlers

        private void OnGameFinished()
        {
            foreach (DispatcherTimer timer in _timers.Values)
                timer.Stop();
        }

        private void OnGamePaused()
        {
            foreach (DispatcherTimer timer in _timers.Values)
                timer.Stop();
        }

        private void OnConfusionToggled(bool active)
        {
            _isConfusionActive = active;
            if (active)
            {
                _confusionMapping.Clear();
                //List<Commands> commands = Enum.GetValues(typeof(Commands)).Cast<Commands>().Where(x => x != Commands.Invalid).ToList();
                List<Commands> shuffled = Shuffle(_random, CommandsAvailableForConfusion);
                for (int i = 0; i < CommandsAvailableForConfusion.Length; i++)
                {
                    _confusionMapping.Add(CommandsAvailableForConfusion[i], shuffled[i]);
                    Log.WriteLine(Log.LogLevels.Debug, "Confusion mapping {0} -> {1}", CommandsAvailableForConfusion[i], shuffled[i]);
                }
            }
        }

        #endregion

        private void DropTickHandler(object sender, EventArgs elapsedEventArgs)
        {
            _client.Drop();
        }

        private void DownTickHandler(object sender, EventArgs e)
        {
            _client.MoveDown();
        }

        private void LeftTickHandler(object sender, EventArgs e)
        {
            _client.MoveLeft();
        }

        private void RightTickHandler(object sender, EventArgs e)
        {
            _client.MoveRight();
        }

        private void AddTimer(Commands cmd, double interval, EventHandler handler)
        {
            if (!_timers.ContainsKey(cmd))
            {
                DispatcherTimer timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), DispatcherPriority.Normal, handler, Dispatcher.CurrentDispatcher);
                timer.Stop(); // Dunno why but these timer are started by default
                _timers.Add(cmd, timer);
            }
        }

        private void RemoveTimer(Commands cmd)
        {
            DispatcherTimer timer;
            if (_timers.TryGetValue(cmd, out timer))
            {
                timer.Stop();
                _timers.Remove(cmd);
            }
        }

        private List<T> Shuffle<T>(Random random, IEnumerable<T> list)
        {
            List<T> newList = list.Select(x => x).ToList();
            for (int i = newList.Count; i > 1; i--)
            {
                // Pick random element to swap.
                int j = random.Next(i); // 0 <= j <= i-1
                // Swap.
                T tmp = newList[j];
                newList[j] = newList[i - 1];
                newList[i - 1] = tmp;
            }
            return newList;
        }
    }
}
