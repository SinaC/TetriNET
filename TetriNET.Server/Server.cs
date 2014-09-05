using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.Common.Randomizer;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server
{
    public sealed class Server : IServer
    {
        private const int PiecesSendOnGameStarted = 5;
        private const int PiecesSendOnPlacePiece = 4;
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const bool IsTimeoutDetectionActive = false;

        private readonly IPieceProvider _pieceProvider;
        private readonly IPlayerManager _playerManager;
        private readonly ISpectatorManager _spectatorManager;
        private readonly IActionQueue _gameActionQueue;
        private readonly List<IHost> _hosts;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _timeoutTask;

        private DateTime _gameStartTime;
        private bool _isSuddenDeathActive;
        private DateTime _suddenDeathStartTime;
        private DateTime _lastSuddenDeathAddLines;

        public Server(IPlayerManager playerManager, ISpectatorManager spectatorManager, IPieceProvider pieceProvider, IActionQueue gameActionQueue)
        {
            if (playerManager == null)
                throw new ArgumentNullException("playerManager");

            Options = new GameOptions();
            Options.ResetToDefault();

            _pieceProvider = pieceProvider;
            _pieceProvider.Occurancies = () => Options.PieceOccurancies;
            _playerManager = playerManager;
            _spectatorManager = spectatorManager;
            _gameActionQueue = gameActionQueue;
            _hosts = new List<IHost>();

            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                Version version = entryAssembly.GetName().Version;
                Version = new Versioning
                {
                    Major = version.Major,
                    Minor = version.Minor,
                };
            }// else, we suppose SetVersion will be called later, before connecting

            GameStatistics = new Dictionary<string, GameStatisticsByPlayer>();

            WinList = new List<WinEntry>();

            State = ServerStates.WaitingStartServer;
        }

        #region IServer

        public ServerStates State { get; private set; }
        public int SpecialId { get; private set; }
        public List<WinEntry> WinList { get; private set; }
        public Dictionary<string, GameStatisticsByPlayer> GameStatistics { get; private set; } // By player (cannot be stored in IPlayer because IPlayer is lost when a player is disconnected during a game)
        public GameOptions Options { get; private set; }

        public Versioning Version { get; private set; }

        public void SetVersion(int major, int minor)
        {
            Version = new Versioning
            {
                Major = major,
                Minor = minor
            };
        }

        public bool AddHost(IHost host)
        {
            if (_hosts.Any(x => x == host))
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to add host more than once");
                return false;
            }

            if (State != ServerStates.WaitingStartServer)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to add host when server is already started");
                return false;
            }

            _hosts.Add(host);

            host.SetVersion(Version);

            host.HostPlayerRegistered += OnRegisterPlayer;
            host.HostPlayerUnregistered += OnUnregisterPlayer;
            host.HostPlayerTeamChanged += OnPlayerTeam;
            host.HostMessagePublished += OnPublishMessage;
            host.HostPiecePlaced += OnPlacePiece;
            host.HostUseSpecial += OnUseSpecial;
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

            Debug.Assert(CheckEvents(host), "Every host events must be handled");

            return true;
        }

        public void StartServer()
        {
            Log.Default.WriteLine(LogLevels.Info, "Starting server");

            if (State == ServerStates.WaitingStartServer)
            {
                if (_hosts.Count != 0)
                {

                    State = ServerStates.StartingServer;

                    SpecialId = 0;

                    _cancellationTokenSource = new CancellationTokenSource();
                    _timeoutTask = Task.Factory.StartNew(TimeoutTask, _cancellationTokenSource.Token);
                    _gameActionQueue.Start(_cancellationTokenSource);

                    // Start hosts
                    foreach (IHost host in _hosts)
                        host.Start();

                    State = ServerStates.WaitingStartGame;

                    Log.Default.WriteLine(LogLevels.Info, "Server started");
                }
                else
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot start server without any host");
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Server already started");
        }

        public void StopServer()
        {
            Log.Default.WriteLine(LogLevels.Info, "Stopping server");

            if (State != ServerStates.StoppingServer && State != ServerStates.WaitingStartServer)
            {
                State = ServerStates.StoppingServer;

                try
                {
                    // Stop worker threads
                    _cancellationTokenSource.Cancel();

                    _timeoutTask.Wait(2000);
                    _gameActionQueue.Wait(2000);
                }
                catch (AggregateException ex)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Aggregate exception while stopping. Exception: {0}", ex.Flatten());
                }

                // Inform players and spectators
                foreach (IEntity entity in Entities)
                    entity.OnServerStopped();

                // Stop hosts
                foreach (IHost host in _hosts)
                    host.Stop();
                // Clear player manager
                _playerManager.Clear();

                State = ServerStates.WaitingStartServer;

                Log.Default.WriteLine(LogLevels.Info, "Server stopped");
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Server already stopped or stopping");
        }

        public void StartGame()
        {
            Log.Default.WriteLine(LogLevels.Info, "Starting game");

            if (State == ServerStates.WaitingStartGame)
            {
                if (_playerManager.PlayerCount > 0)
                {
                    // Reset action list
                    _gameActionQueue.Reset();

                    // Reset special id
                    SpecialId = 0;

                    // Reset Piece Queue
                    _pieceProvider.Reset(); // TODO: random seed
                    //Pieces firstPiece = _pieceProvider[0];
                    //Pieces secondPiece = _pieceProvider[1];
                    //Pieces thirdPiece = _pieceProvider[2];
                    //Log.Default.WriteLine(LogLevels.Info, "Starting game with {0} {1} {2}", firstPiece, secondPiece, thirdPiece);

                    List<Pieces> pieces = new List<Pieces>();
                    for (int i = 0; i < PiecesSendOnGameStarted; i++)
                        pieces.Add(_pieceProvider[i]);
                    Log.Default.WriteLine(LogLevels.Info, "Starting game with {0}", pieces.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i));

                    _gameStartTime = DateTime.Now;

                    // Reset sudden death
                    _isSuddenDeathActive = false;
                    if (Options.DelayBeforeSuddenDeath > 0)
                    {
                        _suddenDeathStartTime = DateTime.Now.AddMinutes(Options.DelayBeforeSuddenDeath);
                        _isSuddenDeathActive = true;
                        Log.Default.WriteLine(LogLevels.Info, "Sudden death will be activated after {0} minutes and send lines every {1} seconds", Options.DelayBeforeSuddenDeath, Options.SuddenDeathTick);
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

                    State = ServerStates.GameStarted;

                    Log.Default.WriteLine(LogLevels.Info, "Game started");
                }
                else
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot start game, no players found");
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Cannot start game");
        }

        public void StopGame()
        {
            Log.Default.WriteLine(LogLevels.Info, "Stopping game");

            if (State == ServerStates.GameStarted || State == ServerStates.GamePaused)
            {
                State = ServerStates.GameFinished;

                GameStatistics statistics = PrepareGameStatistics();

                // Send game started to players
                foreach (IPlayer p in _playerManager.Players)
                {
                    p.State = PlayerStates.Registered;
                    p.OnGameFinished(statistics);
                }
                // Send game finished to spectators
                foreach (ISpectator spectator in _spectatorManager.Spectators)
                    spectator.OnGameFinished(statistics);

                State = ServerStates.WaitingStartGame;

                Log.Default.WriteLine(LogLevels.Info, "Game stopped");
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Cannot stop game");
        }

        public void PauseGame()
        {
            Log.Default.WriteLine(LogLevels.Info, "Pausing game");

            if (State == ServerStates.GameStarted)
            {
                State = ServerStates.GamePaused;

                // Send game paused to players and spectators
                foreach (IEntity entity in Entities)
                    entity.OnGamePaused();

                Log.Default.WriteLine(LogLevels.Info, "Game paused");
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Cannot pause game");
        }

        public void ResumeGame()
        {
            Log.Default.WriteLine(LogLevels.Info, "Resuming game");
            if (State == ServerStates.GamePaused)
            {
                State = ServerStates.GameStarted;

                // Reset sudden death
                _lastSuddenDeathAddLines = DateTime.Now; // TODO: a player, could pause<->resume forever to avoid sudden death

                // Send game resumed to players and spectators
                foreach (IEntity entity in Entities)
                    entity.OnGameResumed();

                Log.Default.WriteLine(LogLevels.Info, "Game resumed");
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Cannot resume game");
        }

        public void ResetWinList()
        {
            Log.Default.WriteLine(LogLevels.Info, "Resetting win list");

            if (State == ServerStates.WaitingStartGame)
            {
                // Reset
                WinList.Clear();

                // Inform players and spectators
                IPlayer serverMaster = _playerManager.ServerMaster;
                foreach (IEntity entity in Entities)
                {
                    entity.OnPublishServerMessage(String.Format("Win list resetted by {0}", serverMaster == null ? "SERVER" : serverMaster.Name));
                    entity.OnWinListModified(WinList);
                }
                Log.Default.WriteLine(LogLevels.Info, "Win list resetted");
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Cannot reset win list");
        }

        #endregion

        private void ResetStatistics()
        {
            // Delete previous stats
            GameStatistics.Clear();
            // Create GameStatisticsByPlayer for each player
            foreach (IPlayer player in _playerManager.Players)
            {
                // Init stats
                GameStatisticsByPlayer stats = new GameStatisticsByPlayer
                    {
                        PlayerName = player.Name,
                        SpecialsUsed = new Dictionary<Specials, Dictionary<string, int>>()
                    };
                foreach (SpecialOccurancy occurancy in Options.SpecialOccurancies.Where(x => x.Occurancy > 0))
                {
                    Dictionary<string, int> specialsByPlayer = _playerManager.Players.Select(p => p.Name).ToDictionary(x => x, x => 0);
                    stats.SpecialsUsed.Add(occurancy.Value, specialsByPlayer);
                }
                // Add stats
                GameStatistics.Add(player.Name, stats);
            }
        }

        private void UpdateStatistics(string playerName, int linesCount)
        {
            if (linesCount == 1)
                GameStatistics[playerName].SingleCount++;
            else if (linesCount == 2)
                GameStatistics[playerName].DoubleCount++;
            else if (linesCount == 3)
                GameStatistics[playerName].TripleCount++;
            else if (linesCount >= 4)
                GameStatistics[playerName].TetrisCount++;
        }

        private void UpdateStatistics(string playerName, string targetName, Specials special)
        {
            GameStatistics[playerName].SpecialsUsed[special][targetName]++;
        }

        private void UpdateStatistics(string playerName, DateTime gameStartTime, DateTime lossTime)
        {
            GameStatistics[playerName].PlayingTime = (lossTime - gameStartTime).TotalSeconds;
        }

        private GameStatistics PrepareGameStatistics()
        {
            GameStatistics statistics = new GameStatistics
                {
                    GameStarted = _gameStartTime,
                    GameFinished = DateTime.Now,
                    Players = GameStatistics.Select(x => x.Value).ToList()
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

        private void EnqueueAction(Action action)
        {
            _gameActionQueue.Enqueue(action);
        }

        #region IHost event handler

        private void OnRegisterPlayer(IPlayer player, int playerId)
        {
            Log.Default.WriteLine(LogLevels.Info, "New player:[{0}]{1}|{2}", playerId, player.Name, player.Team);

            // Send player id back to player
            player.OnPlayerRegistered(RegistrationResults.RegistrationSuccessful, Version, playerId, State == ServerStates.GameStarted || State == ServerStates.GamePaused, _playerManager.ServerMaster == player, Options);

            // Inform new player about other players
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                player.OnPlayerJoined(p.Id, p.Name, p.Team);
            // Inform new player about spectators
            foreach (ISpectator s in _spectatorManager.Spectators)
                player.OnSpectatorJoined(s.Id, s.Name);

            // Inform other players and spectators about new player connected
            foreach (IEntity entity in Entities.Where(x => x != player))
                entity.OnPlayerJoined(playerId, player.Name, player.Team);

            // Server master
            IPlayer serverMaster = _playerManager.ServerMaster;
            if (serverMaster != null)
            {
                // Send new server master id
                if (serverMaster == player)
                    foreach (IEntity entity in Entities.Where(x => x != player))
                        entity.OnServerMasterChanged(serverMaster.Id);
                // Send server master id to player even if not modified
                player.OnServerMasterChanged(serverMaster.Id);
            }

            // If game is running, send grid for playing player and game lost for dead player
            if (State == ServerStates.GamePaused || State == ServerStates.GameStarted)
            {
                foreach(IPlayer p in _playerManager.Players)
                {
                    if (p.State == PlayerStates.Playing)
                        player.OnGridModified(p.Id, p.Grid);
                    else if (p.State == PlayerStates.GameLost)
                        player.OnPlayerLost(p.Id);
                }
            }

            // Send win list
            player.OnWinListModified(WinList);
        }

        private void OnUnregisterPlayer(IPlayer player)
        {
            Log.Default.WriteLine(LogLevels.Info, "Unregister player:{0}", player.Name);

            OnPayerLeft(player, LeaveReasons.Disconnected);
        }

        private void OnPlayerTeam(IPlayer player, string team)
        {
            Log.Default.WriteLine(LogLevels.Info, "Player team changed:{0}:{1}", player.Name, team);

            player.Team = String.IsNullOrWhiteSpace(team) ? null : team;
            // Send message to players and spectators
            foreach (IEntity entity in Entities)
                entity.OnPlayerTeamChanged(player.Id, team);
        }

        private void OnPublishMessage(IPlayer player, string msg)
        {
            Log.Default.WriteLine(LogLevels.Info, "PublishMessage:{0}:{1}", player.Name, msg);

            // Send message to players and spectators
            foreach (IEntity entity in Entities)
                entity.OnPublishPlayerMessage(player.Name, msg);
        }

        private void OnPlacePiece(IPlayer player, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            if (State == ServerStates.GameStarted)
                EnqueueAction(() => PlacePiece(player, pieceIndex, highestIndex, piece, orientation, posX, posY, grid));
            else
                Log.Default.WriteLine(LogLevels.Warning, "OnPlacePiece received from {0} while game is not started", player.Name);
        }

        private void OnUseSpecial(IPlayer player, IPlayer target, Specials special)
        {
            if (State == ServerStates.GameStarted)
                EnqueueAction(() => Special(player, target, special));
            else
                Log.Default.WriteLine(LogLevels.Warning, "OnUseSpecial received from {0} while game is not started", player.Name);
        }

        private void OnClearLines(IPlayer player, int count)
        {
            if (State == ServerStates.GameStarted)
            {
                UpdateStatistics(player.Name, count);
                if (Options.ClassicStyleMultiplayerRules && count > 1)
                {
                    int addLines = count - 1;
                    if (addLines >= 4)
                        // special case for Tetris and above
                        addLines = 4;
                    EnqueueAction(() => SendLines(player, addLines));
                }
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "OnClearLines received from {0} while game is not started", player.Name);
        }

        private void OnModifyGrid(IPlayer player, byte[] grid)
        {
            if (State == ServerStates.GameStarted)
                EnqueueAction(() => ModifyGrid(player, grid));
            else
                Log.Default.WriteLine(LogLevels.Warning, "OnClearLines received from {0} while game is not started", player.Name);
        }

        private void OnStartGame(IPlayer player)
        {
            Log.Default.WriteLine(LogLevels.Info, "StartGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                StartGame();
            }
        }

        private void OnStopGame(IPlayer player)
        {
            Log.Default.WriteLine(LogLevels.Info, "StopGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                StopGame();
            }
        }

        private void OnPauseGame(IPlayer player)
        {
            Log.Default.WriteLine(LogLevels.Info, "PauseGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                PauseGame();
            }
        }

        private void OnResumeGame(IPlayer player)
        {
            Log.Default.WriteLine(LogLevels.Info, "ResumeGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                ResumeGame();
            }
        }

        private void OnGameLost(IPlayer player)
        {
            EnqueueAction(() => GameLost(player)); // Must be handled even if game is not started
        }

        private void OnChangeOptions(IPlayer player, GameOptions options)
        {
            Log.Default.WriteLine(LogLevels.Info, "ChangeOptions:{0} {1}", player.Name, options);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player && State == ServerStates.WaitingStartGame)
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
                    Options = options; // Options will be sent to players when starting a new game
                    // Inform other players/spectators about options modification
                    foreach (IEntity entity in Entities.Where(e => e != player))
                        entity.OnOptionsChanged(Options);
                }
                else
                    Log.Default.WriteLine(LogLevels.Info, "Invalid options");
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Cannot change options");
        }

        private void OnKickPlayer(IPlayer player, int playerId)
        {
            Log.Default.WriteLine(LogLevels.Info, "KickPlayer:{0} [{1}]", player.Name, playerId);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            IPlayer kickedPlayer = _playerManager[playerId];
            if (masterPlayer == player && State == ServerStates.WaitingStartGame && kickedPlayer != null)
            {
                // Send server stopped
                kickedPlayer.OnServerStopped();
                // Remove player from player manager and hosts + warn other players
                OnPayerLeft(kickedPlayer, LeaveReasons.Kick);
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Cannot kick player");
        }

        private void OnBanPlayer(IPlayer player, int playerId)
        {
            Log.Default.WriteLine(LogLevels.Info, "BanPlayer:{0} [{1}]", player.Name, playerId);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            IPlayer bannedPlayer = _playerManager[playerId];
            if (masterPlayer == player && State == ServerStates.WaitingStartGame && bannedPlayer != null)
            {
                // Send server stopped
                bannedPlayer.OnServerStopped();
                // Remove player from player manager and hosts + warn other players
                OnPayerLeft(bannedPlayer, LeaveReasons.Ban);
                // TODO: add to ban list
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Cannot ban player");
        }

        private void OnResetWinList(IPlayer player)
        {
            Log.Default.WriteLine(LogLevels.Info, "ResetWinLost:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
            {
                ResetWinList();
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Cannot reset win list");
        }

        private void OnFinishContinuousSpecial(IPlayer player, Specials special)
        {
            EnqueueAction(() => FinishContinuousSpecial(player, special)); // Must be handled even if game is not started
        }

        private void OnEarnAchievement(IPlayer player, int achievementId, string achievementTitle)
        {
            Log.Default.WriteLine(LogLevels.Info, "EarnAchievement:{0} {1} {2}", player.Name, achievementId, achievementTitle);

            foreach (IEntity entity in Entities.Where(x => x != player))
                entity.OnAchievementEarned(player.Id, achievementId, achievementTitle);
        }

        private void OnRegisterSpectator(ISpectator spectator, int spectatorId)
        {
            Log.Default.WriteLine(LogLevels.Info, "New spectator:[{0}]{1}", spectatorId, spectator.Name);

            // Send spectator id back to spectator
            spectator.OnSpectatorRegistered(RegistrationResults.RegistrationSuccessful, Version, spectatorId, State == ServerStates.GameStarted || State == ServerStates.GamePaused, Options);

            // Inform new spectator about players
            foreach (IPlayer p in _playerManager.Players)
                spectator.OnPlayerJoined(p.Id, p.Name, p.Team);
            // Inform new spectator about other spectators
            foreach(ISpectator s in _spectatorManager.Spectators.Where(x => x != spectator))
                spectator.OnSpectatorJoined(s.Id, s.Name);

            // Inform players and other spectators about new spectator connected
            foreach (IEntity entity in Entities.Where(x => x != spectator))
                entity.OnSpectatorJoined(spectatorId, spectator.Name);

            // If game is running, send grid for playing player and game lost for dead player
            if (State == ServerStates.GamePaused || State == ServerStates.GameStarted)
            {
                foreach (IPlayer p in _playerManager.Players)
                {
                    if (p.State == PlayerStates.Playing)
                        spectator.OnGridModified(p.Id, p.Grid);
                    else if (p.State == PlayerStates.GameLost)
                        spectator.OnPlayerLost(p.Id);
                }
            }

            // Send win list
            spectator.OnWinListModified(WinList);
        }

        private void OnUnregisterSpectator(ISpectator spectator)
        {
            Log.Default.WriteLine(LogLevels.Info, "Unregister spectator:{0}", spectator.Name);

            OnSpectatorLeft(spectator, LeaveReasons.Disconnected);
        }

        private void OnPublishSpectatorMessage(ISpectator spectator, string msg)
        {
            Log.Default.WriteLine(LogLevels.Info, "PublishSpectatorMessage:{0}:{1}", spectator.Name, msg);

            // Send message to players and spectators
            foreach (IEntity entity in Entities)
                entity.OnPublishPlayerMessage(spectator.Name, msg);
        }

        private void OnPayerLeft(IPlayer player, LeaveReasons reason)
        {
            Log.Default.WriteLine(LogLevels.Info, "Player left:{0} {1}", player.Name, reason);

            bool wasServerMaster = false;
            // Remove from player manager
            lock (_playerManager.LockObject)
            {
                if (player == _playerManager.ServerMaster)
                    wasServerMaster = true;
                // Remove player from player list
                _playerManager.Remove(player);
            }

            // Clean host tables
            foreach (IHost host in _hosts)
                host.RemovePlayer(player);

            // If game was running and player was playing, check if only one player left (see GameLostHandler)
            if ((State == ServerStates.GameStarted || State == ServerStates.GamePaused) && player.State == PlayerStates.Playing)
            {
                int playingCount = _playerManager.Players.Count(p => p.State == PlayerStates.Playing);
                if (playingCount == 0 || playingCount == 1)
                {
                    Log.Default.WriteLine(LogLevels.Info, "Game finished by forfeit no winner");
                    State = ServerStates.GameFinished;
                    GameStatistics statistics = PrepareGameStatistics();
                    // Send game finished (no winner)
                    foreach (IEntity entity in Entities.Where(x => x != player))
                        entity.OnGameFinished(statistics);
                    State = ServerStates.WaitingStartGame;
                }
            }

            // Inform players and spectators except disconnected player
            foreach (IEntity entity in Entities.Where(x => x != player))
                entity.OnPlayerLeft(player.Id, player.Name, reason);

            // Send new server master id
            if (wasServerMaster)
            {
                IPlayer serverMaster = _playerManager.ServerMaster;
                if (serverMaster != null)
                    foreach (IEntity entity in Entities.Where(x => x != player))
                        entity.OnServerMasterChanged(serverMaster.Id);
            }
        }

        private void OnSpectatorLeft(ISpectator spectator, LeaveReasons reason)
        {
            Log.Default.WriteLine(LogLevels.Info, "Spectator left:{0} {1}", spectator.Name, reason);

            // Remove from spectator
            lock (_spectatorManager.LockObject)
            {
                // Remove spectator from spectator list
                _spectatorManager.Remove(spectator);
            }

            // Clean host tables
            foreach (IHost host in _hosts)
                host.RemoveSpectator(spectator);

            // Inform players and spectators except disconnected spectator
            foreach (IEntity entity in Entities.Where(x => x != spectator))
                entity.OnSpectatorLeft(spectator.Id, spectator.Name, reason);
        }

        #endregion

        #region Game actions

        private void PlacePiece(IPlayer player, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            Log.Default.WriteLine(LogLevels.Info, "PlacePiece[{0}]{1}:{2} {3} {4} at {5},{6} {7}", player.Name, pieceIndex, highestIndex, piece, orientation, posX, posY, grid == null ? -1 : grid.Count(x => x > 0));

            //if (index != player.PieceIndex)
            //    Log.Default.WriteLine(LogLevels.Error, "!!!! piece index different for player {0} local {1} remote {2}", player.Name, player.PieceIndex, index);

            bool sendNextPieces = false;
            // Set grid
            player.Grid = grid;
            // Get next piece
            player.PieceIndex = pieceIndex;
            List<Pieces> nextPiecesToSend = new List<Pieces>();
            Log.Default.WriteLine(LogLevels.Info, "{0} {1} indexes: {2} {3}", player.Id, player.Name, highestIndex, pieceIndex);
            if (highestIndex < pieceIndex)
                Log.Default.WriteLine(LogLevels.Error, "PROBLEM WITH INDEXES!!!!!");
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
            foreach(IEntity callback in Entities.Where(x => x != player))
                callback.OnGridModified(player.Id, grid);

            if (sendNextPieces)
            {
                Log.Default.WriteLine(LogLevels.Debug, "Send next piece {0} {1} {2}", highestIndex, pieceIndex, nextPiecesToSend.Any() ? nextPiecesToSend.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i) : String.Empty);
                // Send next pieces
                player.OnNextPiece(highestIndex, nextPiecesToSend);
            }

        }

        private void Special(IPlayer player, IPlayer target, Specials special)
        {
            Log.Default.WriteLine(LogLevels.Info, "UseSpecial[{0}][{1}]{2}", player.Name, target.Name, special);

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
                player.OnGridModified(player.Id, player.Grid);
                if (player != target)
                    target.OnGridModified(target.Id, target.Grid);
                // They will send their grid when receiving them (with an optional capping)
            }
            // Inform about special use
            foreach (IEntity entity in Entities)
                entity.OnSpecialUsed(specialId, player.Id, target.Id, special);
        }

        private void SendLines(IPlayer player, int count)
        {
            Log.Default.WriteLine(LogLevels.Info, "SendLines[{0}]:{1}", player.Name, count);

            // Store special id locally
            int specialId = SpecialId;
            // Increment special id
            SpecialId++;
            // Send lines to everyone including sender (so attack msg can be displayed)
            foreach (IEntity entity in Entities)
                entity.OnPlayerAddLines(specialId, player.Id, count);
        }

        private void ModifyGrid(IPlayer player, byte[] grid)
        {
            Log.Default.WriteLine(LogLevels.Info, "ModifyGrid[{0}]", player.Name);

            // Set grid
            player.Grid = grid;
            // Send grid modification to everyone except sender
            foreach (IEntity entity in Entities.Where(x => x != player))
                entity.OnGridModified(player.Id, player.Grid);
        }

        private void GameLost(IPlayer player)
        {
            Log.Default.WriteLine(LogLevels.Info, "GameLost[{0}]  {1}", player.Name, State);

            if (player.State == PlayerStates.Playing)
            {
                // Set player state
                player.State = PlayerStates.GameLost;
                player.LossTime = DateTime.Now;

                // Inform other players and spectators
                foreach (IEntity entity in Entities.Where(x => x != player))
                    entity.OnPlayerLost(player.Id);

                UpdateStatistics(player.Name, _gameStartTime, player.LossTime);

                //
                int playingCount = _playerManager.Players.Count(p => p.State == PlayerStates.Playing);
                if (playingCount == 0) // there were only one playing player
                {
                    Log.Default.WriteLine(LogLevels.Info, "Game finished with only one player playing, no winner");
                    State = ServerStates.GameFinished;
                    // Send game finished (no winner) to players and spectators
                    GameStatistics statistics = PrepareGameStatistics();
                    foreach (IEntity entity in Entities)
                        entity.OnGameFinished(statistics);
                    State = ServerStates.WaitingStartGame;
                }
                else if (playingCount == 1) // only one playing left
                {
                    Log.Default.WriteLine(LogLevels.Info, "Game finished checking winner");
                    State = ServerStates.GameFinished;
                    // Game won
                    IPlayer winner = _playerManager.Players.Single(p => p.State == PlayerStates.Playing);
                    winner.State = PlayerStates.Registered;
                    Log.Default.WriteLine(LogLevels.Info, "Winner: {0}[{1}]", winner.Name, winner.Id);

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
                        entity.OnPlayerWon(winner.Id);
                        entity.OnGameFinished(statistics);
                        entity.OnWinListModified(WinList);
                    }
                    State = ServerStates.WaitingStartGame;
                }
            }
            else
                Log.Default.WriteLine(LogLevels.Info, "Game lost from non-playing player {0} {1}", player.Name, player.State);
        }

        private void FinishContinuousSpecial(IPlayer player, Specials special)
        {
            Log.Default.WriteLine(LogLevels.Info, "FinishContinuousSpecial[{0}]: {1}", player.Name, special);

            // Send to everyone except sender
            foreach (IEntity entity in Entities.Where(x => x != player))
                entity.OnContinuousSpecialFinished(player.Id, special);
        }

        #endregion

        private void TimeoutTask()
        {
            Log.Default.WriteLine(LogLevels.Info, "TimeoutTask started");

            try
            {
                while (true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "Stop background task event raised");
                        break;
                    }

                    // Check sudden death
                    if (State == ServerStates.GameStarted && _isSuddenDeathActive)
                    {
                        if (DateTime.Now > _suddenDeathStartTime)
                        {
                            TimeSpan timespan = DateTime.Now - _lastSuddenDeathAddLines;
                            if (timespan.TotalSeconds >= Options.SuddenDeathTick)
                            {
                                Log.Default.WriteLine(LogLevels.Info, "Sudden death tick");
                                // Delay elapsed, send lines
                                foreach (IPlayer p in _playerManager.Players.Where(p => p.State == PlayerStates.Playing))
                                    p.OnServerAddLines(1);
                                _lastSuddenDeathAddLines = DateTime.Now;
                            }
                        }
                    }

                    // Check running game without any player
                    if (State == ServerStates.GameStarted || State == ServerStates.GamePaused)
                    {
                        if (_playerManager.Players.Count(x => x.State == PlayerStates.Playing) == 0)
                        {
                            Log.Default.WriteLine(LogLevels.Info, "Game finished because no more playing players");
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
                            Log.Default.WriteLine(LogLevels.Info, "Timeout++ for player {0}", p.Name);
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
                            Log.Default.WriteLine(LogLevels.Info, "Timeout++ for player {0}", s.Name);
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
                        Log.Default.WriteLine(LogLevels.Info, "Stop background task event raised");
                        break;
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "TimeoutTask exception. Exception: {0}", ex);
            }

            Log.Default.WriteLine(LogLevels.Info, "TimeoutTask stopped");
        }

        // Check if every events of instance are handled
        private static bool CheckEvents<T>(T instance)
        {
            Type t = instance.GetType();
            EventInfo[] events = t.GetEvents();
            foreach (EventInfo e in events)
            {
                if (e.DeclaringType == null)
                    return false;
                FieldInfo fi = e.DeclaringType.GetField(e.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (fi == null)
                    return false;
                object value = fi.GetValue(instance);
                if (value == null)
                    return false;
            }
            return true;
        }
    }
}
