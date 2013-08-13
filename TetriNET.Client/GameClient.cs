using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using TetriNET.Common;

namespace TetriNET.Client
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class GameClient : ITetriNETCallback, IClient
    {
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

        private const int InactivityTimeoutBeforePing = 500; // in ms

        private readonly IProxyManager _proxyManager;
        private ITetriNET Proxy { get; set; }

        public string PlayerName { get; set; }
        public States State { get; private set; }
        public int PlayerId { get; private set; }

        public GameClient(IProxyManager proxyManager)
        {
            State = States.ApplicationStarted;
            _proxyManager = proxyManager;
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
                            Proxy.PlaceTetrimino(Tetriminos.TetriminoI, Orientations.Top, new Position
                            {
                                X = 5,
                                Y = 3
                            });
                            break;
                        case 2:
                            Proxy.SendAttack(PlayerId, Attacks.Nuke);
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
                    Proxy.Ping();
            }
        }

        //public void Close()
        //{
        //    (Proxy as ICommunicationObject).Close();
        //}

        #region IClient

        public DateTime LastAction { get; set; }

        public void OnDisconnectedFromServer(ITetriNET proxy)
        {
            //throw new ApplicationException("OnDisconnectedFromServer");
            State = States.ApplicationStarted;
        }

        public void OnServerUnreachable(ITetriNET proxy)
        {
            // TODO
            throw new ApplicationException("OnServerUnreachable");
        }

        #endregion

        #region ITetriNETCallback

        public void OnPingReceived()
        {
            Log.WriteLine("OnPingReceived");
            LastAction = DateTime.Now;
        }

        public void OnServerStopped()
        {
            Log.WriteLine("OnServerStopped");
            State = States.ApplicationStarted;
            LastAction = DateTime.Now;
        }

        public void OnPlayerRegistered(bool succeeded, int playerId)
        {
            Log.WriteLine("OnPlayerRegistered:" + succeeded + " => " + playerId);
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
            Log.WriteLine("OnGameStarted:" + firstTetrimino+" "+secondTetrimino);
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
            Log.WriteLine("OnServerAddLines");
            LastAction = DateTime.Now;
        }

        public void OnPlayerAddLines(int lineCount)
        {
            Log.WriteLine("OnPlayerAddLines");
            LastAction = DateTime.Now;
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            Log.WriteLine("OnPublishPlayerMessage:" + playerName + ":" + msg);
            LastAction = DateTime.Now;
        }

        public void OnPublishServerMessage(string msg)
        {
            Log.WriteLine("OnPublishServerMessage:" + msg);
            LastAction = DateTime.Now;
        }

        public void OnAttackReceived(Attacks attack)
        {
            Log.WriteLine("OnAttackReceived:" + attack);
            LastAction = DateTime.Now;
        }

        public void OnAttackMessageReceived(string msg)
        {
            Log.WriteLine("OnAttackMessageReceived:" + msg);
            LastAction = DateTime.Now;
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            Log.WriteLine("OnNextTetrimino:" + index + " " + tetrimino);
            LastAction = DateTime.Now;
        }

        #endregion
    }
}
