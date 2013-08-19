using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using TetriNET.Common;
using TetriNET.Common.Contracts;

namespace TetriNET.Client
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class GameClient : ITetriNETCallback, IClient
    {
        private const int InactivityTimeoutBeforePing = 500; // in ms
        private const int HeartBeatDelay = 500; // in ms
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

        private readonly IProxyManager _proxyManager;
        private IWCFTetriNET Proxy { get; set; }
        private byte[] PlayerGrid { get; set; }
        private int TetriminoIndex { get; set; }

        public string PlayerName { get; set; }
        public States State { get; private set; }
        public int PlayerId { get; private set; }
        public bool IsServerMaster { get; private set; }

        private DateTime _lastHeartbeat;

        public GameClient(IProxyManager proxyManager)
        {
            State = States.ApplicationStarted;
            _proxyManager = proxyManager;

            PlayerGrid = new byte[Width*Height];
            for (int i = 0; i < Height; i++)
                PlayerGrid[i * Width] = 1;

            TetriminoIndex = 0;
            IsServerMaster = false;

            _lastHeartbeat = DateTime.Now;
        }

        public void ConnectToServer()
        {
            Log.WriteLine("Connecting to server");
            State = States.ConnectingToServer;

            Proxy = _proxyManager.CreateProxy(this, this);

            if (Proxy != null)
                State = States.ConnectedToServer;
            else
                State = States.ApplicationStarted;
        }

        public void DisconnectFromServer()
        {
            Log.WriteLine("Disconnecting from server");

            Proxy.UnregisterPlayer();
            //
            State = States.ApplicationStarted;
        }

        public void Register(string playerName)
        {
            Log.WriteLine("Registering");
            State = States.Registering;

            PlayerName = playerName;
            
            Proxy.RegisterPlayer(PlayerName);
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
                            Proxy.PublishMessage("I'll kill you");
                            break;
                        case 1:
                            Proxy.PlaceTetrimino(TetriminoIndex, Tetriminos.TetriminoI, Orientations.Top, new Position
                            {
                                X = 5,
                                Y = 3
                            },
                            PlayerGrid);
                            TetriminoIndex++;
                            break;
                        case 2:
                            Proxy.UseSpecial(PlayerId, Specials.Nuke);
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
                // TODO: server timeout
                //TimeSpan timespan = DateTime.Now - LastAction;
                //if (timespan.TotalMilliseconds > InactivityTimeoutBeforePing)
                //    Proxy.Heartbeat();
                if ((DateTime.Now - _lastHeartbeat).TotalMilliseconds >= HeartBeatDelay)
                {
                    Proxy.Heartbeat();
                    _lastHeartbeat = DateTime.Now;
                }
            }
        }

        //public void Close()
        //{
        //    (Proxy as ICommunicationObject).Close();
        //}

        #region IClient

        public DateTime LastAction { get; set; }

        public void OnDisconnectedFromServer(IWCFTetriNET proxy)
        {
            //throw new ApplicationException("OnDisconnectedFromServer");
            State = States.ApplicationStarted;
        }

        public void OnServerUnreachable(IWCFTetriNET proxy)
        {
            // TODO
            throw new ApplicationException("OnServerUnreachable");
        }

        #endregion

        #region ITetriNETCallback

        public void OnHeartbeatReceived()
        {
            Log.WriteLine("OnHeartbeatReceived");
            LastAction = DateTime.Now;
        }

        public void OnServerStopped()
        {
            Log.WriteLine("OnServerStopped");
            State = States.ApplicationStarted;
            LastAction = DateTime.Now;
        }

        public void OnPlayerRegistered(bool succeeded, int playerId, bool gameStarted)
        {
            Log.WriteLine("OnPlayerRegistered:{0} => {1} {2}", succeeded, playerId, gameStarted);
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
            Log.WriteLine("OnPlayerJoined:{0}[{1}]", name, playerId);
            LastAction = DateTime.Now;
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            Log.WriteLine("OnPlayedLeft:{0}[{1}] [2]", name, playerId, reason);
            LastAction = DateTime.Now;
        }

        public void OnPlayerLost(int playerId)
        {
            Log.WriteLine("OnPlayerLost:[{0}]", playerId);
            LastAction = DateTime.Now;
        }

        public void OnPlayerWon(int playerId)
        {
            Log.WriteLine("OnPlayerWon:[{0}]", playerId);
            LastAction = DateTime.Now;
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, GameOptions options)
        {
            Log.WriteLine("OnGameStarted:{0} {1}", firstTetrimino, secondTetrimino);
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
            Log.WriteLine("OnGameFinished:");
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
            Log.WriteLine("OnGamePaused");
            LastAction = DateTime.Now;
        }

        public void OnGameResumed()
        {
            Log.WriteLine("OnGameResumed");
            LastAction = DateTime.Now;
        }

        public void OnServerAddLines(int lineCount)
        {
            Log.WriteLine("OnServerAddLines:{0}", lineCount);
            LastAction = DateTime.Now;
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            Log.WriteLine("OnPlayerAddLines:{0} [{1}] {2}", specialId, playerId, lineCount);
            LastAction = DateTime.Now;
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            Log.WriteLine("OnPublishPlayerMessage:{0}:{1}", playerName, msg);
            LastAction = DateTime.Now;
        }

        public void OnPublishServerMessage(string msg)
        {
            Log.WriteLine("OnPublishServerMessage:{0}", msg);
            LastAction = DateTime.Now;
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            Log.WriteLine("OnSpecialUsed[{0}]:{1} [{2}] [{3}] {4}", PlayerName, specialId, playerId, targetId, special);
            LastAction = DateTime.Now;
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            Log.WriteLine("OnNextTetrimino:{0} {1}", index, tetrimino);
            LastAction = DateTime.Now;
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            Log.WriteLine("OnGridModified:[{0}] [1]", playerId, grid.Count(x => x > 0));
            LastAction = DateTime.Now;
        }

        public void OnServerMasterChanged(int playerId)
        {
            Log.WriteLine("OnServerMasterChanged:[{0}]", playerId);
            LastAction = DateTime.Now;
            IsServerMaster = (playerId == PlayerId);
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            Log.WriteLine("OnWinListModified:{1}", winList.Select(x => String.Format("{0}:{1}", x.PlayerName, x.Score)).Aggregate((n, i) => n + "|" + i));
            LastAction = DateTime.Now;
        }


        #endregion
    }
}
