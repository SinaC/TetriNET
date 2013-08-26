using System;
using System.Collections.Generic;
using System.Timers;
using TetriNET.Common.Interfaces;

namespace TetriNET.ConsoleWCFClient.GameController
{
    public enum Commands
    {
        Drop,
        Down,
        Left,
        Right,
        RotateClockwise,
        RotateCounterclockwise
    }

    public class GameController
    {
        private readonly IClient _client;
        private readonly Dictionary<Commands, Timer> _timers = new Dictionary<Commands, Timer>();

        public GameController(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            _client = client;

            client.OnGamePaused += ClientOnOnGamePaused;

            _timers.Add(Commands.Drop, CreateTimer(75, DropTickHandler));
            _timers.Add(Commands.Down, CreateTimer(75, DownTickHandler));
            _timers.Add(Commands.Left, CreateTimer(170, LeftTickHandler));
            _timers.Add(Commands.Right, CreateTimer(170, RightTickHandler));
        }

        public void KeyDown(Commands cmd)
        {
            if (_client.IsGameStarted)
            {
                if (_timers.ContainsKey(cmd) && _timers[cmd].Enabled)
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

        private void DropTickHandler(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _client.Drop();
        }

        private void DownTickHandler(object sender, ElapsedEventArgs e)
        {
            _client.MoveDown();
        }

        private void LeftTickHandler(object sender, ElapsedEventArgs e)
        {
            _client.MoveLeft();
        }

        private void RightTickHandler(object sender, ElapsedEventArgs e)
        {
            _client.MoveRight();
        }

        #region IClient event handlers
        
        private void ClientOnOnGamePaused()
        {
            foreach (Timer timer in _timers.Values)
                timer.Stop();
        }

        #endregion

        private Timer CreateTimer(double interval, ElapsedEventHandler handler)
        {
            Timer timer = new Timer(interval);
            timer.Elapsed += handler;

            return timer;
        }
    }
}
