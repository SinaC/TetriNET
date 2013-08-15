using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class DummyBuiltInClient : ITetriNETCallback
    {
        private const int InactivityTimeoutBeforePing = 500; // in ms

        public enum States
        {
            ApplicationStarted, // -> ConnectingToServer
            ConnectingToServer, // -> ConnectedToServer
            ConnectedToServer, // -> Registering
            Registering, // -> WaitingStartGame | ApplicationStarted
            WaitingStartGame, // -> GameStarted
            GameStarted, // -> GameFinished
            GameFinished, // -> WaitingStartGame
        }

        private readonly Func<ITetriNET> _getProxyFunc;
        private ITetriNET Proxy { get; set; }

        public DateTime LastAction { get; set; }
        public string PlayerName { get; private set; }
        public States State { get; private set; }
        public int PlayerId { get; private set; }


        public DummyBuiltInClient(string playerName, Func<ITetriNET> getProxyFunc)
        {
            PlayerName = playerName;
            _getProxyFunc = getProxyFunc;

            State = States.ApplicationStarted;
        }

        public void ConnectToServer()
        {
            Log.WriteLine("Connecting to server");
            State = States.ConnectingToServer;

            Proxy = _getProxyFunc();

            if (Proxy != null)
                State = States.ConnectedToServer;
            else
                State = States.ApplicationStarted;
        }

        public void DisconnectFromServer()
        {
            Log.WriteLine("Disconnecting from server");

            Proxy.UnregisterPlayer(this);
            //
            State = States.ApplicationStarted;
        }

        public void Register(string playerName)
        {
            Log.WriteLine("Registering");
            State = States.Registering;

            PlayerName = playerName;

            Proxy.RegisterPlayer(this, PlayerName);
        }

        public void Test()
        {
            switch (State)
            {
                case States.ApplicationStarted:
                    ConnectToServer();
                    break;
                case States.ConnectingToServer:
                    // NOP: wait connection resolution
                    break;
                case States.ConnectedToServer:
                    Register(PlayerName);
                    break;
                case States.Registering:
                    // NOP: waiting callback OnPlayerRegistered
                    break;
                case States.WaitingStartGame:
                    // NOP: waiting callback OnGameStarted
                    break;
                case States.GameStarted:
                    int rnd = new Random().Next(3);
                    switch (rnd)
                    {
                        case 0:
                            Proxy.PublishMessage(this, "I'll kill you");
                            break;
                        case 1:
                            Proxy.PlaceTetrimino(this, Tetriminos.TetriminoI, Orientations.Top, new Position
                            {
                                X = 5,
                                Y = 3
                            });
                            break;
                        case 2:
                            Proxy.SendAttack(this, PlayerId, Attacks.Nuke);
                            break;
                    }
                    Thread.Sleep(60);
                    break;
                case States.GameFinished:
                    State = States.WaitingStartGame;
                    break;
            }
            if (State == States.WaitingStartGame || State == States.GameStarted || State == States.GameFinished)
            {
                TimeSpan timespan = DateTime.Now - LastAction;
                if (timespan.TotalMilliseconds > InactivityTimeoutBeforePing)
                    Proxy.Ping(this);
            }
        }

        #region ITetriNETCallback

        public void OnPingReceived()
        {
            Log.WriteLine("OnPingReceived[{0}]", PlayerName);
            LastAction = DateTime.Now;
        }

        public void OnServerStopped()
        {
            Log.WriteLine("OnServerStopped[{0}]", PlayerName);
            State = States.ApplicationStarted;
            LastAction = DateTime.Now;
        }

        public void OnPlayerRegistered(bool succeeded, int playerId)
        {
            Log.WriteLine("OnPlayerRegistered[{0}]:{1} => {2}", PlayerName, succeeded, playerId);
            LastAction = DateTime.Now;
            if (succeeded)
            {
                PlayerId = playerId;
                State = States.WaitingStartGame;
            }
            else
                State = States.ApplicationStarted;
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, List<PlayerData> players)
        {
            Log.WriteLine("OnGameStarted[{0}]:{1} {2}", PlayerName, firstTetrimino, secondTetrimino);
            LastAction = DateTime.Now;
            if (State == States.WaitingStartGame)
            {
                Log.WriteLine("Game started with players:" + players.Select(x => x.Name + "[" + x.Id + "]").Aggregate((n, i) => n + "," + i));
                State = States.GameStarted;
            }
            else
                Log.WriteLine("Was not waiting start game");
        }

        public void OnGameFinished()
        {
            Log.WriteLine("OnGameFinished");
            LastAction = DateTime.Now;
            if (State == States.GameStarted)
            {
                State = States.GameFinished;
            }
            else
                Log.WriteLine("Game was not started");
        }

        public void OnServerAddLines(int lineCount)
        {
            Log.WriteLine("OnServerAddLines[{0}]:{1}", PlayerName, lineCount);
            LastAction = DateTime.Now;
        }

        public void OnPlayerAddLines(int lineCount)
        {
            Log.WriteLine("OnPlayerAddLines[{0}]:{1}", PlayerName, lineCount);
            LastAction = DateTime.Now;
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            Log.WriteLine("OnPublishPlayerMessage[{0}]:{1}:{2}", PlayerName, playerName, msg);
            LastAction = DateTime.Now;
        }

        public void OnPublishServerMessage(string msg)
        {
            Log.WriteLine("OnPublishServerMessage[{0}]:{1}", PlayerName, msg);
            LastAction = DateTime.Now;
        }

        public void OnAttackReceived(Attacks attack)
        {
            Log.WriteLine("OnAttackReceived[{0}]:{1}", PlayerName, attack);
            LastAction = DateTime.Now;
        }

        public void OnAttackMessageReceived(string msg)
        {
            Log.WriteLine("OnAttackMessageReceived[{0}]:{1}", PlayerName, msg);
            LastAction = DateTime.Now;
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            Log.WriteLine("OnNextTetrimino[{0}]:{1} {2}", PlayerName, index, tetrimino);
            LastAction = DateTime.Now;
        }

        #endregion
    }
}
