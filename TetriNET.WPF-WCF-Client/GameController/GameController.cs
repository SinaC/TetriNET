using System;
using System.Collections.Generic;
using System.Windows.Threading;
using TetriNET.Common.Interfaces;

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
        private readonly IClient _client;
        private readonly Dictionary<Commands, DispatcherTimer> _timers = new Dictionary<Commands, DispatcherTimer>();

        public GameController(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            _client = client;

            client.OnGamePaused += OnGamePaused;
            client.OnGameFinished += OnGameFinished;

            _timers.Add(Commands.Drop, CreateTimer(100, DropTickHandler));
            _timers.Add(Commands.Down, CreateTimer(50, DownTickHandler));
            _timers.Add(Commands.Left, CreateTimer(100, LeftTickHandler));
            _timers.Add(Commands.Right, CreateTimer(100, RightTickHandler));
        }

        public void UnsubscribeFromClientEvents()
        {
            _client.OnGamePaused -= OnGamePaused;
            _client.OnGameFinished -= OnGameFinished;
        }

        public void KeyDown(Commands cmd)
        {
            if (_client.IsPlaying)
            {
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

        private static DispatcherTimer CreateTimer(double interval, EventHandler handler)
        {
            DispatcherTimer timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), DispatcherPriority.Normal, handler, Dispatcher.CurrentDispatcher);
            timer.Stop(); // Dunno why but these timer are started by default
            return timer;
        }
    }
}
