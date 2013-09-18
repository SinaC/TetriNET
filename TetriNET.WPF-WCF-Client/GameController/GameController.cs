using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using TetriNET.Common.Interfaces;
using TetriNET.Logger;

namespace TetriNET.WPF_WCF_Client.GameController
{
    public class GameController : IGameController
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
        private readonly Dictionary<Commands, DispatcherTimer> _timers = new Dictionary<Commands, DispatcherTimer>();
        private readonly Dictionary<Commands, Commands> _confusionMapping = new Dictionary<Commands, Commands>();

        private bool _isConfusionActive;

        public GameController(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            _random = new Random();
            Client = client;
            _isConfusionActive = false;

            client.OnGamePaused += OnGamePaused;
            client.OnGameFinished += OnGameFinished;
            client.OnConfusionToggled += OnConfusionToggled;
        }

        #region IGameController

        public IClient Client { get; private set; }

        public void AddSensibility(Commands cmd, int interval)
        {
            DispatcherTimer timer;
            if (_timers.TryGetValue(cmd, out timer))
                timer.Interval = TimeSpan.FromMilliseconds(interval);
            else
                switch (cmd)
                {
                    case Commands.Drop:
                        AddTimer(Commands.Drop, interval, DropTickHandler);
                        break;
                    case Commands.Down:
                        AddTimer(Commands.Down, interval, DownTickHandler);
                        break;
                    case Commands.Left:
                        AddTimer(Commands.Left, interval, LeftTickHandler);
                        break;
                    case Commands.Right:
                        AddTimer(Commands.Right, interval, RightTickHandler);
                        break;
                }
        }

        public void RemoveSensibility(Commands cmd)
        {
            if (_timers.ContainsKey(cmd))
                RemoveTimer(cmd);
        }

        public void UnsubscribeFromClientEvents()
        {
            Client.OnGamePaused -= OnGamePaused;
            Client.OnGameFinished -= OnGameFinished;
            Client.OnConfusionToggled -= OnConfusionToggled;
        }

        public void KeyDown(Commands cmd)
        {
            if (Client.IsPlaying)
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
                        Client.Drop();
                        break;
                    case Commands.Down:
                        Client.MoveDown();
                        break;
                    case Commands.Left:
                        Client.MoveLeft();
                        break;
                    case Commands.Right:
                        Client.MoveRight();
                        break;
                    case Commands.RotateClockwise:
                        Client.RotateClockwise();
                        break;
                    case Commands.RotateCounterclockwise:
                        Client.RotateCounterClockwise();
                        break;

                    case Commands.DiscardFirstSpecial:
                        Client.DiscardFirstSpecial();
                        break;
                    case Commands.UseSpecialOn1:
                        Client.UseSpecial(0);
                        break;
                    case Commands.UseSpecialOn2:
                        Client.UseSpecial(1);
                        break;
                    case Commands.UseSpecialOn3:
                        Client.UseSpecial(2);
                        break;
                    case Commands.UseSpecialOn4:
                        Client.UseSpecial(3);
                        break;
                    case Commands.UseSpecialOn5:
                        Client.UseSpecial(4);
                        break;
                    case Commands.UseSpecialOn6:
                        Client.UseSpecial(5);
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

        #endregion

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
            Client.Drop();
        }

        private void DownTickHandler(object sender, EventArgs e)
        {
            Client.MoveDown();
        }

        private void LeftTickHandler(object sender, EventArgs e)
        {
            Client.MoveLeft();
        }

        private void RightTickHandler(object sender, EventArgs e)
        {
            Client.MoveRight();
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

        private static List<T> Shuffle<T>(Random random, IEnumerable<T> list)
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
