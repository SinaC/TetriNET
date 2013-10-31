using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Logger;
using TetriNET.Common.Randomizer;
using TetriNET.Server.Interfaces;

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

        private const int PiecesSendOnGameStarted = 5;
        private const int PiecesSendOnPlacePiece = 4;
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const bool IsTimeoutDetectionActive = false;

        private readonly PieceQueue _pieceQueue;
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
            if (hosts == null || !hosts.Any())
                throw new ArgumentNullException("hosts");
            _options = new GameOptions();
            _options.ResetToDefault();

            _pieceQueue = new PieceQueue(() => RangeRandom.Random(_options.PieceOccurancies));
            _playerManager = playerManager;
            _hosts = hosts.ToList();
            
            WinList = new List<WinEntry>();

            foreach (IHost host in _hosts)
            {
                host.OnPlayerRegistered += RegisterPlayerHandler;
                host.OnPlayerUnregistered += UnregisterPlayerHandler;
                host.OnPlayerTeamChanged += PlayerTeamHandler;
                host.OnMessagePublished += PublishMessageHandler;
                host.OnPiecePlaced += PlacePieceHandler;
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
                host.OnFinishContinuousSpecial += FinishContinuousSpecialHandler;
                host.OnEarnAchievement += EarnAchievementHandler;

                host.OnPlayerLeft += PlayerLeftHandler;

                Debug.Assert(Check.CheckEvents(host), "Every host events must be handled");
            }

            State = States.WaitingStartServer;
        }

        public void StartServer()
        {
            Log.WriteLine(Log.LogLevels.Info, "Starting server");

            State = States.StartingServer;

            SpecialId = 0;

            Task.Factory.StartNew(TimeoutTask);
            Task.Factory.StartNew(GameActionsTask);

            // Start hosts
            foreach (IHost host in _hosts)
                host.Start();

            State = States.WaitingStartGame;

            Log.WriteLine(Log.LogLevels.Info, "Server started");
        }

        public void StopServer()
        {
            Log.WriteLine(Log.LogLevels.Info, "Stopping server");
            State = States.StoppingServer;

            // Stop worker threads
            _stopBackgroundTaskEvent.Set();
            _cancellationTokenSource.Cancel();

            // Inform players
            foreach (IPlayer p in _playerManager.Players)
                p.OnServerStopped();

            // Stop hosts
            foreach (IHost host in _hosts)
                host.Stop();

            // Clear player manager
            _playerManager.Clear();

            State = States.WaitingStartServer;

            Log.WriteLine(Log.LogLevels.Info, "Server stopped");
        }

        public void StartGame()
        {
            Log.WriteLine(Log.LogLevels.Info, "Starting game");

            if (State == States.WaitingStartGame)
            {
                // Reset action list
                //Action outAction;
                //while (!_gameActionQueue.IsEmpty)
                //    _gameActionQueue.TryDequeue(out outAction);
                while (_gameActionBlockingCollection.Count > 0)
                {
                    Action item;
                    _gameActionBlockingCollection.TryTake(out item);
                }

                // Reset special id
                SpecialId = 0;

                // Reset Piece Queue
                _pieceQueue.Reset(); // TODO: random seed
                //Pieces firstPiece = _pieceQueue[0];
                //Pieces secondPiece = _pieceQueue[1];
                //Pieces thirdPiece = _pieceQueue[2];
                //Log.WriteLine(Log.LogLevels.Info, "Starting game with {0} {1} {2}", firstPiece, secondPiece, thirdPiece);

                List<Pieces> pieces = new List<Pieces>();
                for (int i = 0; i < PiecesSendOnGameStarted; i++)
                    pieces.Add(_pieceQueue[i]);
                Log.WriteLine(Log.LogLevels.Info, "Starting game with {0}", pieces.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i));

                // Reset sudden death
                _isSuddenDeathActive = false;
                if (_options.DelayBeforeSuddenDeath > 0)
                {
                    _suddenDeathStartTime = DateTime.Now.AddMinutes(_options.DelayBeforeSuddenDeath);
                    _isSuddenDeathActive = true;
                    Log.WriteLine(Log.LogLevels.Info, "Sudden death will be activated after {0} minutes and send lines every {1} seconds", _options.DelayBeforeSuddenDeath, _options.SuddenDeathTick);
                }

                // Send start game to every connected player
                foreach (IPlayer p in _playerManager.Players)
                {
                    p.PieceIndex = 0;
                    p.State = PlayerStates.Playing;
                    p.LossTime = DateTime.MaxValue;
                    //p.OnGameStarted(firstPiece, secondPiece, thirdPiece, _options);
                    p.OnGameStarted(pieces, _options);
                }
                State = States.GameStarted;

                Log.WriteLine(Log.LogLevels.Info, "Game started");
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Cannot start game");
        }

        public void StopGame()
        {
            Log.WriteLine(Log.LogLevels.Info, "Stopping game");

            if (State == States.GameStarted || State == States.GamePaused)
            {
                State = States.GameFinished;

                // Send start game to every connected player
                foreach (IPlayer p in _playerManager.Players)
                    p.OnGameFinished();

                State = States.WaitingStartGame;

                Log.WriteLine(Log.LogLevels.Info, "Game stopped");
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Cannot stop game");
        }

        public void PauseGame()
        {
            Log.WriteLine(Log.LogLevels.Info, "Pausing game");

            if (State == States.GameStarted)
            {
                State = States.GamePaused;

                // Send pause to players
                foreach (IPlayer p in _playerManager.Players)
                    p.OnGamePaused();

                Log.WriteLine(Log.LogLevels.Info, "Game paused");
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Cannot pause game");
        }

        public void ResumeGame()
        {
            Log.WriteLine(Log.LogLevels.Info, "Resuming game");
            if (State == States.GamePaused)
            {
                State = States.GameStarted;

                // Reset sudden death
                _lastSuddenDeathAddLines = DateTime.Now; // TODO: a player, could pause<->resume forever to avoid sudden death

                // Send resume to players
                foreach (IPlayer p in _playerManager.Players)
                    p.OnGameResumed();

                Log.WriteLine(Log.LogLevels.Info, "Game resumed");
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Cannot resume game");
        }

        public void ResetWinList()
        {
            Log.WriteLine(Log.LogLevels.Info, "Resetting win list");

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
                Log.WriteLine(Log.LogLevels.Info, "Win list resetted");
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Cannot reset win list");
        }

        public void ToggleSuddenDeath()
        {
            if (_options.DelayBeforeSuddenDeath == 0)
            {
                Log.WriteLine(Log.LogLevels.Info, "Activating SUDDEN DEATH");
                _isSuddenDeathActive = true;
                _options.DelayBeforeSuddenDeath = 1;
                _options.SuddenDeathTick = 1;
                _suddenDeathStartTime = DateTime.Now.AddMinutes(-_options.DelayBeforeSuddenDeath);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Info, "Disabling SUDDEN DEATH");
                _isSuddenDeathActive = false;
                _options.DelayBeforeSuddenDeath = 0;
                _options.SuddenDeathTick = 0;
            }
        }

        public GameOptions GetOptions()
        {
            return _options;
        }

        private void UpdateWinList(string playerName, string team, int score)
        {
            WinEntry entry = WinList.SingleOrDefault(x => x.PlayerName == playerName && x.Team == team);
            if (entry == null)
            {
                entry = new WinEntry
                {
                    PlayerName = playerName,
                    Team = team,
                    Score = 0
                };
                WinList.Add(entry);
            }
            entry.Score += score;
        }

        #region IHost event handler

        private void RegisterPlayerHandler(IPlayer player, int playerId)
        {
            Log.WriteLine(Log.LogLevels.Info, "New player:[{0}]{1}", playerId, player.Name);

            // Send player id back to player
            player.OnPlayerRegistered(RegistrationResults.RegistrationSuccessful, playerId, State == States.GameStarted || State == States.GamePaused, _playerManager.ServerMaster == player, _options);

            // Inform new player about other players
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
            {
                int id = _playerManager.GetId(p);
                player.OnPlayerJoined(id, p.Name);
                if (!String.IsNullOrWhiteSpace(p.Team))
                    player.OnPlayerTeamChanged(id, p.Team);
            }

            // Inform players about new played connected
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
            {
                p.OnPlayerJoined(playerId, player.Name);
                //p.OnPlayerTeamChanged(playerId, player.Team); player has not team yet
            }

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

            // Send win list
            player.OnWinListModified(WinList);
        }

        private void UnregisterPlayerHandler(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "Unregister player:{0}", player.Name);

            PlayerLeftHandler(player, LeaveReasons.Disconnected);
        }

        private void PlayerTeamHandler(IPlayer player, string team)
        {
            Log.WriteLine(Log.LogLevels.Info, "Player team changed:{0}:{1}", player.Name, team);

            if (String.IsNullOrWhiteSpace(team))
                team = null;
            player.Team = team;
            // Send message to players
            int id = _playerManager.GetId(player);
            foreach(IPlayer p in _playerManager.Players)
                p.OnPlayerTeamChanged(id, team);
        }

        private void PublishMessageHandler(IPlayer player, string msg)
        {
            Log.WriteLine(Log.LogLevels.Info, "PublishMessage:{0}:{1}", player.Name, msg);

            // Send message to players
            //foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
            foreach (IPlayer p in _playerManager.Players)
                p.OnPublishPlayerMessage(player.Name, msg);
        }

        private void PlacePieceHandler(IPlayer player, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            EnqueueAction(() => PlacePiece(player, pieceIndex, highestIndex, piece, orientation, posX, posY, grid));
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
            Log.WriteLine(Log.LogLevels.Info, "StartGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                StartGame();
            }
        }

        private void StopGameHandler(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "StopGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                StopGame();
            }
        }

        private void PauseGameHandler(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "PauseGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                PauseGame();
            }
        }

        private void ResumeGameHandler(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "ResumeGame:{0}", player.Name);

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
            Log.WriteLine(Log.LogLevels.Info, "ChangeOptionsHandler:{0} {1}", player.Name, options);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player && State == States.WaitingStartGame)
            {
                // Remove duplicates (just in case)
                options.SpecialOccurancies = options.SpecialOccurancies.GroupBy(x => x.Value).Select(x => x.First()).ToList();
                options.PieceOccurancies = options.PieceOccurancies.GroupBy(x => x.Value).Select(x => x.First()).ToList();

                // Check options before accepting them
                bool accepted =
                    RangeRandom.SumOccurancies(options.PieceOccurancies) == 100 &&
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
                    Log.WriteLine(Log.LogLevels.Info, "Invalid options");
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Cannot change options");
        }

        private void KickPlayerHandler(IPlayer player, int playerId)
        {
            Log.WriteLine(Log.LogLevels.Info, "KickPlayer:{0} [{1}]", player.Name, playerId);

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
                Log.WriteLine(Log.LogLevels.Info, "Cannot kick player");
        }

        private void BanPlayerHandler(IPlayer player, int playerId)
        {
            Log.WriteLine(Log.LogLevels.Info, "BanPlayer:{0} [{1}]", player.Name, playerId);

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
                Log.WriteLine(Log.LogLevels.Info, "Cannot ban player");
        }

        private void ResetWinListHandler(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "ResetWinLost:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                ResetWinList();
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Cannot reset win list");
        }

        private void FinishContinuousSpecialHandler(IPlayer player, Specials special)
        {
            EnqueueAction(() => FinishContinuousSpecial(player, special));
        }

        private void EarnAchievementHandler(IPlayer player, int achievementId, string achievementTitle)
        {
            Log.WriteLine(Log.LogLevels.Info, "EarnAchievementHandler:{0} {1} {2}", player.Name, achievementId, achievementTitle);

            int id = _playerManager.GetId(player);
            foreach(IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnAchievementEarned(id, achievementId, achievementTitle);
        }

        private void PlayerLeftHandler(IPlayer player, LeaveReasons reason)
        {
            Log.WriteLine(Log.LogLevels.Info, "Player left:{0} {1}", player.Name, reason);

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
                    Log.WriteLine(Log.LogLevels.Info, "Game finished by forfeit no winner");
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

        #region Game action queue
        //private readonly ManualResetEvent _actionEnqueuedEvent = new ManualResetEvent(false);
        //private readonly ConcurrentQueue<Action> _gameActionQueue = new ConcurrentQueue<Action>();
        
        private readonly BlockingCollection<Action> _gameActionBlockingCollection = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
        private CancellationTokenSource _cancellationTokenSource;

        private void EnqueueAction(Action action)
        {
            //_gameActionQueue.Enqueue(action);
            //_actionEnqueuedEvent.Set();

            _gameActionBlockingCollection.Add(action);
        }

        private void GameActionsTask()
        {
            Log.WriteLine(Log.LogLevels.Info, "GameActionsTask started");

            //WaitHandle[] waitHandles =
            //{
            //    _stopBackgroundTaskEvent,
            //    _actionEnqueuedEvent
            //};

            //while (true)
            //{
            //    int handle = WaitHandle.WaitAny(waitHandles, 20);
            //    if (handle == 0) // _stopBackgroundTaskEvent
            //        break; // Stop here
            //    // Even if WaitAny returned WaitHandle.WaitTimeout, we check action queue
            //    _actionEnqueuedEvent.Reset();
            //    // Perform game actions
            //    if (State == States.GameStarted && !_gameActionQueue.IsEmpty)
            //    {
            //        while (!_gameActionQueue.IsEmpty)
            //        {
            //            Action action;
            //            bool dequeue = _gameActionQueue.TryDequeue(out action);
            //            if (dequeue)
            //            {
            //                try
            //                {
            //                    Log.WriteLine(Log.LogLevels.Debug, "Dequeue, item in queue {0}", _gameActionQueue.Count);
            //                    action();
            //                    Thread.Sleep(1);
            //                }
            //                catch (Exception ex)
            //                {
            //                    Log.WriteLine(Log.LogLevels.Error, "Exception raised in TaskResolveGameActions. Exception:{0}", ex);
            //                }
            //            }
            //        }
            //    }
            //}

            _cancellationTokenSource = new CancellationTokenSource();

            while (true)
            {
                try
                {
                    Action action;
                    bool taken = _gameActionBlockingCollection.TryTake(out action, 10, _cancellationTokenSource.Token);
                    if (taken)
                    {
                        try
                        {
                            Log.WriteLine(Log.LogLevels.Debug, "Dequeue, item in queue {0}", _gameActionBlockingCollection.Count);
                            action();
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine(Log.LogLevels.Error, "Exception raised in GameActionsTask. Exception:{0}", ex);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.WriteLine(Log.LogLevels.Info, "Taking cancelled");
                    break;
                }
            }

            Log.WriteLine(Log.LogLevels.Info, "GameActionsTask stopped");
        }

        #endregion

        #region Game actions

        private void PlacePiece(IPlayer player, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            Log.WriteLine(Log.LogLevels.Info, "PlacePiece[{0}]{1}:{2} {3} {4} at {5},{6} {7}", player.Name, pieceIndex, highestIndex, piece, orientation, posX, posY, grid == null ? -1 : grid.Count(x => x > 0));

            //if (index != player.PieceIndex)
            //    Log.WriteLine(Log.LogLevels.Error, "!!!! piece index different for player {0} local {1} remote {2}", player.Name, player.PieceIndex, index);

            // Set grid
            player.Grid = grid;
            // Get next piece
            //player.PieceIndex++;
            player.PieceIndex = pieceIndex;
            //int indexToSend = player.PieceIndex + 2; // indices 0, 1 and 2 have been sent when starting game
            //Pieces nextPieceToSend = _pieceQueue[indexToSend];
            List<Pieces> nextPiecesToSend = new List<Pieces>();
            for (int i = 0; i < PiecesSendOnPlacePiece; i++)
                nextPiecesToSend.Add(_pieceQueue[highestIndex + i]);

            // Send grid to other playing players
            int playerId = _playerManager.GetId(player);
            //foreach (IPlayer p in _playerManager.Players.Where(p => p != player && p.State == PlayerStates.Playing))
            foreach (IPlayer p in _playerManager.Players.Where(p => p != player))
                p.OnGridModified(playerId, grid);

            //Logger.Log.WriteLine("Send next piece {0} {1} to {2}", nextPieceToSend, indexToSend, player.Name);
            //// Send next piece
            //player.OnNextPiece(indexToSend, nextPieceToSend);
            Log.WriteLine(Log.LogLevels.Debug, "Send next piece {0} {1} {2}", highestIndex, nextPiecesToSend.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i));
            // Send next pieces
            player.OnNextPiece(highestIndex, nextPiecesToSend);

        }

        private void Special(IPlayer player, IPlayer target, Specials special)
        {
            Log.WriteLine(Log.LogLevels.Info, "UseSpecial[{0}][{1}]{2}", player.Name, target.Name, special);

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
                //// Send switched grid to player and target
                //target.OnGridModified(targetId, player.Grid);
                //player.OnGridModified(playerId, target.Grid);
                // Switch locally
                byte[] tmp = target.Grid;
                target.Grid = player.Grid;
                player.Grid = tmp;
                //// Send new grid to player and target
                //target.OnGridModified(playerId, player.Grid);
                //player.OnGridModified(targetId, target.Grid);

                //// Send switched grid to everyone
                //foreach (IPlayer p in _playerManager.Players)
                //{
                //    //if (p != player)
                //        p.OnGridModified(playerId, player.Grid);
                //    //if (p != target)
                //        p.OnGridModified(targetId, target.Grid);
                //}
                
                // Send switched grid to player and target
                player.OnGridModified(playerId, player.Grid);
                if (player != target)
                    target.OnGridModified(targetId, target.Grid);
                // They will send their grid when receiving them (with an optional capping)
            }
            // Inform about special use
            foreach (IPlayer p in _playerManager.Players)
                p.OnSpecialUsed(specialId, playerId, targetId, special);
        }

        private void SendLines(IPlayer player, int count)
        {
            Log.WriteLine(Log.LogLevels.Info, "SendLines[{0}]:{1}", player.Name, count);

            //
            int playerId = _playerManager.GetId(player);
            // Store special id locally
            int specialId = SpecialId;
            // Increment special id
            SpecialId++;
            // Send lines to everyone including sender (so attack msg can be displayed)
            //foreach (IPlayer p in _playerManager.Players.Where(x => x.State == PlayerStates.Playing))
            foreach (IPlayer p in _playerManager.Players)
                p.OnPlayerAddLines(specialId, playerId, count);
        }

        private void ModifyGrid(IPlayer player, byte[] grid)
        {
            Log.WriteLine(Log.LogLevels.Info, "ModifyGrid[{0}]", player.Name);

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
            Log.WriteLine(Log.LogLevels.Info, "GameLost[{0}]  {1}", player.Name, State);

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
                    Log.WriteLine(Log.LogLevels.Info, "Game finished with only one player playing, no winner");
                    State = States.GameFinished;
                    // Send game finished (no winner)
                    foreach (IPlayer p in _playerManager.Players)
                        p.OnGameFinished();
                    State = States.WaitingStartGame;
                }
                else if (playingCount == 1) // only one playing left
                {
                    Log.WriteLine(Log.LogLevels.Info, "Game finished checking winner");
                    State = States.GameFinished;
                    // Game won
                    IPlayer winner = _playerManager.Players.Single(p => p.State == PlayerStates.Playing);
                    int winnerId = _playerManager.GetId(winner);
                    winner.State = PlayerStates.Registered;
                    Log.WriteLine(Log.LogLevels.Info, "Winner: {0}[{1}]", winner.Name, winnerId);

                    // Update win list
                    UpdateWinList(winner.Name, winner.Team, 3);
                    int points = 2;
                    foreach (IPlayer p in _playerManager.Players.Where(x => x.State == PlayerStates.GameLost).OrderByDescending(x => x.LossTime))
                    {
                        UpdateWinList(p.Name, p.Team, points);
                        points--;
                        if (points == 0)
                            break;
                    }

                    // Send winner, game finished and win list
                    foreach (IPlayer p in _playerManager.Players)
                    {
                        p.OnPlayerWon(winnerId);
                        p.OnGameFinished();
                        p.OnWinListModified(WinList);
                    }
                    State = States.WaitingStartGame;
                }
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Game lost from non-playing player {0} {1}", player.Name, player.State);
        }

        private void FinishContinuousSpecial(IPlayer player, Specials special)
        {
            Log.WriteLine(Log.LogLevels.Info, "FinishContinuousSpecial[{0}]: {1}", player.Name, special);

            // Get id
            int id = _playerManager.GetId(player);
            // Send to everyone except sender
            foreach (IPlayer p in _playerManager.Players.Where(p => p != player))
                p.OnContinuousSpecialFinished(id, special);
        }

        #endregion

        private void TimeoutTask()
        {
            Log.WriteLine(Log.LogLevels.Info, "TimeoutTask started");

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
                            Log.WriteLine(Log.LogLevels.Info, "Sudden death tick");
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
                        Log.WriteLine(Log.LogLevels.Info, "Game finished because no more playing players");
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
                        Log.WriteLine(Log.LogLevels.Info, "Timeout++ for player {0}", p.Name);
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
                {
                    Log.WriteLine(Log.LogLevels.Info, "Stop background task event raised");
                    break;
                }
            }

            Log.WriteLine(Log.LogLevels.Info, "TimeoutTask stopped");
        }
    }
}
