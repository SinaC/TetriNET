using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Logger;

namespace TetriNET.WPF_WCF_Client.GameController
{
    public class GameController : IGameController
    {
        private static readonly Client.Interfaces.Commands[] CommandsAvailableForConfusion =
            {
                TetriNET.Client.Interfaces.Commands.Drop,
                TetriNET.Client.Interfaces.Commands.Down,
                TetriNET.Client.Interfaces.Commands.Left,
                TetriNET.Client.Interfaces.Commands.Right,
                TetriNET.Client.Interfaces.Commands.RotateClockwise,
                TetriNET.Client.Interfaces.Commands.RotateCounterclockwise
            };

        private readonly Random _random;
        private readonly Dictionary<Client.Interfaces.Commands, DispatcherTimer> _timers = new Dictionary<Client.Interfaces.Commands, DispatcherTimer>();
        private readonly Dictionary<Client.Interfaces.Commands, Client.Interfaces.Commands> _confusionMapping = new Dictionary<Client.Interfaces.Commands, Client.Interfaces.Commands>();

        private bool _isConfusionActive;

        public GameController(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            _random = new Random();
            Client = client;
            _isConfusionActive = false;

            client.OnGameStarted += OnGameStarted;
            client.OnGamePaused += OnGamePaused;
            client.OnGameFinished += OnGameFinished;
            client.OnContinuousEffectToggled += OnContinuousEffectToggled;
        }

        #region IGameController

        public IClient Client { get; private set; }

        public void AddSensibility(Client.Interfaces.Commands cmd, int interval)
        {
            DispatcherTimer timer;
            if (_timers.TryGetValue(cmd, out timer))
                timer.Interval = TimeSpan.FromMilliseconds(interval);
            else
                switch (cmd)
                {
                    case TetriNET.Client.Interfaces.Commands.Drop:
                        AddTimer(TetriNET.Client.Interfaces.Commands.Drop, interval, DropTickHandler);
                        break;
                    case TetriNET.Client.Interfaces.Commands.Down:
                        AddTimer(TetriNET.Client.Interfaces.Commands.Down, interval, DownTickHandler);
                        break;
                    case TetriNET.Client.Interfaces.Commands.Left:
                        AddTimer(TetriNET.Client.Interfaces.Commands.Left, interval, LeftTickHandler);
                        break;
                    case TetriNET.Client.Interfaces.Commands.Right:
                        AddTimer(TetriNET.Client.Interfaces.Commands.Right, interval, RightTickHandler);
                        break;
                }
        }

        public void RemoveSensibility(Client.Interfaces.Commands cmd)
        {
            if (_timers.ContainsKey(cmd))
                RemoveTimer(cmd);
        }

        public void UnsubscribeFromClientEvents()
        {
            Client.OnGameStarted -= OnGameStarted;
            Client.OnGamePaused -= OnGamePaused;
            Client.OnGameFinished -= OnGameFinished;
            Client.OnContinuousEffectToggled -= OnContinuousEffectToggled;
        }

        public void KeyDown(Client.Interfaces.Commands cmd)
        {
            if (Client.IsPlaying)
            {
                if (_isConfusionActive)
                {
                    Client.Interfaces.Commands confusedCmd;
                    if (_confusionMapping.TryGetValue(cmd, out confusedCmd))
                        cmd = confusedCmd;
                }
                if (_timers.ContainsKey(cmd) && _timers[cmd].IsEnabled)
                    return;
                switch (cmd)
                {
                    case TetriNET.Client.Interfaces.Commands.Hold:
                        Client.Hold();
                        break;
                    case TetriNET.Client.Interfaces.Commands.Drop:
                        Client.Drop();
                        break;
                    case TetriNET.Client.Interfaces.Commands.Down:
                        Client.MoveDown();
                        break;
                    case TetriNET.Client.Interfaces.Commands.Left:
                        Client.MoveLeft();
                        break;
                    case TetriNET.Client.Interfaces.Commands.Right:
                        Client.MoveRight();
                        break;
                    case TetriNET.Client.Interfaces.Commands.RotateClockwise:
                        Client.RotateClockwise();
                        break;
                    case TetriNET.Client.Interfaces.Commands.RotateCounterclockwise:
                        Client.RotateCounterClockwise();
                        break;

                    case TetriNET.Client.Interfaces.Commands.DiscardFirstSpecial:
                        Client.DiscardFirstSpecial();
                        break;
                    case TetriNET.Client.Interfaces.Commands.UseSpecialOn1:
                        Client.UseSpecial(0);
                        break;
                    case TetriNET.Client.Interfaces.Commands.UseSpecialOn2:
                        Client.UseSpecial(1);
                        break;
                    case TetriNET.Client.Interfaces.Commands.UseSpecialOn3:
                        Client.UseSpecial(2);
                        break;
                    case TetriNET.Client.Interfaces.Commands.UseSpecialOn4:
                        Client.UseSpecial(3);
                        break;
                    case TetriNET.Client.Interfaces.Commands.UseSpecialOn5:
                        Client.UseSpecial(4);
                        break;
                    case TetriNET.Client.Interfaces.Commands.UseSpecialOn6:
                        Client.UseSpecial(5);
                        break;
                    case TetriNET.Client.Interfaces.Commands.UseSpecialOnSelf:
                        Client.UseSpecial(Client.PlayerId);
                        break;
                }
                if (_timers.ContainsKey(cmd))
                    _timers[cmd].Start();
            }
        }

        public void KeyUp(Client.Interfaces.Commands cmd)
        {
            if (_timers.ContainsKey(cmd))
                _timers[cmd].Stop();
        }

        #endregion

        #region IClient event handlers

        private void OnGameStarted()
        {
            _isConfusionActive = false;
        }

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

        private void OnContinuousEffectToggled(Specials special, bool active, double duration)
        {
            if (special == Specials.Confusion)
            {
                _isConfusionActive = active;
                if (active)
                {
                    _confusionMapping.Clear();
                    //List<Commands> commands = Enum.GetValues(typeof(Commands)).Cast<Commands>().Where(x => x != Commands.Invalid).ToList();
                    List<Client.Interfaces.Commands> shuffled = Shuffle(_random, CommandsAvailableForConfusion);
                    for (int i = 0; i < CommandsAvailableForConfusion.Length; i++)
                    {
                        _confusionMapping.Add(CommandsAvailableForConfusion[i], shuffled[i]);
                        Log.WriteLine(Log.LogLevels.Debug, "Confusion mapping {0} -> {1}", CommandsAvailableForConfusion[i], shuffled[i]);
                    }
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

        private void AddTimer(Client.Interfaces.Commands cmd, double interval, EventHandler handler)
        {
            if (!_timers.ContainsKey(cmd))
            {
                DispatcherTimer timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), DispatcherPriority.Normal, handler, Dispatcher.CurrentDispatcher);
                timer.Stop(); // Dunno why but these timer are started by default
                _timers.Add(cmd, timer);
            }
        }

        private void RemoveTimer(Client.Interfaces.Commands cmd)
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
