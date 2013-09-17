using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Randomizer;

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

            GamePaused, // -> GameStarted
        }

        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const bool IsTimeoutDetectionActive = false;

        private readonly TetriminoQueue _tetriminoQueue;
        private readonly ManualResetEvent _stopBackgroundTaskEvent = new ManualResetEvent(false);
        private readonly IPlayerManager _playerManager;
        private readonly List<IHost> _hosts;

        private GameOptions _options;

        private bool _isSuddenDeathActive;
        private DateTime _suddenDeathStartTime;
        private DateTime _lastSuddenDeathAddLines;

        public States State { get; private set; }
        public int SpecialId { get; private set; }
        public List<WinEntry> WinList { get; private set; }

        public Server(IPlayerManager playerManager, params IHost[] hosts)
        {
            if (playerManager == null)
                throw new ArgumentNullException("playerManager");
            if (hosts == null)
                throw new ArgumentNullException("hosts");
            _options = new GameOptions(); // TODO: get options from save file

            _tetriminoQueue = new TetriminoQueue(() => RangeRandom.Random(_options.TetriminoOccurancies));
            _playerManager = playerManager;
            _hosts = hosts.ToList();
            
            WinList = new List<WinEntry>(); // TODO: get win list from save file

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

            Task.Factory.StartNew(TimeoutTask);
            Task.Factory.StartNew(GameActionsTask);

            State = States.WaitingStartServer;
        }

        public void StartServer()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Starting server");

            State = States.StartingServer;

            // Start hosts
            foreach (IHost host in _hosts)
                host.Start();

            State = States.WaitingStartGame;

            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Server started");
        }

        public void StopServer()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Stopping server");
            State = States.StoppingServer;

            // Inform players
            foreach (IPlayer p in _playerManager.Players)
                p.OnServerStopped();

            // Stop hosts
            foreach (IHost host in _hosts)
                host.Stop();

            // Clear player manager
            _playerManager.Clear();

            State = States.WaitingStartServer;

            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Server stopped");
        }

        public void StartGame()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Starting game");

            if (State == States.WaitingStartGame)
            {
                // Reset action list
                Action outAction;
                while (!_gameActionQueue.IsEmpty)
                    _gameActionQueue.TryDequeue(out outAction);

                // Reset special id
                SpecialId = 0;

                // Reset Tetrimino Queue
                _tetriminoQueue.Reset(); // TODO: random seed
                Tetriminos firstTetrimino = _tetriminoQueue[0];
                Tetriminos secondTetrimino = _tetriminoQueue[1];
                Tetriminos thirdTetrimino = _tetriminoQueue[2];

                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Starting game with {0} {1} {2}", firstTetrimino, secondTetrimino, thirdTetrimino);

                // Reset sudden death
                _isSuddenDeathActive = false;
                if (_options.DelayBeforeSuddenDeath > 0)
                {
                    _suddenDeathStartTime = DateTime.Now.AddMinutes(_options.DelayBeforeSuddenDeath);
                    _isSuddenDeathActive = true;
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Sudden death will be activated after {0} minutes and send lines every {1} seconds", _options.DelayBeforeSuddenDeath, _options.SuddenDeathTick);
                }

                // Send start game to every connected player
                foreach (IPlayer p in _playerManager.Players)
                {
                    p.TetriminoIndex = 0;
                    p.State = PlayerStates.Playing;
                    p.LossTime = DateTime.MaxValue;
                    p.OnGameStarted(firstTetrimino, secondTetrimino, thirdTetrimino, _options);
                }
                State = States.GameStarted;

                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Game started");
            }
            else
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Cannot start game");
        }

        public void StopGame()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Stopping game");

            if (State == States.GameStarted || State == States.GamePaused)
            {
                State = States.GameFinished;

                // Send start game to every connected player
                foreach (IPlayer p in _playerManager.Players)
                    p.OnGameFinished();

                State = States.WaitingStartGame;

                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Game stopped");
            }
            else
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Cannot stop game");
        }

        public void PauseGame()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Pausing game");

            if (State == States.GameStarted)
            {
                State = States.GamePaused;

                // Send pause to players
                foreach (IPlayer p in _playerManager.Players)
                    p.OnGamePaused();

                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Game paused");
            }
            else
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Cannot pause game");
        }

        public void ResumeGame()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Resuming game");
            if (State == States.GamePaused)
            {
                State = States.GameStarted;

                // Reset sudden death
                _lastSuddenDeathAddLines = DateTime.Now; // TODO: a player, could pause<->resume forever to avoid sudden death

                // Send resume to players
                foreach (IPlayer p in _playerManager.Players)
                    p.OnGameResumed();

                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Game resumed");
            }
            else
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Cannot resume game");
        }

        public void ResetWinList()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Resetting win list");

            if (State == States.WaitingStartGame)
            {
                // Reset
                WinList.Clear();

                // Inform player
                IPlayer serverMaster = _playerManager.ServerMaster;
                foreach (IPlayer p in _playerManager.Players)
                {
                    p.OnPublishServerMessage(String.Format("Win list resetted by {0}", serverMaster == null ? "SERVER" : serverMaster.Name));
                    p.OnWinListModified(WinList);
                }
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Win list resetted");
            }
            else
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Cannot reset win list");
        }

        public void ToggleSuddenDeath()
        {
            if (_options.DelayBeforeSuddenDeath == 0)
            {
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Activating SUDDEN DEATH");
                _isSuddenDeathActive = true;
                _options.DelayBeforeSuddenDeath = 1;
                _options.SuddenDeathTick = 1;
                _suddenDeathStartTime = DateTime.Now.AddMinutes(-_options.DelayBeforeSuddenDeath);
            }
            else
            {
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Disabling SUDDEN DEATH");
                _isSuddenDeathActive = false;
                _options.DelayBeforeSuddenDeath = 0;
                _options.SuddenDeathTick = 0;
            }
        }

        private void UpdateWinList(string playerName, int score)
        {
            WinEntry entry = WinList.SingleOrDefault(x => x.PlayerName == playerName);
            if (entry == null)
            {
                entry = new WinEntry
                {
                    PlayerName = playerName,
                    Score = 0
                };
                WinList.Add(entry);
            }
            entry.Score += score;
        }

        #region IHost event handler

        private void RegisterPlayerHandler(IPlayer player, int playerId)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "New player:[{0}]{1}", playerId, player.Name);

            // Send player id back to player
            player.OnPlayerRegistered(true, playerId, State == States.GameStarted || State == States.GamePaused);

            // Inform new player about other players
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                player.OnPlayerJoined(_playerManager.GetId(p), p.Name);

            // Inform players about new played connected
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnPlayerJoined(playerId, player.Name);

            // Server master
            IPlayer serverMaster = _playerManager.ServerMaster;
            if (serverMaster != null)
            {
                int serverMasterId = _playerManager.GetId(serverMaster);
                // Send new server master id
                if (serverMaster == player)
                    foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                        p.OnServerMasterChanged(serverMasterId);
                // Send server master id to player even if not modified
                player.OnServerMasterChanged(serverMasterId);
            }
        }

        private void UnregisterPlayerHandler(IPlayer player)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Unregister player:{0}", player.Name);

            PlayerLeftHandler(player, LeaveReasons.Disconnected);
        }

        private void PublishMessageHandler(IPlayer player, string msg)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "PublishMessage:{0}:{1}", player.Name, msg);

            // Send message to players
            //foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
            foreach (IPlayer p in _playerManager.Players)
                p.OnPublishPlayerMessage(player.Name, msg);
        }

        private void PlaceTetriminoHandler(IPlayer player, int index, Tetriminos tetrimino, int orientation, int posX, int posY, byte[] grid)
        {
            EnqueueAction(() => PlaceTetrimino(player, index, tetrimino, orientation, posX, posY, grid));
        }

        private void UseSpecialHandler(IPlayer player, IPlayer target, Specials special)
        {
            EnqueueAction(() => Special(player, target, special));
        }

        private void SendLinesHandler(IPlayer player, int count)
        {
            EnqueueAction(() => SendLines(player, count));
        }

        private void ModifyGridHandler(IPlayer player, byte[] grid)
        {
            EnqueueAction(() => ModifyGrid(player, grid));
        }

        private void StartGameHandler(IPlayer player)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "StartGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                StartGame();
            }
        }

        private void StopGameHandler(IPlayer player)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "StopGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                StopGame();
            }
        }

        private void PauseGameHandler(IPlayer player)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "PauseGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                PauseGame();
            }
        }

        private void ResumeGameHandler(IPlayer player)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "ResumeGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                ResumeGame();
            }
        }

        private void GameLostHandler(IPlayer player)
        {
            EnqueueAction(() => GameLost(player));
        }

        private void ChangeOptionsHandler(IPlayer player, GameOptions options)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "ChangeOptionsHandler:{0} {1}", player.Name, options);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player && State == States.WaitingStartGame)
            {
                // Check options before accepting them
                bool accepted =
                    RangeRandom.SumOccurancies(options.TetriminoOccurancies) == 100 &&
                    RangeRandom.SumOccurancies(options.SpecialOccurancies) == 100 &&
                    options.InventorySize >= 1 && options.InventorySize <= 15 &&
                    options.LinesToMakeForSpecials >= 1 && options.LinesToMakeForSpecials <= 4 &&
                    options.SpecialsAddedEachTime >= 1 && options.SpecialsAddedEachTime <= 4 &&
                    options.DelayBeforeSuddenDeath >= 0 && options.DelayBeforeSuddenDeath <= 15 &&
                    options.SuddenDeathTick >= 1 && options.SuddenDeathTick <= 30 &&
                    options.StartingLevel >= 0 && options.StartingLevel <= 100;
                if (accepted)
                    _options = options; // Options will be sent to players when starting a new game
                else
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Invalid options");
            }
            else
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Cannot change options");
        }

        private void KickPlayerHandler(IPlayer player, int playerId)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "KickPlayer:{0} [{1}]", player.Name, playerId);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            IPlayer kickedPlayer = _playerManager[playerId];
            if (masterPlayer == player && State == States.WaitingStartGame && kickedPlayer != null)
            {
                // Send server stopped
                kickedPlayer.OnServerStopped();
                // Remove player from player manager and hosts + warn other players
                PlayerLeftHandler(kickedPlayer, LeaveReasons.Kick);
            }
            else
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Cannot kick player");
        }

        private void BanPlayerHandler(IPlayer player, int playerId)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "BanPlayer:{0} [{1}]", player.Name, playerId);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            IPlayer bannedPlayer = _playerManager[playerId];
            if (masterPlayer == player && State == States.WaitingStartGame && bannedPlayer != null)
            {
                // Send server stopped
                bannedPlayer.OnServerStopped();
                // Remove player from player manager and hosts + warn other players
                PlayerLeftHandler(bannedPlayer, LeaveReasons.Ban);
                // TODO: add to ban list
            }
            else
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Cannot ban player");
        }

        private void ResetWinListHandler(IPlayer player)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "ResetWinLost:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                ResetWinList();
            }
            else
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Cannot reset win list");
        }

        private void PlayerLeftHandler(IPlayer player, LeaveReasons reason)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Player left:{0} {1}", player.Name, reason);

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
            foreach (IHost host in _hosts)
                host.RemovePlayer(player);

            // If game was running and player was playing, check if only one player left (see GameLostHandler)
            if ((State == States.GameStarted || State == States.GamePaused) && player.State == PlayerStates.Playing)
            {
                int playingCount = _playerManager.Players.Count(p => p.State == PlayerStates.Playing);
                if (playingCount == 0 || playingCount == 1)
                {
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Game finished by forfeit no winner");
                    State = States.GameFinished;
                    // Send game finished (no winner)
                    foreach (IPlayer p in _playerManager.Players.Where(p => p != player))
                        p.OnGameFinished();
                    State = States.WaitingStartGame;
                }
            }

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
            private readonly object _lock = new object();
            private readonly Func<Tetriminos> _randomFunc;
            private int _size;
            private Tetriminos[] _array;

            public TetriminoQueue(Func<Tetriminos> randomFunc, int seed = 0)
            {
                _randomFunc = randomFunc;
                Grow(64);
            }

            public void Reset()
            {
                lock (_lock)
                {
                    Fill(0, _size);
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
                        tetrimino = _array[index];
                    }
                    return tetrimino;
                }
            }

            private void Grow(int increment)
            {
                int newSize = _size + increment;
                Tetriminos[] newArray = new Tetriminos[newSize];
                if (_size > 0)
                    Array.Copy(_array, newArray, _size);
                _array = newArray;
                Fill(_size, increment);
                _size = newSize;
            }

            private void Fill(int from, int count)
            {
                for (int i = from; i < from + count; i++)
                    _array[i] = _randomFunc();
            }
        }

        #endregion

        #region Game action queue
        private readonly ManualResetEvent _actionEnqueuedEvent = new ManualResetEvent(false);
        private readonly ConcurrentQueue<Action> _gameActionQueue = new ConcurrentQueue<Action>();

        private void EnqueueAction(Action action)
        {
            _gameActionQueue.Enqueue(action);
            _actionEnqueuedEvent.Set();
        }

        private void GameActionsTask()
        {
            WaitHandle[] waitHandles =
            {
                _stopBackgroundTaskEvent,
                _actionEnqueuedEvent
            };

            while (true)
            {
                int handle = WaitHandle.WaitAny(waitHandles, 100);
                if (handle == 0) // _stopBackgroundTaskEvent
                    break; // Stop here
                // Even if WaitAny returned WaitHandle.WaitTimeout, we check action queue
                _actionEnqueuedEvent.Reset();
                // Perform game actions
                if (State == States.GameStarted && !_gameActionQueue.IsEmpty)
                {
                    while (!_gameActionQueue.IsEmpty)
                    {
                        Action action;
                        bool dequeue = _gameActionQueue.TryDequeue(out action);
                        if (dequeue)
                        {
                            try
                            {
                                action();
                                Thread.Sleep(1);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log.WriteLine(Logger.Log.LogLevels.Error, "Exception raised in TaskResolveGameActions. Exception:{0}", ex);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Game actions

        private void PlaceTetrimino(IPlayer player, int index, Tetriminos tetrimino, int orientation, int posX, int posY, byte[] grid)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "PlaceTetrimino[{0}]{1}:{2} {3} at {4},{5} {6}", player.Name, index, tetrimino, orientation, posX, posY, grid == null ? -1 : grid.Count(x => x > 0));

            // TODO: check if index is equal to player.TetriminoIndex
            if (index != player.TetriminoIndex)
                Logger.Log.WriteLine(Logger.Log.LogLevels.Error, "!!!! tetrimino index different for player {0} local {1} remote {2}", player.Name, player.TetriminoIndex, index);

            // Set grid
            player.Grid = grid;
            // Get next tetrimino
            player.TetriminoIndex++;
            int indexToSend = player.TetriminoIndex + 2; // indices 0, 1 and 2 have been sent when starting game
            Tetriminos nextTetriminoToSend = _tetriminoQueue[indexToSend];

            // Send grid to other playing players
            int playerId = _playerManager.GetId(player);
            //foreach (IPlayer p in _playerManager.Players.Where(p => p != player && p.State == PlayerStates.Playing))
            foreach (IPlayer p in _playerManager.Players.Where(p => p != player))
                p.OnGridModified(playerId, grid);

            //Logger.Log.WriteLine("Send next tetrimino {0} {1} to {2}", nextTetriminoToSend, indexToSend, player.Name);
            // Send next tetrimino
            player.OnNextTetrimino(indexToSend, nextTetriminoToSend);
        }

        private void Special(IPlayer player, IPlayer target, Specials special)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "UseSpecial[{0}][{1}]{2}", player.Name, target.Name, special);

            //
            int playerId = _playerManager.GetId(player);
            int targetId = _playerManager.GetId(target);
            // Store special id locally
            int specialId = SpecialId;
            // Increment special
            SpecialId++;
            // If special is Switch, call OnGridModified with switched grids
            if (special == Specials.SwitchFields)
            {
                // Send switched grid to player and target
                target.OnGridModified(targetId, player.Grid);
                player.OnGridModified(playerId, target.Grid);
                // Switch locally
                byte[] tmp = target.Grid;
                target.Grid = player.Grid;
                player.Grid = tmp;
                // Send new grid to player and target
                target.OnGridModified(playerId, player.Grid);
                player.OnGridModified(targetId, target.Grid);
            }
            // Inform about special use
            foreach (IPlayer p in _playerManager.Players)
                p.OnSpecialUsed(specialId, playerId, targetId, special);
        }

        private void SendLines(IPlayer player, int count)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "SendLines[{0}]:{1}", player.Name, count);

            //
            int playerId = _playerManager.GetId(player);
            // Store special id locally
            int specialId = SpecialId;
            // Increment special id
            SpecialId++;
            // Send lines to everyone including sender (so attack msg can be displayed)
            foreach (IPlayer p in _playerManager.Players.Where(x => x.State == PlayerStates.Playing))
                p.OnPlayerAddLines(specialId, playerId, count);
        }

        private void ModifyGrid(IPlayer player, byte[] grid)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "ModifyGrid[{0}]", player.Name);

            // Set grid
            player.Grid = grid;
            // Get id
            int id = _playerManager.GetId(player);
            // Send grid modification to everyone except sender
            foreach (IPlayer p in _playerManager.Players.Where(p => p != player))
                p.OnGridModified(id, player.Grid);
        }

        private void GameLost(IPlayer player)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "GameLost[{0}]  {1}", player.Name, State);

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
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Game finished with only one player playing, no winner");
                    State = States.GameFinished;
                    // Send game finished (no winner)
                    foreach (IPlayer p in _playerManager.Players)
                        p.OnGameFinished();
                    State = States.WaitingStartGame;
                }
                else if (playingCount == 1) // only one playing left
                {
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Game finished checking winner");
                    State = States.GameFinished;
                    // Game won
                    IPlayer winner = _playerManager.Players.Single(p => p.State == PlayerStates.Playing);
                    int winnerId = _playerManager.GetId(winner);
                    winner.State = PlayerStates.Registered;
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Winner: {0}[{1}]", winner.Name, winnerId);

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
                        p.OnWinListModified(WinList);
                    }
                    State = States.WaitingStartGame;
                }
            }
            else
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Game lost from non-playing player {0} {1}", player.Name, player.State);
        }

        #endregion

        private void TimeoutTask()
        {
            while (true)
            {
                // Check sudden death
                if (State == States.GameStarted && _isSuddenDeathActive)
                {
                    if (DateTime.Now > _suddenDeathStartTime)
                    {
                        TimeSpan timespan = DateTime.Now - _lastSuddenDeathAddLines;
                        if (timespan.TotalSeconds >= _options.SuddenDeathTick)
                        {
                            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Sudden death tick");
                            // Delay elapsed, send lines
                            foreach (IPlayer p in _playerManager.Players.Where(p => p.State == PlayerStates.Playing))
                                p.OnServerAddLines(1);
                            _lastSuddenDeathAddLines = DateTime.Now;
                        }
                    }
                }

                // Check running game without any player
                if (State == States.GameStarted || State == States.GamePaused)
                {
                    if (_playerManager.Players.Count(x => x.State == PlayerStates.Playing) == 0)
                    {
                        Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Game finished because no more playing players");
                        // Stop game
                        StopGame();
                    }
                }

                // Check player timeout + send heartbeat if needed
                foreach (IPlayer p in _playerManager.Players)
                {
                    // Check player timeout
                    TimeSpan timespan = DateTime.Now - p.LastActionFromClient;
                    if (timespan.TotalMilliseconds > TimeoutDelay && IsTimeoutDetectionActive)
                    {
                        Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "Timeout++ for player {0}", p.Name);
                        // Update timeout count
                        p.SetTimeout();
                        if (p.TimeoutCount >= MaxTimeoutCountBeforeDisconnection)
                            PlayerLeftHandler(p, LeaveReasons.Timeout);
                    }

                    // Send heartbeat if needed
                    TimeSpan delayFromPreviousHeartbeat = DateTime.Now - p.LastActionToClient;
                    if (delayFromPreviousHeartbeat.TotalMilliseconds > HeartbeatDelay)
                        p.OnHeartbeatReceived();
                }

                // Stop task if stop event is raised
                if (_stopBackgroundTaskEvent.WaitOne(10))
                    break;
            }
        }
    }
}
