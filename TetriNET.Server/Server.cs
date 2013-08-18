using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common;
using TetriNET.Common.Helpers;
using TetriNET.Server.Ban;
using TetriNET.Server.Host;
using TetriNET.Server.Player;

namespace TetriNET.Server
{
    public sealed class Server
    {
        public enum States
        {
            WaitingStartServer, // -> StartingServer
            StartingServer, // -> WaitingStartGame
            WaitingStartGame, // -> StartingGame
            StartingGame, // -> GameStarted
            GameStarted, // -> GameFinished | GamePaused
            GameFinished, // -> WaitingStartGame
            StoppingServer, // -> WaitingStartServer

            GamePaused, // GameStarted
        }

        private const int InactivityTimeoutBeforePing = 500; // in ms

        private readonly TetriminoQueue _tetriminoQueue = new TetriminoQueue();
        private readonly ManualResetEvent _stopActionTaskEvent = new ManualResetEvent(false);
        private readonly IPlayerManager _playerManager;
        private readonly List<IHost> _hosts;

        private List<WinEntry> _winList;

        private GameOptions _options;
        
        public States State { get; private set; }
        public int SpecialId { get; private set; }

        public Server(IPlayerManager playerManager, params IHost[] hosts)
        {
            if (hosts == null)
                throw new ArgumentNullException("hosts");

            _playerManager = playerManager;
            _hosts = hosts.ToList();
            _options = new GameOptions(); // TODO: get options from save file
            _winList = new List<WinEntry>(); // TODO: get win list from save file

            foreach (IHost host in _hosts)
            {
                host.OnPlayerRegistered += RegisterPlayerHandler;
                host.OnPlayerUnregistered += UnregisterPlayerHandler;
                host.OnMessagePublished += PublishMessageHandler;
                host.OnTetriminoPlaced += PlaceTetriminoHandler;
                host.OnUseSpecial += UseSpecialHandler;
                host.OnSendLines += SendLinesHandler;
                host.OnGridModified += ModifyGridHandler;
                host.OnStartGame += StartGameHandler;
                host.OnStopGame += StopGameHandler;
                host.OnPauseGame += PauseGameHandler;
                host.OnResumeGame += ResumeGameHandler;
                host.OnGameLost += GameLostHandler;
                host.OnChangeOptions += ChangeOptionsHandler;
                host.OnKickPlayer += KickPlayerHandler;
                host.OnBanPlayer += BanPlayerHandler;
                host.OnResetWinList += ResetWinListHandler;
                
                host.OnPlayerLeft += PlayerLeftHandler;

                Debug.Assert(Check.CheckEvents(host), "Every host events must be handled");
            }

            SpecialId = 0;

            Task.Factory.StartNew(TaskResolveGameActions);

            State = States.WaitingStartServer;
        }

        public void StartServer()
        {
            Log.WriteLine("Starting server");

            State = States.StartingServer;

            // Start hosts
            foreach (IHost host in _hosts)
                host.Start();

            State = States.WaitingStartGame;

            Log.WriteLine("Server started");
        }

        public void StopServer()
        {
            Log.WriteLine("Stopping server");
            State = States.StoppingServer;

            // Inform players
            foreach(IPlayer p in _playerManager.Players)
                p.OnServerStopped();

            // Stop hosts
            foreach (IHost host in _hosts)
                host.Stop();

            // Clear player manager
            _playerManager.Clear();

            State = States.WaitingStartServer;

            Log.WriteLine("Server stopped");
        }

        // Start game and stop game should be removed and code included in StartGameHandler and StopGameHandler
        public void StartGame()
        {
            Log.WriteLine("Starting game");

            if (State == States.WaitingStartGame)
            {
                State = States.GameStarted;

                // Reset Tetrimino Queue
                _tetriminoQueue.Reset(); // TODO: random seed
                Tetriminos firstTetrimino = _tetriminoQueue[0];
                Tetriminos secondTetrimino = _tetriminoQueue[1];
                // Send start game to every connected player
                foreach (IPlayer p in _playerManager.Players)
                {
                    p.TetriminoIndex = 0;
                    p.State = PlayerStates.Playing;
                    p.LossTime = DateTime.MaxValue;
                    p.OnGameStarted(firstTetrimino, secondTetrimino, _options);
                }

                Log.WriteLine("Game started");
            }
            else
                Log.WriteLine("Cannot start game");
        }

        public void StopGame()
        {
            Log.WriteLine("Stopping game");

            if (State == States.GameStarted || State == States.GamePaused)
            {
                State = States.GameFinished;

                // Send start game to every connected player
                foreach (IPlayer p in _playerManager.Players)
                    p.OnGameFinished();

                State = States.WaitingStartGame;

                Log.WriteLine("Game stopped");
            }
            else
                Log.WriteLine("Cannot stop game");
        }

        private void UpdateWinList(string playerName, int score)
        {
            WinEntry entry = _winList.SingleOrDefault(x => x.PlayerName == playerName);
            if (entry == null)
            {
                entry = new WinEntry
                {
                    PlayerName = playerName,
                    Score = 0
                };
                _winList.Add(entry);
            }
            entry.Score = score;
        }

        #region IHost event handler
        private void RegisterPlayerHandler(IPlayer player, int playerId)
        {
            Log.WriteLine("New player:[{0}]{1}", playerId, player.Name);

            // Send player id back to player
            player.OnPlayerRegistered(true, playerId, State == States.GameStarted);

            // Inform players about new played connected
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnPlayerJoined(playerId, player.Name);

            // Send new server master id
            IPlayer serverMaster = _playerManager.ServerMaster;
            if (serverMaster != null && player == serverMaster)
            {
                int serverMasterId = _playerManager.GetId(serverMaster);
                foreach (IPlayer p in _playerManager.Players)
                    p.OnServerMasterChanged(serverMasterId);
            }
        }

        private void UnregisterPlayerHandler(IPlayer player)
        {
            Log.WriteLine("Unregister player:{0}", player.Name);

            PlayerLeftHandler(player, LeaveReasons.Disconnected);
        }

        private void PublishMessageHandler(IPlayer player, string msg)
        {
            Log.WriteLine("PublishMessage:{0}:{1}", player.Name, msg);

            // Send message to players
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnPublishPlayerMessage(player.Name, msg);
        }

        private void PlaceTetriminoHandler(IPlayer player, int index, Tetriminos tetrimino, Orientations orientation, Position position, byte[] grid)
        {
            if (State == States.GameStarted)
                _actionQueue.Enqueue(() => PlaceTetrimino(player, index, tetrimino, orientation, position, grid));
        }

        private void UseSpecialHandler(IPlayer player, IPlayer target, Specials special)
        {
            if (State == States.GameStarted)
                _actionQueue.Enqueue(() => Special(player, target, special));
        }

        private void SendLinesHandler(IPlayer player, int count)
        {
            if (State == States.GameStarted)
                _actionQueue.Enqueue(() => SendLines(player, count));
        }

        private void ModifyGridHandler(IPlayer player, byte[] grid)
        {
            if (State == States.GameStarted)
                _actionQueue.Enqueue(() => ModifyGrid(player, grid));
        }

        private void StartGameHandler(IPlayer player)
        {
            Log.WriteLine("StartGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
                StartGame();
        }

        private void StopGameHandler(IPlayer player)
        {
            Log.WriteLine("StopGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
                StopGame();
        }

        private void PauseGameHandler(IPlayer player)
        {
            Log.WriteLine("PauseGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player && State == States.GameStarted)
            {
                State = States.GamePaused;

                // Send pause to players
                foreach (IPlayer p in _playerManager.Players)
                    p.OnGamePaused();

                Log.WriteLine("Game paused");
            }
            else
                Log.WriteLine("Cannot pause game");
        }

        private void ResumeGameHandler(IPlayer player)
        {
            Log.WriteLine("ResumeGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player && State == States.GamePaused)
            {
                State = States.GameStarted;

                // Send resume to players
                foreach (IPlayer p in _playerManager.Players)
                    p.OnGameResumed();

                Log.WriteLine("Game resumed");
            }
            else
                Log.WriteLine("Cannot resume game");
        }

        private void GameLostHandler(IPlayer player)
        {
            Log.WriteLine("GameLost:{0}", player.Name);

            if (player.State == PlayerStates.Playing)
            {
                // Set player state
                player.State = PlayerStates.GameLost;
                player.LossTime = DateTime.Now;

                // Inform players
                int id = _playerManager.GetId(player);
                foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                    p.OnPlayerLost(id);

                //
                int playingCount = _playerManager.Players.Count(p => p.State == PlayerStates.Playing);
                if (playingCount == 0) // there were only one playing player
                {
                    // Send game finished (no winner)
                    foreach (IPlayer p in _playerManager.Players)
                        p.OnGameFinished();
                }
                else if (playingCount == _playerManager.PlayerCount - 1) // only one playing left
                {
                    // Game won
                    IPlayer winner = _playerManager.Players.Single(p => p.State == PlayerStates.Playing);
                    int winnerId = _playerManager.GetId(winner);

                    // Update win list
                    UpdateWinList(winner.Name, 3);
                    int points = 2;
                    foreach (IPlayer p in _playerManager.Players.Where(x => x.State == PlayerStates.GameLost).OrderByDescending(x => x.LossTime))
                    {
                        UpdateWinList(p.Name, points);
                        points--;
                        if (points == 0)
                            break;
                    }

                    // Send game finished, winner and win list
                    foreach (IPlayer p in _playerManager.Players)
                    {
                        p.OnGameFinished();
                        p.OnPlayerWon(winnerId);
                        p.OnWinListModified(_winList);
                    }
                }
            }
        }

        private void ChangeOptionsHandler(IPlayer player, GameOptions options)
        {
            Log.WriteLine("ChangeOptionsHandler:{0} {1}", player.Name, options);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player && State == States.WaitingStartGame)
            {
                _options = options; // Options will be sent to players when starting a new game
            }
            else
                Log.WriteLine("Cannot change options");
        }

        private void KickPlayerHandler(IPlayer player, int playerId)
        {
            Log.WriteLine("KickPlayer:{0} [{1}]", player.Name, playerId);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player && State == States.WaitingStartGame)
            {
                // Send server stopped
                player.OnServerStopped();
                // Remove player from player manager and hosts + warn other players
                PlayerLeftHandler(player, LeaveReasons.Kick);
            }
            else
                Log.WriteLine("Cannot kick player");
        }

        private void BanPlayerHandler(IPlayer player, int playerId)
        {
            Log.WriteLine("BanPlayer:{0} [{1}]", player.Name, playerId);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player && State == States.WaitingStartGame)
            {
                // Send server stopped
                player.OnServerStopped();
                // Remove player from player manager and hosts + warn other players
                PlayerLeftHandler(player, LeaveReasons.Ban);
                // TODO: add to ban list
            }
            else
                Log.WriteLine("Cannot ban player");
        }

        private void ResetWinListHandler(IPlayer player)
        {
            Log.WriteLine("ResetWinLost:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player && State == States.WaitingStartGame)
            {
                // Reset
                _winList = new List<WinEntry>();

                // Inform player
                foreach (IPlayer p in _playerManager.Players)
                {
                    p.OnPublishServerMessage("Win list has been resetted");
                    p.OnWinListModified(_winList);
                }
            }
            else
                Log.WriteLine("Cannot reset win list");
        }

        private void PlayerLeftHandler(IPlayer player, LeaveReasons reason)
        {
            Log.WriteLine("Player left:{0} {1}", player.Name, reason);

            bool wasServerMaster = false;
            // Remove from player manager
            int id;
            lock (_playerManager.LockObject)
            {
                if (player == _playerManager.ServerMaster)
                    wasServerMaster = true;
                // Get id
                id = _playerManager.GetId(player);
                // Remove player from player list
                _playerManager.Remove(player);
            }

            // Clean host tables
            foreach(IHost host in _hosts)
                host.RemovePlayer(player);

            // Inform players except disconnected player
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnPlayerLeft(id, player.Name, reason);

            // Send new server master id
            if (wasServerMaster)
            {
                IPlayer serverMaster = _playerManager.ServerMaster;
                if (serverMaster != null)
                {
                    int serverMasterId = _playerManager.GetId(serverMaster);
                    foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                        p.OnServerMasterChanged(serverMasterId);
                }
            }
        }
        #endregion

        #region Tetrimino queue

        private class TetriminoQueue
        {
            private int _tetriminosCount;
            private readonly object _lock = new object();
            private int _size;
            private int[] _array;
            private Random _random;

            public void Reset(int seed = 0)
            {
                _tetriminosCount = Enum.GetValues(typeof(Tetriminos)).Length;
                _random = new Random(seed);
                lock (_lock)
                {
                    Grow(1);
                }
            }

            public Tetriminos this[int index]
            {
                get
                {
                    Tetriminos tetrimino;
                    lock (_lock)
                    {
                        if (index >= _size)
                            Grow(128);
                        tetrimino = (Tetriminos)_array[index];
                    }
                    return tetrimino;
                }
            }

            private void Grow(int increment)
            {
                int newSize = _size + increment;
                int[] newArray = new int[newSize];
                if (_size > 0)
                    Array.Copy(_array, newArray, _size);
                for (int i = _size; i < newSize; i++)
                    newArray[i] = _random.Next(_tetriminosCount);
                _array = newArray;
                _size = newSize;
            }
        }

        #endregion

        #region Game action queue

        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

        private void TaskResolveGameActions()
        {
            while (true)
            {
                if (State == States.GameStarted && !_actionQueue.IsEmpty)
                {
                    Action action;
                    bool dequeue = _actionQueue.TryDequeue(out action);
                    if (dequeue)
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine("Exception raised in TaskResolveGameActions. Exception:{0}", ex);
                        }
                    }
                }
                // TODO: hearbeat only player on each loop ?
                foreach (IPlayer p in _playerManager.Players)
                {
                    TimeSpan timespan = DateTime.Now - p.LastAction;
                    if (timespan.TotalMilliseconds > InactivityTimeoutBeforePing)
                        p.OnHeartbeatReceived(); // Check if player is alive
                    // TODO: timeout count on player, if count > MaxTimeoutCount -> player left reason Timeout
                }
                if (_stopActionTaskEvent.WaitOne(0))
                    break;
            }
        }
        
        #endregion

        #region Game actions

        private void PlaceTetrimino(IPlayer player, int index, Tetriminos tetrimino, Orientations orientation, Position position, byte[] grid)
        {
            Log.WriteLine("PlaceTetrimino[{0}]{1}:{2} {3} at {4},{5} {6}", player.Name, index, tetrimino, orientation, position.X, position.Y, grid.Count(x => x > 0));

            // TODO: check if index is equal to player.TetriminoIndex
            if (index != player.TetriminoIndex)
                Log.WriteLine("!!!! tetrimino index different for player {0} local {1} remove {2}", player.Name, player.TetriminoIndex, index);

            // Set grid
            player.Grid = grid;
            // Get next piece
            player.TetriminoIndex++;
            Tetriminos nextTetrimino = _tetriminoQueue[player.TetriminoIndex];
            // Send next piece
            player.OnNextTetrimino(player.TetriminoIndex, nextTetrimino);
        }

        private void Special(IPlayer player, IPlayer target, Specials special)
        {
            Log.WriteLine("UseSpecial[{0}][{1}]{2}", player.Name, target.Name, special);

            //
            int playerId = _playerManager.GetId(player);
            int targetId = _playerManager.GetId(target);
            // Store special id locally
            int specialId = SpecialId;
            // Increment special
            SpecialId++;
            // If special is Switch, call OnGridModified with switched grids
            if (special == Specials.Switch)
            {
                // Send grid to player and target
                target.OnGridModified(targetId, player.Grid);
                player.OnGridModified(playerId, target.Grid);
                // Switch locally
                byte[] tmp = target.Grid;
                target.Grid = player.Grid;
                player.Grid = tmp;
            }
            // Inform about special use
            foreach (IPlayer p in _playerManager.Players)
                p.OnSpecialUsed(specialId, playerId, targetId, special);
        }

        private void SendLines(IPlayer player, int count)
        {
            Log.WriteLine("SendLines[{0}]:{1}", player, count);

            //
            int playerId = _playerManager.GetId(player);
            // Store special id locally
            int specialId = SpecialId;
            // Increment special id
            SpecialId++;
            // Send lines to everyone except sender
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnPlayerAddLines(specialId, playerId, count);
        }

        private void ModifyGrid(IPlayer player, byte[] grid)
        {
            Log.WriteLine("ModifyGrid");

            // Set grid
            player.Grid = grid;
            // Get id
            int id = _playerManager.GetId(player);
            // Send grid modification to everyone except sender
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnGridModified(id, player.Grid);
        }

        #endregion
    }
}
