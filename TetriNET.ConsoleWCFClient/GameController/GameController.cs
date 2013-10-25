using System;
using System.Collections.Generic;
using System.Timers;
using TetriNET.Client.Interfaces;

namespace TetriNET.ConsoleWCFClient.GameController
{
    public class GameController : IGameController
    {
        private readonly Dictionary<Commands, Timer> _timers = new Dictionary<Commands, Timer>();

        public GameController(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            Client = client;

            client.OnGamePaused += OnGamePaused;
            client.OnGameFinished += OnGameFinished;

            _timers.Add(Commands.Drop, CreateTimer(75, DropTickHandler));
            _timers.Add(Commands.Down, CreateTimer(75, DownTickHandler));
            _timers.Add(Commands.Left, CreateTimer(170, LeftTickHandler));
            _timers.Add(Commands.Right, CreateTimer(170, RightTickHandler));
        }

        #region IGameController

        public IClient Client { get; private set; }

        public void AddSensibility(Commands cmd, int interval)
        {
        }

        public void RemoveSensibility(Commands cmd)
        {
        }

        public void UnsubscribeFromClientEvents()
        {
            Client.OnGamePaused -= OnGamePaused;
            Client.OnGameFinished -= OnGameFinished;
        }

        public void KeyDown(Commands cmd)
        {
            if (Client.IsPlaying)
            {
                if (_timers.ContainsKey(cmd) && _timers[cmd].Enabled)
                    return;
                switch (cmd)
                {
                    case Commands.Hold:
                        Client.Hold();
                        break;
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
                    case Commands.UseSpecialOnSelf:
                        Client.UseSpecial(Client.PlayerId);
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
            foreach (Timer timer in _timers.Values)
                timer.Stop();
        }

        private void OnGamePaused()
        {
            foreach (Timer timer in _timers.Values)
                timer.Stop();
        }
        #endregion

        private void DropTickHandler(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Client.Drop();
        }

        private void DownTickHandler(object sender, ElapsedEventArgs e)
        {
            Client.MoveDown();
        }

        private void LeftTickHandler(object sender, ElapsedEventArgs e)
        {
            Client.MoveLeft();
        }

        private void RightTickHandler(object sender, ElapsedEventArgs e)
        {
            Client.MoveRight();
        }

        private static Timer CreateTimer(double interval, ElapsedEventHandler handler)
        {
            Timer timer = new Timer(interval);
            timer.Elapsed += handler;

            return timer;
        }
    }
}
