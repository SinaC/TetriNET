using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TetriNET.Common;
using TetriNET.Common.Contracts;

namespace TetriNET.Server
{
    // TODO: dissociate LastAction from server than LastAction to server
    public sealed class DummyBuiltInClient : ITetriNETCallback
    {
        private const int HeartbeatDelay = 300; // in ms
        private const int Width = 12;
        private const int Height = 20;

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
        private byte[] PlayerGrid { get; set; }
        private int TetriminoIndex { get; set; }

        public DateTime LastAction { get; set; }
        public string PlayerName { get; private set; }
        public States State { get; private set; }
        public int PlayerId { get; private set; }
        public bool IsServerMaster { get; private set; }

        public DummyBuiltInClient(string playerName, Func<ITetriNET> getProxyFunc)
        {
            PlayerName = playerName;
            _getProxyFunc = getProxyFunc;

            PlayerGrid = new byte[Width*Height];
            for (int i = 0; i < Height; i++)
                PlayerGrid[i * Width] = 1;

            IsServerMaster = false;
            TetriminoIndex = 0;
            LastAction = DateTime.Now.AddMilliseconds(-HeartbeatDelay);
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
                    int rnd = new Random().Next(4);
                    switch (rnd)
                    {
                        case 0:
                            Proxy.PublishMessage(this, "I'll kill you");
                            break;
                        case 1:
                            Proxy.PlaceTetrimino(this, TetriminoIndex, Tetriminos.TetriminoI, Orientations.Top, new Position
                            {
                                X = 5,
                                Y = 3
                            },
                            PlayerGrid);
                            TetriminoIndex++;
                            break;
                        case 2:
                            Proxy.UseSpecial(this, PlayerId, Specials.Nuke);
                            break;
                        case 3:
                            Proxy.ModifyGrid(this, PlayerGrid);
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
                Log.WriteLine("TEST {0} {1} {2}", PlayerName, State, timespan.TotalMilliseconds);
                if (timespan.TotalMilliseconds > HeartbeatDelay)
                    Proxy.Heartbeat(this);
            }
        }

        public void Lose()
        {
            Log.WriteLine("Loseeeeeeeer");
            State = States.WaitingStartGame;

            Proxy.GameLost(this);
        }
        
        #region ITetriNETCallback

        public void OnHeartbeatReceived()
        {
            Log.WriteLine("OnHeartbeatReceived[{0}]", PlayerName);
            LastAction = DateTime.Now;
        }

        public void OnServerStopped()
        {
            Log.WriteLine("OnServerStopped[{0}]", PlayerName);
            State = States.ApplicationStarted;
            LastAction = DateTime.Now;
        }

        public void OnPlayerRegistered(bool succeeded, int playerId, bool gameStarted)
        {
            Log.WriteLine("OnPlayerRegistered[{0}]:{1} => {2} {3}", PlayerName, succeeded, playerId, gameStarted);
            LastAction = DateTime.Now;
            if (succeeded)
            {
                PlayerId = playerId;
                State = States.WaitingStartGame;
            }
            else
                State = States.ApplicationStarted;
        }

        public void OnPlayerJoined(int playerId, string name)
        {
            Log.WriteLine("OnPlayerJoined[{0}]:{1}[{2}]", PlayerName, name, playerId);
            LastAction = DateTime.Now;
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            Log.WriteLine("OnPlayedLeft[{0}]:{1}[{2}] {3}", PlayerName, name, playerId, reason);
            LastAction = DateTime.Now;
        }

        public void OnPlayerLost(int playerId)
        {
            Log.WriteLine("OnPlayerLost[{0}]:[{1}]", PlayerName, playerId);
            LastAction = DateTime.Now;
        }

        public void OnPlayerWon(int playerId)
        {
            Log.WriteLine("OnPlayerWon[{0}]:[{1}]", PlayerName, playerId);
            LastAction = DateTime.Now;
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, GameOptions options)
        {
            Log.WriteLine("OnGameStarted[{0}]:{1} {2}", PlayerName, firstTetrimino, secondTetrimino);
            LastAction = DateTime.Now;
            if (State == States.WaitingStartGame)
            {
                State = States.GameStarted;
                TetriminoIndex = 0;
            }
            else
                Log.WriteLine("Was not waiting start game");
        }

        public void OnGameFinished()
        {
            Log.WriteLine("OnGameFinished[{0}]", PlayerName);
            LastAction = DateTime.Now;
            if (State == States.GameStarted)
            {
                Log.WriteLine("Game finished: #tetrimino: {0}", TetriminoIndex);
                State = States.GameFinished;
            }
            else
                Log.WriteLine("Game was not started");
        }

        public void OnGamePaused()
        {
            Log.WriteLine("OnGamePaused[{0}]", PlayerName);
            LastAction = DateTime.Now;
        }

        public void OnGameResumed()
        {
            Log.WriteLine("OnGameResumed[{0}]", PlayerName);
            LastAction = DateTime.Now;
        }

        public void OnServerAddLines(int lineCount)
        {
            Log.WriteLine("OnServerAddLines[{0}]:{1}", PlayerName, lineCount);
            LastAction = DateTime.Now;
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            Log.WriteLine("OnPlayerAddLines[{0}]:{1} [{2}] {3}", PlayerName, specialId, playerId, lineCount);
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

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            Log.WriteLine("OnSpecialUsed[{0}]:{1} [{2}] [{3}] {4}", PlayerName, specialId, playerId, targetId, special);
            LastAction = DateTime.Now;
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            Log.WriteLine("OnNextTetrimino[{0}]:{1} {2}", PlayerName, index, tetrimino);
            LastAction = DateTime.Now;
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            Log.WriteLine("OnGridModified[{0}]:[{1}] [2]", PlayerName, playerId, grid.Count(x => x > 0));
            LastAction = DateTime.Now;
        }

        public void OnServerMasterChanged(int playerId)
        {
            Log.WriteLine("OnServerMasterChanged[{0}]:[{1}]", PlayerName, playerId);
            LastAction = DateTime.Now;
            IsServerMaster = (playerId == PlayerId);
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            Log.WriteLine("OnWinListModified:{1}", winList.Select(x => String.Format("{0}:{1}", x.PlayerName, x.Score)).Aggregate((n, i) => n + "|" + i));
            LastAction = DateTime.Now;
        }

        #endregion

        private void Register(string playerName)
        {
            Log.WriteLine("Registering");
            State = States.Registering;

            PlayerName = playerName;

            Proxy.RegisterPlayer(this, PlayerName);
        }
    }
}
