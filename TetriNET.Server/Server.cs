using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private readonly IPieceProvider _pieceProvider;
        private readonly IPlayerManager _playerManager;
        private readonly ISpectatorManager _spectatorManager;
        private readonly List<IHost> _hosts;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _timeoutTask;
        private Task _gameActionsTask;

        private GameOptions _options;

        private DateTime _gameStartTime;
        private bool _isSuddenDeathActive;
        private DateTime _suddenDeathStartTime;
        private DateTime _lastSuddenDeathAddLines;
        
        public States State { get; private set; }
        public int SpecialId { get; private set; }
        public List<WinEntry> WinList { get; private set; }
        public Dictionary<string, GameStatisticsByPlayer> PlayerStatistics { get; private set; } // By player (cannot be stored in IPlayer because IPlayer is lost when a player is disconnected during a game)

        public Server(IPlayerManager playerManager, ISpectatorManager spectatorManager, IPieceProvider pieceProvider, params IHost[] hosts)
        {
            if (playerManager == null)
                throw new ArgumentNullException("playerManager");
            if (hosts == null || !hosts.Any())
                throw new ArgumentNullException("hosts");
            _options = new GameOptions();
            _options.ResetToDefault();

            //_pieceProvider = new PieceQueue(() => RangeRandom.Random(_options.PieceOccurancies));
            _pieceProvider = pieceProvider;
            _pieceProvider.Occurancies = () => _options.PieceOccurancies;
            _playerManager = playerManager;
            _spectatorManager = spectatorManager;
            _hosts = hosts.ToList();

            PlayerStatistics = new Dictionary<string, GameStatisticsByPlayer>();

            WinList = new List<WinEntry>();

            foreach (IHost host in _hosts)
            {
                host.HostPlayerRegistered += OnRegisterPlayer;
                host.HostPlayerUnregistered += OnUnregisterPlayer;
                host.HostPlayerTeamChanged += OnPlayerTeam;
                host.HostMessagePublished += OnPublishMessage;
                host.HostPiecePlaced += OnPlacePiece;
                host.HostUseSpecial += OnUseSpecial;
                //host.HostSendLines += OnSendLines;
                host.HostClearLines += OnClearLines;
                host.HostGridModified += OnModifyGrid;
                host.HostStartGame += OnStartGame;
                host.HostStopGame += OnStopGame;
                host.HostPauseGame += OnPauseGame;
                host.HostResumeGame += OnResumeGame;
                host.HostGameLost += OnGameLost;
                host.HostChangeOptions += OnChangeOptions;
                host.HostKickPlayer += OnKickPlayer;
                host.HostBanPlayer += OnBanPlayer;
                host.HostResetWinList += OnResetWinList;
                host.HostFinishContinuousSpecial += OnFinishContinuousSpecial;
                host.HostEarnAchievement += OnEarnAchievement;
                host.HostSpectatorRegistered += OnRegisterSpectator;
                host.HostSpectatorUnregistered += OnUnregisterSpectator;
                host.HostSpectatorMessagePublished += OnPublishSpectatorMessage;

                host.HostPlayerLeft += OnPayerLeft;
                host.HostSpectatorLeft += OnSpectatorLeft;

                Debug.Assert(Check.CheckEvents(host), "Every host events must be handled");
            }

            State = States.WaitingStartServer;
        }

        public void StartServer()
        {
            Log.WriteLine(Log.LogLevels.Info, "Starting server");

            State = States.StartingServer;

            SpecialId = 0;

            _cancellationTokenSource = new CancellationTokenSource();
            _timeoutTask = Task.Factory.StartNew(TimeoutTask, _cancellationTokenSource.Token);
            _gameActionsTask = Task.Factory.StartNew(GameActionsTask, _cancellationTokenSource.Token);

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
            _cancellationTokenSource.Cancel();

            Task.WaitAll(new[] {_timeoutTask, _gameActionsTask}, 2000);

            // Inform players and spectators
            //foreach (IPlayer p in _playerManager.Players)
            //    p.OnServerStopped();
            foreach (IEntity entity in Entities)
                entity.OnServerStopped();

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
                _pieceProvider.Reset(); // TODO: random seed
                //Pieces firstPiece = _pieceProvider[0];
                //Pieces secondPiece = _pieceProvider[1];
                //Pieces thirdPiece = _pieceProvider[2];
                //Log.WriteLine(Log.LogLevels.Info, "Starting game with {0} {1} {2}", firstPiece, secondPiece, thirdPiece);

                List<Pieces> pieces = new List<Pieces>();
                for (int i = 0; i < PiecesSendOnGameStarted; i++)
                    pieces.Add(_pieceProvider[i]);
                Log.WriteLine(Log.LogLevels.Info, "Starting game with {0}", pieces.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i));

                _gameStartTime = DateTime.Now;

                // Reset sudden death
                _isSuddenDeathActive = false;
                if (_options.DelayBeforeSuddenDeath > 0)
                {
                    _suddenDeathStartTime = DateTime.Now.AddMinutes(_options.DelayBeforeSuddenDeath);
                    _isSuddenDeathActive = true;
                    Log.WriteLine(Log.LogLevels.Info, "Sudden death will be activated after {0} minutes and send lines every {1} seconds", _options.DelayBeforeSuddenDeath, _options.SuddenDeathTick);
                }

                // Reset statistics
                ResetStatistics();

                // Send game started to players
                foreach (IPlayer p in _playerManager.Players)
                {
                    p.PieceIndex = 0;
                    p.State = PlayerStates.Playing;
                    p.LossTime = DateTime.MaxValue;
                    //p.OnGameStarted(firstPiece, secondPiece, thirdPiece, _options);
                    p.OnGameStarted(pieces);
                }
                // Send game started to spectators
                foreach (ISpectator s in _spectatorManager.Spectators)
                    s.OnGameStarted(pieces);

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

                GameStatistics statistics = PrepareGameStatistics();
                // Send game finished to players and spectators
                foreach (IEntity entity in Entities)
                    entity.OnGameFinished(statistics);

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

                // Send game paused to players and spectators
                //foreach (IPlayer p in _playerManager.Players)
                foreach (IEntity entity in Entities)
                    entity.OnGamePaused();

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

                // Send game resumed to players and spectators
                //foreach (IPlayer p in _playerManager.Players)
                foreach (IEntity entity in Entities)
                    entity.OnGameResumed();

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
                //foreach (IPlayer p in _playerManager.Players)
                foreach (IEntity entity in Entities)
                {
                    entity.OnPublishServerMessage(String.Format("Win list resetted by {0}", serverMaster == null ? "SERVER" : serverMaster.Name));
                    entity.OnWinListModified(WinList);
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

        private void ResetStatistics()
        {
            // Delete previous stats
            PlayerStatistics.Clear();
            // Create GameStatisticsByPlayer for each player
            foreach (IPlayer player in _playerManager.Players)
            {
                // Init stats
                GameStatisticsByPlayer stats = new GameStatisticsByPlayer
                    {
                        PlayerName = player.Name,
                        SpecialsUsed = new Dictionary<Specials, Dictionary<string, int>>()
                    };
                foreach (SpecialOccurancy occurancy in _options.SpecialOccurancies.Where(x => x.Occurancy > 0))
                {
                    Dictionary<string, int> specialsByPlayer = _playerManager.Players.Select(p => p.Name).ToDictionary(x => x, x => 0);
                    stats.SpecialsUsed.Add(occurancy.Value, specialsByPlayer);
                }
                // Add stats
                PlayerStatistics.Add(player.Name, stats);
            }
        }

        private void UpdateStatistics(string playerName, int linesCount)
        {
            if (linesCount == 1)
                PlayerStatistics[playerName].SingleCount++;
            else if (linesCount == 2)
                PlayerStatistics[playerName].DoubleCount++;
            else if (linesCount == 3)
                PlayerStatistics[playerName].TripleCount++;
            else if (linesCount >= 4)
                PlayerStatistics[playerName].TetrisCount++;
        }

        private void UpdateStatistics(string playerName, string targetName, Specials special)
        {
            PlayerStatistics[playerName].SpecialsUsed[special][targetName]++;
        }

        private void UpdateStatistics(string playerName, DateTime gameStartTime, DateTime lossTime)
        {
            PlayerStatistics[playerName].PlayingTime = (lossTime - gameStartTime).TotalSeconds;
        }

        private GameStatistics PrepareGameStatistics()
        {
            GameStatistics statistics = new GameStatistics
                {
                    MatchTime = (DateTime.Now - _gameStartTime).TotalSeconds,
                    Players = PlayerStatistics.Select(x => x.Value).ToList()
                };
            return statistics;
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

        private IEnumerable<IEntity> Entities
        {
            get { return _playerManager.Players.Cast<IEntity>().Union(_spectatorManager.Spectators); }
        }

        #region IHost event handler

        private void OnRegisterPlayer(IPlayer player, int playerId)
        {
            Log.WriteLine(Log.LogLevels.Info, "New player:[{0}]{1}|{2}", playerId, player.Name, player.Team);

            // Send player id back to player
            player.OnPlayerRegistered(RegistrationResults.RegistrationSuccessful, playerId, State == States.GameStarted || State == States.GamePaused, _playerManager.ServerMaster == player, _options);

            // Inform new player about other players
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
            {
                int id = _playerManager.GetId(p);
                player.OnPlayerJoined(id, p.Name, p.Team);
            }
            // Inform new player about spectators
            foreach (ISpectator s in _spectatorManager.Spectators)
            {
                int id = _spectatorManager.GetId(s);
                player.OnSpectatorJoined(id, s.Name);
            }

            // Inform other players and spectators about new player connected
            foreach (IEntity entity in Entities.Where(x => x != player))
                entity.OnPlayerJoined(playerId, player.Name, player.Team);

            // Server master
            IPlayer serverMaster = _playerManager.ServerMaster;
            if (serverMaster != null)
            {
                int serverMasterId = _playerManager.GetId(serverMaster);
                // Send new server master id
                if (serverMaster == player)
                    foreach (IEntity entity in Entities.Where(x => x != player))
                        entity.OnServerMasterChanged(serverMasterId);
                // Send server master id to player even if not modified
                player.OnServerMasterChanged(serverMasterId);
            }

            // If game is running, send grid for playing player and game lost for dead player
            if (State == States.GamePaused || State == States.GameStarted)
            {
                foreach(IPlayer p in _playerManager.Players)
                {
                    int id = _playerManager.GetId(p);
                    if (p.State == PlayerStates.Playing)
                        player.OnGridModified(id, p.Grid);
                    else if (p.State == PlayerStates.GameLost)
                        player.OnPlayerLost(id);
                }
            }

            // Send win list
            player.OnWinListModified(WinList);
        }

        private void OnUnregisterPlayer(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "Unregister player:{0}", player.Name);

            OnPayerLeft(player, LeaveReasons.Disconnected);
        }

        private void OnPlayerTeam(IPlayer player, string team)
        {
            Log.WriteLine(Log.LogLevels.Info, "Player team changed:{0}:{1}", player.Name, team);

            player.Team = String.IsNullOrWhiteSpace(team) ? null : team;
            // Send message to players and spectators
            int id = _playerManager.GetId(player);
            foreach (IEntity entity in Entities)
                entity.OnPlayerTeamChanged(id, team);
        }

        private void OnPublishMessage(IPlayer player, string msg)
        {
            Log.WriteLine(Log.LogLevels.Info, "PublishMessage:{0}:{1}", player.Name, msg);

            // Send message to players and spectators
            foreach (IEntity entity in Entities)
                entity.OnPublishPlayerMessage(player.Name, msg);
        }

        private void OnPlacePiece(IPlayer player, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            EnqueueAction(() => PlacePiece(player, pieceIndex, highestIndex, piece, orientation, posX, posY, grid));
        }

        private void OnUseSpecial(IPlayer player, IPlayer target, Specials special)
        {
            EnqueueAction(() => Special(player, target, special));
        }

        //private void OnSendLines(IPlayer player, int count)
        //{
        //    EnqueueAction(() => SendLines(player, count));
        //}

        private void OnClearLines(IPlayer player, int count)
        {
            UpdateStatistics(player.Name, count);
            if (_options.ClassicStyleMultiplayerRules && count > 1)
            {
                int addLines = count - 1;
                if (addLines >= 4)
                    // special case for Tetris and above
                    addLines = 4;
                EnqueueAction(() => SendLines(player, addLines));
            }
        }

        private void OnModifyGrid(IPlayer player, byte[] grid)
        {
            EnqueueAction(() => ModifyGrid(player, grid));
        }

        private void OnStartGame(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "StartGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                StartGame();
            }
        }

        private void OnStopGame(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "StopGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                StopGame();
            }
        }

        private void OnPauseGame(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "PauseGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                PauseGame();
            }
        }

        private void OnResumeGame(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "ResumeGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                ResumeGame();
            }
        }

        private void OnGameLost(IPlayer player)
        {
            EnqueueAction(() => GameLost(player));
        }

        private void OnChangeOptions(IPlayer player, GameOptions options)
        {
            Log.WriteLine(Log.LogLevels.Info, "ChangeOptions:{0} {1}", player.Name, options);

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
                {
                    _options = options; // Options will be sent to players when starting a new game
                    // Inform other players/spectators about options modification
                    foreach (IEntity entity in Entities.Where(e => e != player))
                        entity.OnOptionsChanged(_options);
                }
                else
                    Log.WriteLine(Log.LogLevels.Info, "Invalid options");
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Cannot change options");
        }

        private void OnKickPlayer(IPlayer player, int playerId)
        {
            Log.WriteLine(Log.LogLevels.Info, "KickPlayer:{0} [{1}]", player.Name, playerId);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            IPlayer kickedPlayer = _playerManager[playerId];
            if (masterPlayer == player && State == States.WaitingStartGame && kickedPlayer != null)
            {
                // Send server stopped
                kickedPlayer.OnServerStopped();
                // Remove player from player manager and hosts + warn other players
                OnPayerLeft(kickedPlayer, LeaveReasons.Kick);
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Cannot kick player");
        }

        private void OnBanPlayer(IPlayer player, int playerId)
        {
            Log.WriteLine(Log.LogLevels.Info, "BanPlayer:{0} [{1}]", player.Name, playerId);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            IPlayer bannedPlayer = _playerManager[playerId];
            if (masterPlayer == player && State == States.WaitingStartGame && bannedPlayer != null)
            {
                // Send server stopped
                bannedPlayer.OnServerStopped();
                // Remove player from player manager and hosts + warn other players
                OnPayerLeft(bannedPlayer, LeaveReasons.Ban);
                // TODO: add to ban list
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Cannot ban player");
        }

        private void OnResetWinList(IPlayer player)
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

        private void OnFinishContinuousSpecial(IPlayer player, Specials special)
        {
            EnqueueAction(() => FinishContinuousSpecial(player, special));
        }

        private void OnEarnAchievement(IPlayer player, int achievementId, string achievementTitle)
        {
            Log.WriteLine(Log.LogLevels.Info, "EarnAchievement:{0} {1} {2}", player.Name, achievementId, achievementTitle);

            int id = _playerManager.GetId(player);
            foreach (IEntity entity in Entities.Where(x => x != player))
                entity.OnAchievementEarned(id, achievementId, achievementTitle);
        }

        private void OnRegisterSpectator(ISpectator spectator, int spectatorId)
        {
            Log.WriteLine(Log.LogLevels.Info, "New spectator:[{0}]{1}", spectatorId, spectator.Name);

            // Send spectator id back to spectator
            spectator.OnSpectatorRegistered(RegistrationResults.RegistrationSuccessful, spectatorId, State == States.GameStarted || State == States.GamePaused, _options);

            // Inform new spectator about players
            foreach (IPlayer p in _playerManager.Players)
            {
                int id = _playerManager.GetId(p);
                spectator.OnPlayerJoined(id, p.Name, p.Team);
            }
            // Inform new spectator about other spectators
            foreach(ISpectator s in _spectatorManager.Spectators.Where(x => x != spectator))
            {
                int id = _spectatorManager.GetId(s);
                spectator.OnSpectatorJoined(id, s.Name);
            }

            // Inform players and other spectators about new spectator connected
            foreach (IEntity entity in Entities.Where(x => x != spectator))
                entity.OnSpectatorJoined(spectatorId, spectator.Name);

            // If game is running, send grid for playing player and game lost for dead player
            if (State == States.GamePaused || State == States.GameStarted)
            {
                foreach (IPlayer p in _playerManager.Players)
                {
                    int id = _playerManager.GetId(p);
                    if (p.State == PlayerStates.Playing)
                        spectator.OnGridModified(id, p.Grid);
                    else if (p.State == PlayerStates.GameLost)
                        spectator.OnPlayerLost(id);
                }
            }

            // Send win list
            spectator.OnWinListModified(WinList);
        }

        private void OnUnregisterSpectator(ISpectator spectator)
        {
            Log.WriteLine(Log.LogLevels.Info, "Unregister spectator:{0}", spectator.Name);

            OnSpectatorLeft(spectator, LeaveReasons.Disconnected);
        }

        private void OnPublishSpectatorMessage(ISpectator spectator, string msg)
        {
            Log.WriteLine(Log.LogLevels.Info, "PublishSpectatorMessage:{0}:{1}", spectator.Name, msg);

            // Send message to players and spectators
            foreach (IEntity entity in Entities)
                entity.OnPublishPlayerMessage(spectator.Name, msg);
        }

        private void OnPayerLeft(IPlayer player, LeaveReasons reason)
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
                    GameStatistics statistics = PrepareGameStatistics();
                    // Send game finished (no winner)
                    foreach (IEntity entity in Entities.Where(x => x != player))
                        entity.OnGameFinished(statistics);
                    State = States.WaitingStartGame;
                }
            }

            // Inform players and spectators except disconnected player
            foreach (IEntity entity in Entities.Where(x => x != player))
                entity.OnPlayerLeft(id, player.Name, reason);

            // Send new server master id
            if (wasServerMaster)
            {
                IPlayer serverMaster = _playerManager.ServerMaster;
                if (serverMaster != null)
                {
                    int serverMasterId = _playerManager.GetId(serverMaster);
                    foreach (IEntity entity in Entities.Where(x => x != player))
                        entity.OnServerMasterChanged(serverMasterId);
                }
            }
        }

        private void OnSpectatorLeft(ISpectator spectator, LeaveReasons reason)
        {
            Log.WriteLine(Log.LogLevels.Info, "Spectator left:{0} {1}", spectator.Name, reason);

            // Remove from spectator
            int id;
            lock (_spectatorManager.LockObject)
            {
                // Get id
                id = _spectatorManager.GetId(spectator);
                // Remove spectator from spectator list
                _spectatorManager.Remove(spectator);
            }

            // Clean host tables
            foreach (IHost host in _hosts)
                host.RemoveSpectator(spectator);

            // Inform players and spectators except disconnected spectator
            foreach (IEntity entity in Entities.Where(x => x != spectator))
                entity.OnSpectatorLeft(id, spectator.Name, reason);
        }

        #endregion

        #region Game action queue

        private readonly BlockingCollection<Action> _gameActionBlockingCollection = new BlockingCollection<Action>(new ConcurrentQueue<Action>());

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

            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Log.WriteLine(Log.LogLevels.Info, "Stop background task event raised");
                    break;
                }
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

            bool sendNextPieces = false;
            // Set grid
            player.Grid = grid;
            // Get next piece
            //player.PieceIndex++;
            player.PieceIndex = pieceIndex;
            //int indexToSend = player.PieceIndex + 2; // indices 0, 1 and 2 have been sent when starting game
            //Pieces nextPieceToSend = _pieceProvider[indexToSend];
            List<Pieces> nextPiecesToSend = new List<Pieces>();
            Log.WriteLine(Log.LogLevels.Info, "{0} {1} indexes: {2} {3}", _playerManager.GetId(player), player.Name, highestIndex, pieceIndex);
            if (highestIndex < pieceIndex)
                Log.WriteLine(Log.LogLevels.Error, "PROBLEM WITH INDEXES!!!!!");
            if (highestIndex < pieceIndex + PiecesSendOnPlacePiece) // send many pieces when needed
            {
                for (int i = 0; i < 2*PiecesSendOnPlacePiece; i++)
                    nextPiecesToSend.Add(_pieceProvider[highestIndex + i]);
                sendNextPieces = true;
            }
            else if (highestIndex < pieceIndex + 2 * PiecesSendOnPlacePiece) // send next pieces only if needed
            {
                for (int i = 0; i < PiecesSendOnPlacePiece; i++)
                    nextPiecesToSend.Add(_pieceProvider[highestIndex + i]);
                sendNextPieces = true;
            }

            // Send grid to other playing players and spectators
            int playerId = _playerManager.GetId(player);
            foreach(IEntity callback in Entities.Where(x => x != player))
                callback.OnGridModified(playerId, grid);

            if (sendNextPieces)
            {
                Log.WriteLine(Log.LogLevels.Debug, "Send next piece {0} {1} {2}", highestIndex, pieceIndex, nextPiecesToSend.Any() ? nextPiecesToSend.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i) : String.Empty);
                // Send next pieces
                player.OnNextPiece(highestIndex, nextPiecesToSend);
            }

        }

        private void Special(IPlayer player, IPlayer target, Specials special)
        {
            Log.WriteLine(Log.LogLevels.Info, "UseSpecial[{0}][{1}]{2}", player.Name, target.Name, special);

            //
            int playerId = _playerManager.GetId(player);
            int targetId = _playerManager.GetId(target);
            // Update statistics
            UpdateStatistics(player.Name, target.Name, special);
            // Store special id locally
            int specialId = SpecialId;
            // Increment special
            SpecialId++;
            // If special is Switch, call OnGridModified with switched grids
            if (special == Specials.SwitchFields)
            {
                // Switch locally
                byte[] tmp = target.Grid;
                target.Grid = player.Grid;
                player.Grid = tmp;
                
                // Send switched grid to player and target
                player.OnGridModified(playerId, player.Grid);
                if (player != target)
                    target.OnGridModified(targetId, target.Grid);
                // They will send their grid when receiving them (with an optional capping)
            }
            // Inform about special use
            foreach (IEntity entity in Entities)
                entity.OnSpecialUsed(specialId, playerId, targetId, special);
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
            foreach (IEntity entity in Entities)
                entity.OnPlayerAddLines(specialId, playerId, count);
        }

        private void ModifyGrid(IPlayer player, byte[] grid)
        {
            Log.WriteLine(Log.LogLevels.Info, "ModifyGrid[{0}]", player.Name);

            // Set grid
            player.Grid = grid;
            // Get id
            int id = _playerManager.GetId(player);
            // Send grid modification to everyone except sender
            foreach (IEntity entity in Entities.Where(x => x != player))
                entity.OnGridModified(id, player.Grid);
        }

        private void GameLost(IPlayer player)
        {
            Log.WriteLine(Log.LogLevels.Info, "GameLost[{0}]  {1}", player.Name, State);

            if (player.State == PlayerStates.Playing)
            {
                // Set player state
                player.State = PlayerStates.GameLost;
                player.LossTime = DateTime.Now;

                // Inform other players and spectators
                int id = _playerManager.GetId(player);
                foreach (IEntity entity in Entities.Where(x => x != player))
                    entity.OnPlayerLost(id);

                UpdateStatistics(player.Name, _gameStartTime, player.LossTime);

                //
                int playingCount = _playerManager.Players.Count(p => p.State == PlayerStates.Playing);
                if (playingCount == 0) // there were only one playing player
                {
                    Log.WriteLine(Log.LogLevels.Info, "Game finished with only one player playing, no winner");
                    State = States.GameFinished;
                    // Send game finished (no winner) to players and spectators
                    GameStatistics statistics = PrepareGameStatistics();
                    foreach (IEntity entity in Entities)
                        entity.OnGameFinished(statistics);
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

                    GameStatistics statistics = PrepareGameStatistics();
                    // Send winner, game finished and win list
                    foreach (IEntity entity in Entities)
                    {
                        entity.OnPlayerWon(winnerId);
                        entity.OnGameFinished(statistics);
                        entity.OnWinListModified(WinList);
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
            foreach (IEntity entity in Entities.Where(x => x != player))
                entity.OnContinuousSpecialFinished(id, special);
        }

        #endregion

        private void TimeoutTask()
        {
            Log.WriteLine(Log.LogLevels.Info, "TimeoutTask started");

            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Log.WriteLine(Log.LogLevels.Info, "Stop background task event raised");
                    break;
                }

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
                            OnPayerLeft(p, LeaveReasons.Timeout);
                    }

                    // Send heartbeat if needed
                    TimeSpan delayFromPreviousHeartbeat = DateTime.Now - p.LastActionToClient;
                    if (delayFromPreviousHeartbeat.TotalMilliseconds > HeartbeatDelay)
                        p.OnHeartbeatReceived();
                }
                // Check spectator timeout + send heartbeat if needed
                foreach (ISpectator s in _spectatorManager.Spectators)
                {
                    // Check player timeout
                    TimeSpan timespan = DateTime.Now - s.LastActionFromClient;
                    if (timespan.TotalMilliseconds > TimeoutDelay && IsTimeoutDetectionActive)
                    {
                        Log.WriteLine(Log.LogLevels.Info, "Timeout++ for player {0}", s.Name);
                        // Update timeout count
                        s.SetTimeout();
                        if (s.TimeoutCount >= MaxTimeoutCountBeforeDisconnection)
                            OnSpectatorLeft(s, LeaveReasons.Timeout);
                    }

                    // Send heartbeat if needed
                    TimeSpan delayFromPreviousHeartbeat = DateTime.Now - s.LastActionToClient;
                    if (delayFromPreviousHeartbeat.TotalMilliseconds > HeartbeatDelay)
                        s.OnHeartbeatReceived();
                }


                // Stop task if stop event is raised
                bool signaled = _cancellationTokenSource.Token.WaitHandle.WaitOne(10);
                if (signaled)
                {
                    Log.WriteLine(Log.LogLevels.Info, "Stop background task event raised");
                    break;
                }
            }

            Log.WriteLine(Log.LogLevels.Info, "TimeoutTask stopped");
        }
    }
}
