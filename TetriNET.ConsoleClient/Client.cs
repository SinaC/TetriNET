using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common;
using TetriNET.Common.Contracts;

namespace TetriNET.Client
{
    public class Player
    {
        public string Name { get; set; }
        public byte[] Grid { get; set; }
    }

    public sealed class Client : ITetriNETCallback, IClient
    {
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;

        public enum States
        {
            Created,
            Registering,
            Registered,
            Playing
        }

        internal readonly IProxy _proxy; // TODO: set as private
        private readonly Player[] _players = new Player[6];
        private readonly ManualResetEvent _stopBackgroundTaskEvent = new ManualResetEvent(false);

        public States State { get; private set; }

        private List<WinEntry> _winList;
        private GameOptions _options;
        private List<Tetriminos> _tetriminos;
        private int _tetriminoIndex;
        private DateTime _lastHeartbeat;
        private int _playerId;
        private bool _isServerMaster;

        public string Name { get; set; } // TODO: private setter and method to set it only if not already registered

        public Client(Func<ITetriNETCallback, IProxy> createProxyFunc)
        {
            _proxy = createProxyFunc(this);
            _proxy.OnConnectionLost += ConnectionLostHandler;

            _lastHeartbeat = DateTime.Now.AddMilliseconds(-HeartbeatDelay);
            _playerId = -1;
            _isServerMaster = false;

            State = States.Created;

            Task.Factory.StartNew(BackgroundTask);
        }

        #region IProxy event handler

        private void ConnectionLostHandler()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITetriNETCallback

        public void OnHeartbeatReceived()
        {
            _proxy.ResetTimeout();
        }

        public void OnServerStopped()
        {
            ConnectionLostHandler();
        }

        public void OnPlayerRegistered(bool succeeded, int playerId, bool gameStarted)
        {
            _proxy.ResetTimeout();
            if (succeeded && State == States.Registering)
            {
                Log.WriteLine("Registered as player {0} game started {1}", playerId, gameStarted);

                // TODO: if gameStarted fill our screen with random blocks

                _playerId = playerId;
                _players[_playerId] = new Player
                    {
                        Name = Name
                    };
                State = States.Registered;
            }
            else
            {
                Log.WriteLine("Registration failed");
            }
        }

        public void OnPlayerJoined(int playerId, string name)
        {
            Log.WriteLine("Player {0}[{1}] joined", name, playerId);

            _proxy.ResetTimeout();
            if (playerId != _playerId && playerId >= 0) // don't update ourself
                _players[playerId] = new Player
                    {
                        Name = name
                    };
            // TODO: update chat list + display msg in out-game chat + fill player screen with random blocks if game started
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            Log.WriteLine("Player {0}[{1}] left ({2})", name, playerId, reason);

            _proxy.ResetTimeout();
            if (playerId != _playerId && playerId >= 0)
                _players[playerId] = null;
            // TODO: update chat list + display msg in out-game chat + fill player screen with random blocks if game started
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            Log.WriteLine("{0}:{1}", playerName, msg);

            _proxy.ResetTimeout();
            // TODO: display msg in out-game chat
        }

        public void OnPublishServerMessage(string msg)
        {
            Log.WriteLine("{0}", msg);

            _proxy.ResetTimeout();
            // TODO: display msg in out-game chat
        }

        public void OnPlayerLost(int playerId)
        {
            Log.WriteLine("Player [{0}] {1} has lost", playerId, _players[playerId]);

            _proxy.ResetTimeout();
            // TODO: fill player screen with random blocks + display msg in in-game chat + update player status
        }

        public void OnPlayerWon(int playerId)
        {
            Log.WriteLine("Player [{0}] {1} has won", playerId, _players[playerId]);

            _proxy.ResetTimeout();
            // TODO: display msg in in-game chat + out-game chat + update player status
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, GameOptions options)
        {
            Log.WriteLine("Game started");

            _proxy.ResetTimeout();
            State = States.Playing;
            _options = options;
            _tetriminos = new List<Tetriminos>
                {
                    firstTetrimino,
                    secondTetrimino
                };
            _tetriminoIndex = 0;
            // TODO: update current/next piece, clear every player screen
        }

        public void OnGameFinished()
        {
            Log.WriteLine("Game finished");

            _proxy.ResetTimeout();
            State = States.Registered;
            // TODO:
        }

        public void OnGamePaused()
        {
            Log.WriteLine("Game paused");

            _proxy.ResetTimeout();
            // TODO:
        }

        public void OnGameResumed()
        {
            Log.WriteLine("Game resumed");

            _proxy.ResetTimeout();
            // TODO:
        }

        public void OnServerAddLines(int lineCount)
        {
            Log.WriteLine("Server add {0} lines", lineCount);

            _proxy.ResetTimeout();
            // TODO
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            Log.WriteLine("Player {0} add {1} lines (special [{2}])", playerId, lineCount, specialId);

            _proxy.ResetTimeout();
            // TODO: perform attack
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            Log.WriteLine("Special {0}[{1}] from {2} to {3}", special, specialId, playerId, targetId);

            _proxy.ResetTimeout();
            // TODO: if targetId == own id, perform attack + display in-game msg
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            Log.WriteLine("Tetrimino {0}: {1}", index, tetrimino);

            _proxy.ResetTimeout();
            _tetriminos[index] = tetrimino;
            // TODO: update next piece if index == _tetriminoIndex+1
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            Log.WriteLine("Player [{0}] {1} modified", playerId, _players[playerId]);

            _proxy.ResetTimeout();
            if (_players[playerId] != null)
                _players[playerId].Grid = grid;
            // TODO: update player screen
        }

        public void OnServerMasterChanged(int playerId)
        {
            _proxy.ResetTimeout();
            if (playerId == _playerId)
            {
                Log.WriteLine("Yeehaw ... power is ours");

                _isServerMaster = true;
                // TODO: enable server settings, start/stop/pause/resume buttons, ...
            }
            else
            {
                Log.WriteLine("The power is for another one");

                _isServerMaster = false;
                // TODO: disable server settings, start/stop/pause/resume buttons, ...
            }
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            Log.WriteLine("Win list: {0}", winList.Select(x => String.Format("{0}:{1}", x.PlayerName, x.Score)).Aggregate((n, i) => n + "|" + i));

            _proxy.ResetTimeout();
            _winList = winList;
            // TODO: update win list
        }

        #endregion

        #region IClient

        #endregion

        private void BackgroundTask()
        {
            while (true)
            {
                if (State == States.Registered || State == States.Playing)
                {
                    // Check server timeout
                    TimeSpan timespan = DateTime.Now - _proxy.LastServerAction;
                    if (timespan.TotalMilliseconds > TimeoutDelay)
                    {
                        Log.WriteLine("Timeout++");
                        _proxy.SetTimeout();
                        if (_proxy.TimeoutCount >= MaxTimeoutCountBeforeDisconnection)
                            ConnectionLostHandler(); // timeout
                    }

                    // Send heartbeat if needed // TODO: reset this when sending a message
                    TimeSpan delayFromPreviousHeartbeat = DateTime.Now - _lastHeartbeat;
                    if (delayFromPreviousHeartbeat.TotalMilliseconds > HeartbeatDelay)
                    {
                        _proxy.Heartbeat(this);
                        _lastHeartbeat = DateTime.Now;
                    }

                    // Stop task if stop event is raised
                    if (_stopBackgroundTaskEvent.WaitOne(0))
                        break;
                }
            }
        }
    }
}
