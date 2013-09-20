//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.ServiceModel;
//using System.ServiceModel.Channels;
//using System.Threading;
//using System.Threading.Tasks;
//using TetriNET.Common;
//using TetriNET.Common.Contracts;
//using TetriNET.Common.GameDatas;
//using TetriNET.Common.WCF;
//using TetriNET.Logger;

//// TODO: use ResetTimeout in Proxy instead of Client
//namespace POC.Client_POC
//{
//    public sealed class WCFProxy : IProxy
//    {
//        private readonly IWCFTetriNET _proxy;

//        public DateTime LastServerAction { get; private set; }
//        public int TimeoutCount { get; private set; }

//        public WCFProxy(string address, ITetriNETCallback callback)
//        {
//            TimeoutCount = 0;
//            LastServerAction = DateTime.Now;

//            // Get WCF endpoint
//            EndpointAddress endpointAddress = null;
//            if (String.IsNullOrEmpty(address) || address.ToLower() == "auto")
//            {
//                Log.WriteLine("Searching IWCFTetriNET server");
//                List<EndpointAddress> endpointAddresses = DiscoveryHelper.DiscoverAddresses<IWCFTetriNET>();
//                if (endpointAddresses != null && endpointAddresses.Any())
//                {
//                    foreach (EndpointAddress endpoint in endpointAddresses)
//                        Log.WriteLine("{0}:\t{1}", endpointAddresses.IndexOf(endpoint), endpoint.Uri);
//                    Log.WriteLine("Selecting first server");
//                    endpointAddress = endpointAddresses[0];
//                }
//                else
//                {
//                    Log.WriteLine("No server found");
//                }
//            }
//            else
//                endpointAddress = new EndpointAddress(address);

//            // Create WCF proxy from endpoint
//            if (endpointAddress != null)
//            {
//                Log.WriteLine("Connecting to server:{0}", endpointAddress.Uri);
//                Binding binding = new NetTcpBinding(SecurityMode.None);
//                InstanceContext instanceContext = new InstanceContext(callback);
//                _proxy = DuplexChannelFactory<IWCFTetriNET>.CreateChannel(instanceContext, binding, endpointAddress);
//            }
//        }

//        public void ResetTimeout()
//        {
//            TimeoutCount = 0;
//            LastServerAction = DateTime.Now;
//        }

//        public void SetTimeout()
//        {
//            TimeoutCount++;
//            LastServerAction = DateTime.Now;
//        }

//        private void ExceptionFreeAction(Action action, string actionName)
//        {
//            try
//            {
//                action();
//            }
//            catch(Exception ex)
//            {
//                Log.WriteLine("Exception:{0} {1}", actionName, ex);
//                if (OnConnectionLost != null)
//                    OnConnectionLost();
//            }
//        }

//        #region IProxy

//        public event ConnectionLostHandler OnConnectionLost;

//        public void RegisterPlayer(ITetriNETCallback callback, string playerName)
//        {
//            ExceptionFreeAction(() => _proxy.RegisterPlayer(playerName), "RegisterPlayer");
//        }

//        public void UnregisterPlayer(ITetriNETCallback callback)
//        {
//            ExceptionFreeAction(_proxy.UnregisterPlayer, "UnregisterPlayer");
//        }

//        public void Heartbeat(ITetriNETCallback callback)
//        {
//            ExceptionFreeAction(_proxy.Heartbeat, "Heartbeat");
//        }

//        public void PublishMessage(ITetriNETCallback callback, string msg)
//        {
//            ExceptionFreeAction(() => _proxy.PublishMessage(msg), "PublishMessage");
//        }

//        public void PlacePiece(ITetriNETCallback callback, int index, Tetriminos tetrimino, Orientations orientation, Position position, byte[] grid)
//        {
//            ExceptionFreeAction(() => _proxy.PlacePiece(index, tetrimino, orientation, position, grid), "PlacePiece");
//        }

//        public void ModifyGrid(ITetriNETCallback callback, byte[] grid)
//        {
//            ExceptionFreeAction(() => _proxy.ModifyGrid(grid), "ModifyGrid");
//        }

//        public void UseSpecial(ITetriNETCallback callback, int targetId, Specials special)
//        {
//            ExceptionFreeAction(() => _proxy.UseSpecial(targetId, special), "UseSpecial");
//        }

//        public void SendLines(ITetriNETCallback callback, int count)
//        {
//            ExceptionFreeAction(() => _proxy.SendLines(count), "SendLines");
//        }

//        public void StartGame(ITetriNETCallback callback)
//        {
//            ExceptionFreeAction(_proxy.StartGame, "StartGame");
//        }

//        public void StopGame(ITetriNETCallback callback)
//        {
//            ExceptionFreeAction(_proxy.StopGame, "StopGame");
//        }

//        public void PauseGame(ITetriNETCallback callback)
//        {
//            ExceptionFreeAction(_proxy.PauseGame, "PauseGame");
//        }

//        public void ResumeGame(ITetriNETCallback callback)
//        {
//            ExceptionFreeAction(_proxy.ResumeGame, "ResumeGame");
//        }

//        public void GameLost(ITetriNETCallback callback)
//        {
//            ExceptionFreeAction(_proxy.GameLost, "ResumeGame");
//        }

//        public void ChangeOptions(ITetriNETCallback callback, GameOptions options)
//        {
//            ExceptionFreeAction(() => _proxy.ChangeOptions(options), "ChangeOptions");
//        }

//        public void KickPlayer(ITetriNETCallback callback, int playerId)
//        {
//            ExceptionFreeAction(() => _proxy.KickPlayer(playerId), "KickPlayer");
//        }
//        public void BanPlayer(ITetriNETCallback callback, int playerId)
//        {
//            ExceptionFreeAction(() => _proxy.BanPlayer(playerId), "BanPlayer");
//        }

//        public void ResetWinList(ITetriNETCallback callback)
//        {
//            ExceptionFreeAction(_proxy.ResetWinList, "ResetWinList");
//        }

//        #endregion

//    }

//    public sealed class Client : ITetriNETCallback
//    {
//        private const int HeartbeatDelay = 300; // in ms
//        private const int TimeoutDelay = 500; // in ms
//        private const int MaxTimeoutCountBeforeDisconnection = 3;

//        internal readonly IProxy _proxy; // TODO: set as private
//        private readonly string[] _players = new string[6];
//        private readonly ManualResetEvent _stopBackgroundTaskEvent = new ManualResetEvent(false);

//        private List<WinEntry> _winList;
//        private GameOptions _options;
//        private List<Tetriminos> _tetriminos;
//        private int _tetriminoIndex;
//        private DateTime _lastHeartbeat;
//        private bool _registered; // TODO: set to false before calling RegisterPlayer
//        private int _playerId;

//        public string Name { get; set; } // TODO: private setter and method to set it only if not already registered

//        public Client(Func<ITetriNETCallback, IProxy> createProxyFunc )
//        {
//            _proxy = createProxyFunc(this);
//            _proxy.OnConnectionLost += ConnectionLostHandler;

//            _lastHeartbeat = DateTime.Now.AddMilliseconds(-HeartbeatDelay);
//            _playerId = -1;
//            _registered = false;

//            Task.Factory.StartNew(BackgroundTask);
//        }

//        #region IProxy event handler

//        private void ConnectionLostHandler()
//        {
//            throw new NotImplementedException();
//        }

//        #endregion

//        #region ITetriNETCallback

//        public void OnHeartbeatReceived()
//        {
//            _proxy.ResetTimeout();
//        }

//        public void OnServerStopped()
//        {
//            ConnectionLostHandler();
//        }

//        public void OnPlayerRegistered(bool succeeded, int playerId, bool gameStarted)
//        {
//            _proxy.ResetTimeout();
//            if (succeeded)
//            {
//                Log.WriteLine("Registered as player {0} game started {1}", playerId, gameStarted);

//                // TODO: if gameStarted fill our screen with random blocks

//                _playerId = playerId;
//                _players[_playerId] = Name;
//                _registered = true;
//            }
//            else
//            {
//                Log.WriteLine("Registration failed");

//                _registered = false;
//            }
//        }

//        public void OnPlayerJoined(int playerId, string name)
//        {
//            Log.WriteLine("Player {0}[{1}] joined", name, playerId);

//            _proxy.ResetTimeout();
//            if (playerId != _playerId && playerId >= 0) // don't update ourself
//                _players[playerId] = name;
//            // TODO: update chat list + display msg in out-game chat + fill player screen with random blocks if game started
//        }

//        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
//        {
//            Log.WriteLine("Player {0}[{1}] left ({2})", name, playerId, reason);

//            _proxy.ResetTimeout();
//            if (playerId != _playerId && playerId >= 0)
//                _players[playerId] = null;
//            // TODO: update chat list + display msg in out-game chat + fill player screen with random blocks if game started
//        }

//        public void OnPublishPlayerMessage(string playerName, string msg)
//        {
//            Log.WriteLine("{0}:{1}", playerName, msg);

//            _proxy.ResetTimeout();
//            // TODO: display msg in out-game chat
//        }

//        public void OnPublishServerMessage(string msg)
//        {
//            Log.WriteLine("{0}", msg);

//            _proxy.ResetTimeout();
//            // TODO: display msg in out-game chat
//        }

//        public void OnPlayerLost(int playerId)
//        {
//            Log.WriteLine("Player [{0}] {1} has lost", playerId, _players[playerId]);

//            _proxy.ResetTimeout();
//            // TODO: fill player screen with random blocks + display msg in in-game chat + update player status
//        }

//        public void OnPlayerWon(int playerId)
//        {
//            Log.WriteLine("Player [{0}] {1} has won", playerId, _players[playerId]);

//            _proxy.ResetTimeout();
//            // TODO: display msg in in-game chat + out-game chat + update player status
//        }

//        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, Tetriminos thirdTetrimino, GameOptions options)
//        {
//            Log.WriteLine("Game started");

//            _proxy.ResetTimeout();
//            _options = options;
//            _tetriminos = new List<Tetriminos>
//                {
//                    firstTetrimino,
//                    secondTetrimino,
//                    thirdTetrimino
//                };
//            _tetriminoIndex = 0;
//            // TODO: update current/next piece, clear every player screen
//        }

//        public void OnGameFinished()
//        {
//            Log.WriteLine("Game finished");

//            _proxy.ResetTimeout();
//            // TODO:
//        }

//        public void OnGamePaused()
//        {
//            Log.WriteLine("Game paused");

//            _proxy.ResetTimeout();
//            // TODO:
//        }

//        public void OnGameResumed()
//        {
//            Log.WriteLine("Game resumed");

//            _proxy.ResetTimeout();
//            // TODO:
//        }

//        public void OnServerAddLines(int lineCount)
//        {
//            Log.WriteLine("Server add {0} lines", lineCount);

//            _proxy.ResetTimeout();
//            // TODO
//        }

//        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
//        {
//            Log.WriteLine("Player {0} add {1} lines (special [{2}])", playerId, lineCount, specialId);

//            _proxy.ResetTimeout();
//            // TODO: perform attack
//        }

//        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
//        {
//            Log.WriteLine("Special {0}[{1}] from {2} to {3}", special, specialId, playerId, targetId);

//            _proxy.ResetTimeout();
//            // TODO: if targetId == own id, perform attack + display in-game msg
//        }

//        public void OnNextPiece(int index, Tetriminos tetrimino)
//        {
//            Log.WriteLine("Tetrimino {0}: {1}", index, tetrimino);

//            _proxy.ResetTimeout();
//            _tetriminos[index] = tetrimino;
//            // TODO: update next piece if index == _tetriminoIndex+1
//        }

//        public void OnGridModified(int playerId, byte[] grid)
//        {
//            Log.WriteLine("Player [{0}] {1} modified", playerId, _players[playerId]);

//            _proxy.ResetTimeout();
//            // TODO: update player screen
//        }

//        public void OnServerMasterChanged(int playerId)
//        {
//            _proxy.ResetTimeout();
//            if (playerId == _playerId)
//            {
//                Log.WriteLine("Yeehaw ... power is ours");
//                // TODO: enable server settings, start/stop/pause/resume buttons, ...
//            }
//            else
//            {
//                Log.WriteLine("The power is for another one");
//                // TODO: disable server settings, start/stop/pause/resume buttons, ...
//            }
//        }

//        public void OnWinListModified(List<WinEntry> winList)
//        {
//            Log.WriteLine("Win list: {0}", winList.Select(x => String.Format("{0}:{1}", x.PlayerName, x.Score)).Aggregate((n, i) => n + "|" + i));

//            _proxy.ResetTimeout();
//            _winList = winList;
//            // TODO: update win list
//        }
        
//        #endregion

//        private void BackgroundTask()
//        {
//            while (true)
//            {
//                if (_registered)
//                {
//                    // Check server timeout
//                    TimeSpan timespan = DateTime.Now - _proxy.LastServerAction;
//                    if (timespan.TotalMilliseconds > TimeoutDelay)
//                    {
//                        Log.WriteLine("Timeout++");
//                        _proxy.SetTimeout();
//                        if (_proxy.TimeoutCount >= MaxTimeoutCountBeforeDisconnection)
//                            ConnectionLostHandler(); // timeout
//                    }

//                    // Send heartbeat if needed // TODO: reset this when sending a message
//                    TimeSpan delayFromPreviousHeartbeat = DateTime.Now - _lastHeartbeat;
//                    if (delayFromPreviousHeartbeat.TotalMilliseconds > HeartbeatDelay)
//                    {
//                        _proxy.Heartbeat(this);
//                        _lastHeartbeat = DateTime.Now;
//                    }

//                    // Stop task if stop event is raised
//                    if (_stopBackgroundTaskEvent.WaitOne(0))
//                        break;
//                }
//            }
//        }
//    }
//}
