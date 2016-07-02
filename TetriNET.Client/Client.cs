using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Attributes;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.Common.Randomizer;

namespace TetriNET.Client
{
    public sealed class Client : ITetriNETCallback, IClient
    {
        private const int BoardWidth = 12;
        private const int BoardHeight = 22;
        private const int MaxPlayers = 6;
        private const int MaxSpectators = 10;
        private const int MaxLevel = 100;
        private const int GameTimerIntervalStartValue = 1050; // level 0: 1050, level 1: 1040, ..., level 100: 50
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const bool IsTimeoutDetectionActive = false;
        private const int SpawnOrientation = 1;
        private const bool AutomaticallyMoveDown = true; // should always be true, except for some test

        public enum States
        {
            Created, // -> Registering
            Registering, // --> Registered
            Registered, // --> Playing
            Playing, // --> Paused | Registered
            Paused // --> Playing | Registered
        }

        public enum ServerStates
        {
            Waiting,
            Playing,
            Paused
        }

        private readonly IFactory _factory;
        private readonly PlayerData[] _playersData = new PlayerData[MaxPlayers];
        private readonly SpectatorData[] _spectatorData = new SpectatorData[MaxSpectators];
        private readonly PieceArray _pieces;
        private readonly Inventory _inventory;
        private readonly Statistics _statistics;
        private readonly IAchievementManager _achievementManager;
        private readonly IActionQueue _actionQueue;
        private readonly System.Timers.Timer _gameTimer;

        private readonly Task _timeoutTask;
        private readonly CancellationTokenSource _cancellationTokenSource; // TODO: use token to cancel task + wait for task


        public States State { get; private set; }
        public ServerStates ServerState { get; private set; }

        private IProxy _proxy;
        private ISpectatorProxy _proxySpectator;
        private int _clientSpectatorId;
        private DateTime _lastActionFromServer;
        private int _timeoutCount;
        private int _pieceIndex;
        private DateTime _pauseStartTime;
        private bool _isDarknessActive;
        private bool _isConfusionActive;
        private bool _isImmunityActive;
        private DateTime _darknessEndTime;
        private DateTime _confusionEndTime;
        private DateTime _immunityEndTime;
        private int _mutationCount;
        private bool _holdAlreadyUsed;

        private PlayerData Player => PlayerId >= 0 && PlayerId <= MaxPlayers ? _playersData[PlayerId] : null;

        public Client(IFactory factory, IActionQueue actionQueue)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _factory = factory;
            _actionQueue = actionQueue;

            // default options
            Options = new GameOptions();
            Options.ResetToDefault();

            _pieces = new PieceArray(64);
            _inventory = new Inventory(Options.InventorySize);
            _statistics = new Statistics();

            _gameTimer = new System.Timers.Timer
            {
                Interval = GameTimerIntervalStartValue
            };
            _gameTimer.Elapsed += GameTimerOnElapsed;

            _lastActionFromServer = DateTime.Now;
            _timeoutCount = 0;

            PlayerId = -1;
            _clientSpectatorId = -1;
           
            State = States.Created;
            ServerState = ServerStates.Waiting;
            IsServerMaster = false;
            _isDarknessActive = false;
            _isConfusionActive = false;
            _isImmunityActive = false;
            _pauseStartTime = DateTime.Now;
            _mutationCount = 0;

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

            _cancellationTokenSource = new CancellationTokenSource();
            _timeoutTask = Task.Factory.StartNew(TimeoutTask, _cancellationTokenSource.Token);
            _actionQueue.Start(_cancellationTokenSource);
        }

        public Client(IFactory factory, IActionQueue actionQueue, IAchievementManager achievementManager)
            : this(factory, actionQueue)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (achievementManager == null)
                throw new ArgumentNullException(nameof(achievementManager));

            _achievementManager = achievementManager;
            _achievementManager.Achieved += OnAchievementEarned;
        }

        #region IAchievementManager event handler

        private void OnAchievementEarned(IAchievement achievement, bool firstTime)
        {
            if (firstTime)
                _proxy.Do(x => x.EarnAchievement(this, achievement.Id, achievement.Title));

            AchievementEarned.Do(x => x(achievement, firstTime));
        }
        
        #endregion

        #region IProxy event handler

        private void OnConnectionLost()
        {
            States previousState = State;
            State = States.Created;
            IsServerMaster = false;
            PlayerId = -1;
            _clientSpectatorId = -1;

            Disconnect();

            if (ConnectionLost != null)
                ConnectionLost(previousState == States.Registering ? ConnectionLostReasons.ServerNotFound : ConnectionLostReasons.Other);
            else
                throw new ApplicationException("Connection lost");
        }

        #endregion

        #region ITetriNETCallback

        public void OnHeartbeatReceived()
        {
            ResetTimeout();
        }

        public void OnServerStopped()
        {
            ResetTimeout();
            OnConnectionLost();
        }

        public void OnPlayerRegistered(RegistrationResults result, Versioning serverVersion, int playerId, bool isGameStarted, bool isServerMaster, GameOptions options)
        {
            ResetTimeout();
            if (result == RegistrationResults.RegistrationSuccessful && State == States.Registering)
            {
                Log.Default.WriteLine(LogLevels.Info, "Registered as player {0} game started {1}", playerId, isGameStarted);

                if (playerId >= 0 && playerId < MaxPlayers)
                {
                    IsSpectator = false;
                    PlayerId = playerId;
                    PlayerData player = new PlayerData
                    {
                        Name = Name,
                        PlayerId = playerId,
                        Board = _factory.CreateBoard(BoardWidth, BoardHeight),
                        State = PlayerData.States.Joined
                    };
                    _playersData[PlayerId] = player;

                    State = States.Registered;
                    ServerState = isGameStarted ? ServerStates.Playing : ServerStates.Waiting;// TODO: handle server paused
                    IsServerMaster = isServerMaster;
                    Options = options;
                    _statistics.Reset();
                    LinesCleared = 0;
                    Level = 0;
                    Score = 0;
                    _isConfusionActive = false;
                    _isDarknessActive = false;
                    _isImmunityActive = false;
                    _mutationCount = 0;
                    _holdAlreadyUsed = false;
                    HoldPiece = null;

                    RegisteredAsPlayer.Do(x => x(RegistrationResults.RegistrationSuccessful, serverVersion, playerId, isServerMaster));

                    if (isGameStarted)
                    {
                        player.Board.FillWithRandomCells(() => RangeRandom.Random(Options.PieceOccurancies));

                        Redraw.Do(x => x());
                    }
                }
                else
                {
                    State = States.Created;
                    IsServerMaster = false;
                    Log.Default.WriteLine(LogLevels.Warning, "Wrong id {0}", playerId);

                    RegisteredAsPlayer.Do(x => x(RegistrationResults.RegistrationFailedInvalidId, serverVersion, -1, false));
                }
            }
            else
            {
                State = States.Created;
                IsServerMaster = false;

                RegisteredAsPlayer.Do(x => x(result, serverVersion, -1, false));

                Log.Default.WriteLine(LogLevels.Warning, "Registration failed {0}. Version: {1}.{2}", result, serverVersion.Major, serverVersion.Minor);
            }
        }

        public void OnPlayerJoined(int playerId, string name, string team)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Player {0}{1}[{2}] joined", name, team, playerId);

            ResetTimeout();
            // Don't update ourself
            if (playerId != PlayerId && playerId >= 0 && playerId < MaxPlayers)
            {
                PlayerData playerData = new PlayerData
                {
                    Name = name,
                    Team = team,
                    PlayerId = playerId,
                    Board = _factory.CreateBoard(BoardWidth, BoardHeight),
                    IsImmune = false,
                    State = PlayerData.States.Joined
                };
                _playersData[playerId] = playerData;

                PlayerJoined.Do(x => x(playerId, name, team));

                if (IsGameStarted)
                {
                    playerData.Board.FillWithRandomCells(() => RangeRandom.Random(Options.PieceOccurancies));

                    RedrawBoard.Do(x => x(playerId, playerData.Board));
                }
            }
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Player {0}[{1}] left ({2})", name, playerId, reason);

            ResetTimeout();
            if (playerId != PlayerId && playerId >= 0 && playerId < MaxPlayers)
            {
                PlayerData player = _playersData[playerId];
                if (player != null)
                {
                    player.Board.Clear();
                    RedrawBoard.Do(x => x(playerId, player.Board));
                }

                _playersData[playerId] = null;

                PlayerLeft.Do(x => x(playerId, name, reason));
            }
        }

        public void OnPlayerTeamChanged(int playerId, string team)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Player [{0}] team ({1})", playerId, team);

            ResetTimeout();

            if (playerId >= 0 && playerId < MaxPlayers)
            {
                PlayerData player = _playersData[playerId];
                if (player != null)
                {
                    player.Team = team;

                    PlayerTeamChanged.Do(x => x(playerId, team));
                }
            }
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            Log.Default.WriteLine(LogLevels.Debug, "{0}:{1}", playerName, msg);

            ResetTimeout();

            PlayerPublishMessage.Do(x => x(playerName, msg));
        }

        public void OnPublishServerMessage(string msg)
        {
            Log.Default.WriteLine(LogLevels.Debug, "{0}", msg);

            ResetTimeout();

            ServerPublishMessage.Do(x => x(msg));
        }

        public void OnPlayerLost(int playerId)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Player [{0}] has lost", playerId);

            ResetTimeout();
            if (playerId != PlayerId)
            {
                PlayerData playerData = GetPlayer(playerId);
                if (playerData != null && playerData.State == PlayerData.States.Playing)
                {
                    playerData.State = PlayerData.States.Lost;
                    playerData.IsImmune = false;
                    playerData.Board.FillWithRandomCells(() => RangeRandom.Random(Options.PieceOccurancies));

                    RedrawBoard.Do(x => x(playerId, playerData.Board));
                    PlayerLost.Do(x => x(playerId, playerData.Name));
                }
            }
        }

        public void OnPlayerWon(int playerId)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Player [{0}] has won", playerId);

            ResetTimeout();

            PlayerData playerData = _playersData[playerId];
            if (playerData != null)
            {
                playerData.State = PlayerData.States.Joined;
                PlayerWon.Do(x => x(playerId, playerData.Name));

                if (playerId == PlayerId)
                    _statistics.GameWon++;

                if (!IsSpectator)
                {
                    if (_achievementManager != null && playerId == PlayerId)
                        _achievementManager.OnGameWon(_statistics.MoveCount, LinesCleared, PlayingOpponentsInCurrentGame);
                }
            }
        }

        private void OnGameStartedSpectator()
        {
            // Set state
            ServerState = ServerStates.Playing;
 
            // Reset boards
            PlayingOpponentsInCurrentGame = 0;
            for (int i = 0; i < MaxPlayers; i++)
            {
                PlayerData playerData = _playersData[i];
                if (playerData != null)
                {
                    if (playerData.Board != null)
                    {
                        playerData.Board.Clear();
                        //for (int y = 1; y <= 3; y++)
                        //    for (int x = 1; x <= playerData.Board.Width; x++)
                        //        playerData.Board.Cells[playerData.Board.GetCellIndex(x, y)] = (x % 2 == 0) ? (byte)5 : (byte)Specials.BlockBomb;
                    }
                    playerData.State = PlayerData.States.Playing;
                    playerData.IsImmune = false;
                    PlayingOpponentsInCurrentGame++;
                }
            }

            GameStarted.Do(x => x());
        }

        private void OnGameStartedPlayer(List<Pieces> pieces)
        {
            // Reset board action list
            _actionQueue.Reset();

            // Set state
            State = States.Playing;
            ServerState = ServerStates.Playing;
            // Reset statistics
            _statistics.Reset();
            // Reset pieces
            _pieces.Reset();
            for (int i = 0; i < pieces.Count; i++)
                _pieces[i] = pieces[i];
            _pieceIndex = 0;
            CurrentPiece = _factory.CreatePiece(_pieces[0], Board.PieceSpawnX, Board.PieceSpawnY, SpawnOrientation, 0, false);
            MoveCurrentPieceToBoardTop();
            NextPiece = _factory.CreatePiece(_pieces[1], Board.PieceSpawnX, Board.PieceSpawnY, SpawnOrientation, 1, false);
            // Update statistics
            if (_statistics.PieceCount.ContainsKey(_pieces[0]))
                _statistics.PieceCount[_pieces[0]]++;
            // Reset inventory
            _inventory.Reset(Options.InventorySize);
            //_inventory.Enqueue(new List<Specials>
            //{
            //    //Specials.Darkness,
            //    //Specials.Confusion,
            //    //Specials.Confusion,
            //    //Specials.Darkness,
            //    //Specials.Immunity,
            //    //Specials.Immunity,
            //    //Specials.NukeField,
            //    //Specials.NukeField,
            //    //Specials.NukeField,
            //    //Specials.NukeField,
            //    //Specials.BlockBomb,
            //    //Specials.BlockBomb,
            //    //Specials.BlockBomb,
            //    //Specials.BlockBomb,
            //});
            // Reset line, level and score
            LinesCleared = 0;
            Level = Options.StartingLevel;
            Score = 0;
            // Reset gamer timer interval
            _gameTimer.Interval = ComputeGameTimerInterval(Level);
            // Reset specials
            _isConfusionActive = false;
            _isDarknessActive = false;
            _isImmunityActive = false;
            _mutationCount = 0;
            // Reset hold
            _holdAlreadyUsed = false;
            HoldPiece = null;
            // Reset boards
            PlayingOpponentsInCurrentGame = 0;
            for (int i = 0; i < MaxPlayers; i++)
            {
                PlayerData playerData = _playersData[i];
                if (playerData != null)
                {
                    if (playerData.Board != null)
                    {
                        playerData.Board.Clear();
                        //for (int y = 1; y <= 3; y++)
                        //    for (int x = 1; x <= playerData.Board.Width; x++)
                        //        playerData.Board.Cells[playerData.Board.GetCellIndex(x, y)] = (x % 2 == 0) ? (byte)5 : (byte)Specials.BlockBomb;
                    }
                    playerData.State = PlayerData.States.Playing;
                    playerData.IsImmune = false;
                    PlayingOpponentsInCurrentGame++;
                }
            }
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));
            // Restart timer
            _gameTimer.Start();

            GameStarted.Do(x => x());

            _achievementManager.Do(x => x.OnGameStarted(Options));

            //Log.Default.WriteLine("PIECES:{0}", _pieces.Dump(8));
        }

        public void OnGameStarted(List<Pieces> pieces)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Game started with {0}", pieces.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i));

            ResetTimeout();

            if (IsSpectator)
                OnGameStartedSpectator();
            else
                OnGameStartedPlayer(pieces);
        }

        public void OnGameFinished(GameStatistics statistics)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Game finished");

            _gameTimer.Stop();

            ResetTimeout();
            State = States.Registered;
            ServerState = ServerStates.Waiting;
            for (int i = 0; i < MaxPlayers; i++)
            {
                PlayerData playerData = _playersData[i];
                if (playerData != null)
                {
                    playerData.State = PlayerData.States.Joined;
                    playerData.IsImmune = false;
                }
            }

            // Reset board action list
           _actionQueue.Reset();

            GameFinished.Do(x => x(statistics));

            if (!IsSpectator)
                _achievementManager.Do(x => x.OnGameFinished());
        }

        public void OnGamePaused()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Game paused");

            ResetTimeout();

            ServerState = ServerStates.Paused;
            _pauseStartTime = DateTime.Now;

            if (State == States.Playing)
                State = States.Paused;

            GamePaused.Do(x => x());
        }

        public void OnGameResumed()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Game resumed");

            ResetTimeout();

            ServerState = ServerStates.Playing;
            TimeSpan pauseTime = DateTime.Now - _pauseStartTime;
            // Add pause time to confusion and darkness end time
            if (_isConfusionActive)
                _confusionEndTime = _confusionEndTime.Add(pauseTime);
            if (_isDarknessActive)
                _darknessEndTime = _darknessEndTime.Add(pauseTime);
            if (_isImmunityActive)
                _immunityEndTime = _immunityEndTime.Add(pauseTime);

            if (State == States.Paused)
                State = States.Playing;

            GameResumed.Do(x => x());
        }

        public void OnServerAddLines(int lineCount)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Server add {0} lines", lineCount);

            ResetTimeout();

            if (State == States.Playing)
            {
                EnqueueBoardAction(() => AddLines(lineCount));
            }
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Player {0} add {1} lines (special [{2}])", playerId, lineCount, specialId);

            ResetTimeout();

            //if (State == States.Playing)
            //{
                // Don't add line to ourself
                if (playerId != PlayerId && State == States.Playing)
                {
                    EnqueueBoardAction(() => AddLines(lineCount));
                }

                if (PlayerAddLines != null)
                {
                    PlayerData playerData = GetPlayer(playerId);
                    string playerName = playerData == null ? "???" : playerData.Name;

                    PlayerAddLines(playerId, playerName, specialId, lineCount);
                }
            //}
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Special {0}[{1}] from {2} to {3}", special, specialId, playerId, targetId);

            ResetTimeout();
            if (targetId == PlayerId && State == States.Playing)
            {
                EnqueueBoardAction(() => SpecialUsedAction(special));
            }

            if (targetId != PlayerId && special == Specials.Immunity)
            {
                PlayerData target = GetPlayer(targetId);
                if (target != null && target.State == PlayerData.States.Playing)
                    target.IsImmune = true;
            }

            // Messages are displayed immediately, only board action is enqueued
            if (SpecialUsed != null)
            {
                PlayerData playerData = GetPlayer(playerId);
                PlayerData targetData = GetPlayer(targetId);

                string playerName = playerData == null ? "???" : playerData.Name;
                string targetName = targetData == null ? "???" : targetData.Name;

                SpecialUsed(playerId, playerName, targetId, targetName, specialId, special);

                //
                if (!IsSpectator)
                    _achievementManager.Do(x => x.OnSpecialUsed(PlayerId, playerId, playerData == null ? null : playerData.Team, playerData == null ? null : playerData.Board, targetId, targetData == null ? null : targetData.Team, targetData == null ? null : targetData.Board, special));
            }
        }

        public void OnNextPiece(int index, List<Pieces> pieces)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Next piece: {0} {1}", index, pieces.Any() ? pieces.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i) : String.Empty);

            ResetTimeout();

            if (State == States.Playing)
            {
                for (int i = 0; i < pieces.Count; i++)
                    _pieces[index + i] = pieces[i];
            }
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Player [{0}] modified", playerId);

            ResetTimeout();

            PlayerData playerData = GetPlayer(playerId);
            if (playerData != null)
            {
                // if modifying own grid, special Switch occured -> remove lines above 16
                if (playerId == PlayerId)
                {
                    EnqueueBoardAction(() => ModifyGridAction(grid));
                }
                else
                {
                    playerData.Board.SetCells(grid);
                    RedrawBoard.Do(x => x(playerId, playerData.Board));
                }
            }
        }

        public void OnServerMasterChanged(int playerId)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Server master changed: [{0}]", playerId);

            ResetTimeout();

            IsServerMaster = playerId == PlayerId;

            ServerMasterModified.Do(x => x(playerId));
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Win list: {0}", winList.Any() ? winList.Select(x => String.Format("{0}[{1}]:{2}", x.PlayerName, x.Team, x.Score)).Aggregate((n, i) => n + "|" + i) : "");

            ResetTimeout();

            WinListModified.Do(x => x(winList));
        }

        public void OnContinuousSpecialFinished(int playerId, Specials special)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Continous special {0} finished on {1}", special, playerId);

            ResetTimeout();
            if (playerId != PlayerId)
            {
                if (special == Specials.Immunity)
                {
                    PlayerData target = GetPlayer(playerId);
                    if (target != null)
                        target.IsImmune = false;
                }
                ContinuousSpecialFinished.Do(x => x(playerId, special));
            }
        }

        public void OnAchievementEarned(int playerId, int achievementId, string achievementTitle)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Achievement {0}|{1} earned by {2}", achievementId, achievementTitle, playerId);

            ResetTimeout();
            if (playerId != PlayerId || IsSpectator)
            {
                if (PlayerAchievementEarned != null)
                {
                    PlayerData player = GetPlayer(playerId);
                    PlayerAchievementEarned(playerId, player == null ? "???" : player.Name, achievementId, achievementTitle);
                }
            }
        }

        public void OnOptionsChanged(GameOptions options)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Options changed");

            ResetTimeout();

            // Reset options
            Options = options;

            OptionsChanged.Do(x => x());
        }

        public void OnSpectatorRegistered(RegistrationResults result, Versioning serverVersion, int spectatorId, bool isGameStarted, GameOptions options)
        {
            ResetTimeout();
            if (result == RegistrationResults.RegistrationSuccessful && State == States.Registering)
            {
                Log.Default.WriteLine(LogLevels.Info, "Registered as spectator {0} game started {1}", spectatorId, isGameStarted);

                if (spectatorId >= 0 && spectatorId < MaxSpectators)
                {
                    IsSpectator = true;
                    _clientSpectatorId = spectatorId;
                    SpectatorData player = new SpectatorData
                    {
                        Name = Name,
                        SpectatorId = spectatorId
                    };
                    _spectatorData[_clientSpectatorId] = player;

                    State = States.Registered;
                    ServerState = isGameStarted ? ServerStates.Playing : ServerStates.Waiting;// TODO: handle server paused
                    IsServerMaster = false;
                    Options = options;
                    _statistics.Reset();
                    LinesCleared = 0;
                    Level = 0;
                    Score = 0;
                    _isConfusionActive = false;
                    _isDarknessActive = false;
                    _isImmunityActive = false;
                    _mutationCount = 0;
                    _holdAlreadyUsed = false;
                    HoldPiece = null;


                    RegisteredAsSpectator.Do(x => x(RegistrationResults.RegistrationSuccessful, serverVersion, spectatorId));
                }
                else
                {
                    State = States.Created;
                    IsServerMaster = false;
                    Log.Default.WriteLine(LogLevels.Warning, "Wrong id {0}", spectatorId);

                    RegisteredAsSpectator.Do(x => x(RegistrationResults.RegistrationFailedInvalidId, serverVersion, -1));
                }
            }
            else
            {
                State = States.Created;
                IsServerMaster = false;

                RegisteredAsSpectator.Do(x => x(result, serverVersion, -1));

                Log.Default.WriteLine(LogLevels.Warning, "Registration failed {0}. Version: {1}.{2}", result, serverVersion.Major, serverVersion.Minor);
            }
        }

        public void OnSpectatorJoined(int spectatorId, string name)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Spectator {0}[{1}] joined", name, spectatorId);

            ResetTimeout();
            // Don't update ourself
            if (spectatorId != _clientSpectatorId && spectatorId >= 0 && spectatorId < MaxSpectators)
            {
                SpectatorData spectatorData = new SpectatorData
                {
                    Name = name,
                    SpectatorId = spectatorId
                };
                _spectatorData[spectatorId] = spectatorData;

                SpectatorJoined.Do(x => x(spectatorId, name));
            }
        }

        public void OnSpectatorLeft(int spectatorId, string name, LeaveReasons reason)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Spectator {0}[{1}] left ({2})", name, spectatorId, reason);

            ResetTimeout();
            if (spectatorId != _clientSpectatorId && spectatorId >= 0 && spectatorId < MaxPlayers)
            {
                _spectatorData[spectatorId] = null;

                SpectatorLeft.Do(x => x(spectatorId, name, reason));
            }
        }

        #endregion

        private static double ComputeGameTimerInterval(int level)
        {
            double interval = GameTimerIntervalStartValue - (level * 10);  // Reduce by 10ms each level
            if (interval < 10)
                interval = 10; // no less than 10ms
            return interval;
        }

        private PlayerData GetPlayer(int playerId)
        {
            if (playerId >= 0 && playerId < MaxPlayers)
                return _playersData[playerId];
            return null;
        }

        private void ResetTimeout()
        {
            _timeoutCount = 0;
            _lastActionFromServer = DateTime.Now;
        }

        private void SetTimeout()
        {
            _timeoutCount++;
            _lastActionFromServer = DateTime.Now;
        }

        private void PlaceCurrentPiece()
        {
            //Log.Default.WriteLine(LogLevels.Debug, "Place current piece {0} {1}", CurrentPiece.Value, CurrentPiece.Index);

            Board.CommitPiece(CurrentPiece);
        }

        private void EndGame()
        {
            Log.Default.WriteLine(LogLevels.Info, "End game");
            _gameTimer.Stop();

            // Reset board action list
            _actionQueue.Reset();

            //
            State = States.Registered;
            // Send player lost
            _proxy.Do(x => x.GameLost(this));

            if (_isConfusionActive)
            {
                _isConfusionActive = false;
                ContinuousEffectToggled.Do(x => x(Specials.Confusion, false, 0));
                _proxy.Do(x => x.FinishContinuousSpecial(this, Specials.Confusion));
            }

            if (_isDarknessActive)
            {
                _isDarknessActive = false;
                ContinuousEffectToggled.Do(x => x(Specials.Darkness, false, 0));
                _proxy.Do(x => x.FinishContinuousSpecial(this, Specials.Darkness));
            }

            if (_isImmunityActive)
            {
                _isImmunityActive = false;
                ContinuousEffectToggled.Do(x => x(Specials.Immunity, false, 0));
                _proxy.Do(x => x.FinishContinuousSpecial(this, Specials.Immunity));
            }

            GameOver.Do(x => x());

            _statistics.GameLost++;

            if (!IsSpectator)
            {
                int opponentsLeft = _playersData.Count(x => x != null && x.PlayerId != PlayerId && x.State == PlayerData.States.Playing);
                _achievementManager.Do(x => x.OnGameOver(_statistics.MoveCount, LinesCleared, PlayingOpponentsInCurrentGame, opponentsLeft, Inventory));
            }
        }

        private void StartRound()
        {
            // Set new current piece to next, increment piece index and create next piece
            if (_mutationCount > 0)
            {
                CurrentPiece = _factory.CreatePiece(NextPiece.Value, Board.PieceSpawnX, Board.PieceSpawnY, SpawnOrientation, NextPiece.Index + 1, true);
                _mutationCount--;
            }
            else
            {
                CurrentPiece = NextPiece;
                CurrentPiece.Move(Board.PieceSpawnX, Board.PieceSpawnY);
            }
            //
            MoveCurrentPieceToBoardTop();
            // Statistics
            if (_statistics.PieceCount.ContainsKey(CurrentPiece.Value)) // Update statistics
                _statistics.PieceCount[CurrentPiece.Value]++;
            // Get next piece
            _pieceIndex++;
            Pieces nextPiece = Pieces.TetriminoS;
            if (_pieceIndex + 1 < _pieces.Size)
                nextPiece = _pieces[_pieceIndex + 1];
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "End of PieceProvider reached, server is definitively too slow or we are too fast {0}", _pieceIndex+1);
                _statistics.EndOfPieceProviderReached++;
            }
            if (nextPiece == Pieces.Invalid)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Next piece not yet received from server, server is definitively too slow or we are too fast {0}", _pieceIndex + 1);
                _statistics.NextPieceNotYetReceived++;
            }
            NextPiece = _factory.CreatePiece(nextPiece, Board.PieceSpawnX, Board.PieceSpawnY, SpawnOrientation, _pieceIndex + 1, _mutationCount > 0);

            NextPieceModified.Do(x => x());

            // Inform UI
            Redraw.Do(x => x());

            //Log.Default.WriteLine("New piece {0} {1}  next {2}", CurrentPiece.PieceValue, _pieceIndex, NextPiece.PieceValue);
            // Check game over (if current piece has conflict with another piece)
            if (!Board.CheckNoConflict(CurrentPiece))
                EndGame();
            else
            {
                // Start round
                _gameTimer.Start();

                RoundStarted.Do(x => x());
            }
        }

        private void FinishRound()
        {
            //Log.Default.WriteLine("Round finished with piece {0} {1}  next {2}", CurrentPiece.PieceValue, _pieceIndex, NextPiece.PieceValue);
            // Reset hold
            _holdAlreadyUsed = false;

            // Stop game timer
            _gameTimer.Stop();
            // Delete rows, get specials and pieces
            List<Specials> specials;
            List<Pieces> collapsedPieces;
            int deletedRows = DeleteRows(out specials, out collapsedPieces);
            LinesCleared += deletedRows;
            if (deletedRows > 0)
            {
                LinesClearedChanged.Do(x => x(LinesCleared));
                Log.Default.WriteLine(LogLevels.Debug, "{0} lines deleted -> total {1}", deletedRows, LinesCleared);
            }

            // Statistics & Scoring
            switch (deletedRows)
            {
                case 0:
                    // NOP
                    break;
                case 1:
                    _statistics.SingleCount++;
                    Score += Level*100;
                    ScoreChanged.Do(x => x(Score));
                    break;
                case 2:
                    _statistics.DoubleCount++;
                    Score += Level * 300;
                    ScoreChanged.Do(x => x(Score));
                    break;
                case 3:
                    _statistics.TripleCount++;
                    Score += Level * 500;
                    ScoreChanged.Do(x => x(Score));
                    break;
                case 4:
                    _statistics.TetrisCount++;
                    Score += Level * 800;
                    ScoreChanged.Do(x => x(Score));
                    break;
                    // TODO: more than 4 lines ???
            }

            // Check level increase
            if (Level < LinesCleared / 10 && Level < MaxLevel)
            {
                Level = LinesCleared / 10;
                double newInterval = ComputeGameTimerInterval(Level);
                _gameTimer.Interval = newInterval;
                LevelChanged.Do(x => x(Level));
                Log.Default.WriteLine(LogLevels.Debug, "Level increased: {0}", Level);
            }

            // Add specials to inventory
            if (specials.Count > 0)
            {
                _inventory.Enqueue(specials);
                // Update statistics
                foreach (Specials special in specials)
                    _statistics.SpecialCount[special]++;
            }

            // Transform cell into special blocks
            if (deletedRows >= Options.LinesToMakeForSpecials && Options.SpecialsAddedEachTime > 0)
            {
                Board.SpawnSpecialBlocks(deletedRows*Options.SpecialsAddedEachTime, () => RangeRandom.Random(Options.SpecialOccurancies));
                //
                InventoryChanged.Do(x => x());
            }

            // Send piece places to server
            _proxy.Do(x => x.PlacePiece(this, _pieceIndex, _pieces.HighestIndex+1, CurrentPiece.Value, CurrentPiece.Orientation, CurrentPiece.PosX, CurrentPiece.PosY, Board.Cells));

            // Signal to server deleted lines
            if (deletedRows > 0)
            {
                _proxy.Do(x => x.ClearLines(this, deletedRows));
            }

            // UI is updated in StartRound

            //
            RoundFinished.Do(x => x(deletedRows));

            if (!IsSpectator)
                _achievementManager.Do(x => x.OnRoundFinished(deletedRows, Level, _statistics.MoveCount, Score, Board, collapsedPieces));

            // Start next round
            StartRound();
        }

        private int DeleteRows(out List<Specials> specials, out List<Pieces> pieces)
        {
            return Board.CollapseCompletedRows(out specials, out pieces);
        }

        private void Disconnect()
        {
            State = States.Created;
            IsServerMaster = false;
            PlayerId = -1;
            _clientSpectatorId = -1;

            if (_proxy != null)
            {
                _proxy.ConnectionLost -= OnConnectionLost;
                _proxy.Disconnect();
                _proxy = null;
            }
            if (_proxySpectator != null)
            {
                _proxySpectator.ConnectionLost -= OnConnectionLost;
                _proxySpectator.Disconnect();
                _proxySpectator = null;
            }
        }

        private void GameTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (State == States.Playing && AutomaticallyMoveDown)
            {
                MoveDown(true);
            }
        }

        private void MoveCurrentPieceToBoardTop()
        {
            // Move CurrentPiece to top of board (even if SpawnY is not top board or piece cells are not topped)
            int minX, minY, maxX, maxY;
            CurrentPiece.GetAbsoluteBoundingRectangle(out minX, out minY, out maxX, out maxY);
            if (maxY < Board.Height)
                CurrentPiece.Translate(0, Board.Height - maxY);
        }

        private void EnqueueBoardAction(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        #region IClient

        public string Name { get; private set; }

        public string Team { get; private set; }

        public bool IsSpectator { get; private set; }

        public int PlayerId { get; private set; }

        public int MaxPlayersCount => MaxPlayers;

        public IPiece CurrentPiece { get; private set; }

        public IPiece NextPiece { get; private set; }

        public IPiece HoldPiece { get; private set; }

        public IBoard Board => Player == null ? null : Player.Board;

        public bool IsGamePaused => ServerState == ServerStates.Paused;

        public bool IsGameStarted => ServerState == ServerStates.Playing;

        public bool IsPlaying => State == States.Playing;

        public bool IsRegistered => State != States.Created && State != States.Registering;

        public IReadOnlyCollection<Specials> Inventory => _inventory.Specials();

        public int LinesCleared { get; private set; }

        public int Level { get; private set; }

        public int Score { get; private set; }

        public int InventorySize => Options.InventorySize;

        public GameOptions Options { get; private set; }

        public bool IsServerMaster { get; private set; }

        public int PlayingOpponentsInCurrentGame { get; private set; }

        public Versioning Version { get; private set; }

        public IReadOnlyCollection<IOpponent> Opponents
        {
            get
            {
                return _playersData.Where(x => x != null && x.PlayerId != PlayerId && x.Board != null && x.State == PlayerData.States.Playing).ToList();
            }
        }

        public IClientStatistics Statistics => _statistics;

        public IReadOnlyCollection<IAchievement> Achievements => _achievementManager == null ? null : _achievementManager.Achievements;

        public event ClientConnectionLostEventHandler ConnectionLost;
        public event ClientRoundStartedEventHandler RoundStarted;
        public event ClientRoundFinishedEventHandler RoundFinished;
        public event ClientStartGameEventHandler GameStarted;
        public event ClientFinishGameEventHandler GameFinished;
        public event ClientGameOverEventHandler GameOver;
        public event ClientPauseGameEventHandler GamePaused;
        public event ClientResumeGameEventHandler GameResumed;
        public event ClientRedrawEventHandler Redraw;
        public event ClientRedrawBoardEventHandler RedrawBoard;
        public event ClientPieceMovingEventHandler PieceMoving;
        public event ClientPieceMovedEventHandler PieceMoved;
        public event ClientNextPieceModifiedEventHandler NextPieceModified;
        public event ClientHoldPieceModifiedEventHandler HoldPieceModified;
        public event ClientRegisteredAsPlayerEventHandler RegisteredAsPlayer;
        public event ClientPlayerUnregisteredEventHandler PlayerUnregistered;
        public event ClientWinListModifiedEventHandler WinListModified;
        public event ClientServerMasterModifiedEventHandler ServerMasterModified;
        public event ClientPlayerLostEventHandler PlayerLost;
        public event ClientPlayerWonEventHandler PlayerWon;
        public event ClientPlayerJoinedEventHandler PlayerJoined;
        public event ClientPlayerLeftEventHandler PlayerLeft;
        public event ClientPlayerTeamChangedEventHandler PlayerTeamChanged;
        public event ClientPlayerPublishMessageEventHandler PlayerPublishMessage;
        public event ClientServerPublishMessageEventHandler ServerPublishMessage;
        public event ClientInventoryChangedEventHandler InventoryChanged;
        public event ClientLinesClearedChangedEventHandler LinesClearedChanged;
        public event ClientLevelChangedEventHandler LevelChanged;
        public event ClientScoreChangedEventHandler ScoreChanged;
        public event ClientSpecialUsedEventHandler SpecialUsed;
        public event ClientUseSpecialEventHandler UseSpecial;
        public event ClientPlayerAddLinesEventHandler PlayerAddLines;
        public event ClientContinuousSpecialToggledEventHandler ContinuousEffectToggled;
        public event ClientContinuousSpecialFinishedEventHandler ContinuousSpecialFinished;
        public event ClientAchievementEarnedEventHandler AchievementEarned;
        public event ClientPlayerAchievementEarnedEventHandler PlayerAchievementEarned;
        public event ClientOptionsChangedEventHandler OptionsChanged;
        public event ClientRegisteredAsSpectatorEventHandler RegisteredAsSpectator;
        public event ClientSpectatorJoinedEventHandler SpectatorJoined;
        public event ClientSpectatorLeftEventHandler SpectatorLeft;

        public void Dump()
        {
            // Players
            for (int i = 0; i < MaxPlayers; i++)
            {
                PlayerData p = _playersData[i];
                Log.Default.WriteLine(LogLevels.Info, "{0}{1}: {2}", i, i == PlayerId ? "*" : String.Empty, p == null ? String.Empty : p.Name);
            }
            // Inventory
            IReadOnlyCollection<Specials> specials = Inventory;
            StringBuilder sb2 = new StringBuilder();
            foreach (Specials special in specials)
                sb2.Append(ConvertSpecial(special));
            Log.Default.WriteLine(LogLevels.Info, sb2.ToString());
            // Board
            if (PlayerId >= 0 && State == States.Playing)
            {
                for (int y = Board.Height; y >= 1; y--)
                {
                    StringBuilder sb = new StringBuilder("|");
                    for (int x = 1; x <= Board.Width; x++)
                    {
                        byte cellValue = Board[x, y];
                        if (cellValue == CellHelper.EmptyCell)
                            sb.Append(" ");
                        else
                        {
                            Pieces cellPiece = CellHelper.GetColor(cellValue);
                            Specials cellSpecial = CellHelper.GetSpecial(cellValue);
                            if (cellSpecial == Specials.Invalid)
                                sb.Append((int) cellPiece);
                            else
                                sb.Append(ConvertSpecial(cellSpecial));
                        }
                    }
                    sb.Append("|");
                    Log.Default.WriteLine(LogLevels.Info, sb.ToString());
                }
                Log.Default.WriteLine(LogLevels.Info, "".PadLeft(Board.Width + 2, '-'));
            }
            // TODO: current & next piece
        }

        public void SetVersion(int major, int minor)
        {
            Version = new Versioning
                {
                    Major = major,
                    Minor = minor
                };
        }

        public bool ConnectAndRegisterAsPlayer(string address, string name, string team)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (Version == null)
                throw new InvalidOperationException("Cannot ConnectAndRegisterAsPlayer, no application version found");

            if (_proxy != null || _proxySpectator != null)
                return false; // should disconnect first

            try
            {
                _proxy = _factory.CreatePlayerProxy(this, address);
                _proxy.ConnectionLost += OnConnectionLost;

                State = States.Registering;
                Name = name;
                Team = team;

                _proxy.RegisterPlayer(this, Version, name, team);

                return true;
            }
            catch(Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Problem in ConnectAndRegisterAsPlayer. Exception:{0}", ex.ToString());
                return false;
            }
        }

        public bool ConnectAndRegisterAsSpectator(string address, string name)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (Version == null)
                throw new InvalidOperationException("Cannot ConnectAndRegisterAsSpectator, no application version found");

            if (_proxy != null || _proxySpectator != null)
                return false; // should disconnect first

            try
            {
                _proxySpectator = _factory.CreateSpectatorProxy(this, address);
                _proxySpectator.ConnectionLost += OnConnectionLost;

                State = States.Registering;
                Name = name;
                _proxySpectator.RegisterSpectator(this, Version, name);
               
                return true;
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Problem in ConnectAndRegisterAsSpectator. Exception:{0}", ex.ToString());
                return false;
            }
        }
        
        public bool UnregisterAndDisconnect()
        {
            bool wasSpectator = IsSpectator;
            State = States.Created;
            IsServerMaster = false;
            PlayerId = -1;
            _clientSpectatorId = -1;
            IsSpectator = false;

            _proxy.Do(x => x.UnregisterPlayer(this));
            _proxySpectator.Do(x => x.UnregisterSpectator(this));

            PlayerUnregistered.Do(x => x());

            Disconnect();

            return true;
        }

        public void StartGame()
        {
            if (State == States.Registered && ServerState == ServerStates.Waiting)
                _proxy.Do(x => x.StartGame(this));
        }

        public void StopGame()
        {
            if (ServerState == ServerStates.Playing || ServerState == ServerStates.Paused)
                _proxy.Do(x => x.StopGame(this));
        }

        public void PauseGame()
        {
            if (ServerState == ServerStates.Playing)
                _proxy.Do(x => x.PauseGame(this));
        }

        public void ResumeGame()
        {
            if (ServerState == ServerStates.Paused)
                _proxy.Do(x => x.ResumeGame(this));
        }

        public void ResetWinList()
        {
            if (State == States.Registered)
                _proxy.Do(x => x.ResetWinList(this));
        }

        public void ChangeTeam(string team)
        {
            Team = team;
            if (State == States.Registered)
                _proxy.Do(x => x.PlayerTeam(this, team));
        }

        public void ChangeOptions(GameOptions options)
        {
            if (State == States.Registered)
                _proxy.Do(x => x.ChangeOptions(this, options));
        }

        public void KickPlayer(int playerId)
        {
            if (State == States.Registered)
                _proxy.Do(x => x.KickPlayer(this, playerId));
        }

        public void BanPlayer(int playerId)
        {
            if (State == States.Registered)
                _proxy.Do(x => x.BanPlayer(this, playerId));
        }

        public void PublishMessage(string msg)
        {
            if (State == States.Registered || State == States.Playing)
            {
                if (IsSpectator)
                    _proxySpectator.Do(x => x.PublishSpectatorMessage(this, msg));
                else
                    _proxy.Do(x => x.PublishMessage(this, msg));
            }
        }

        public void Hold()
        {
            if (State != States.Playing)
                return;

            if (_holdAlreadyUsed)
                return;

            EnqueueBoardAction(HoldAction);
        }

        public void Drop()
        {
            //Log.Default.WriteLine(LogLevels.Debug, "DROP {0}", CurrentPiece.Value);

            if (State != States.Playing)
                return;

            //Log.Default.WriteLine(LogLevels.Debug, "ENQUEUE DROP {0} {1}", CurrentPiece.Value, CurrentPiece.Index);
            EnqueueBoardAction(DropAction);
            _statistics.MoveCount++;
        }

        public void MoveDown(bool automatic = false)
        {
            //Log.Default.WriteLine(LogLevels.Debug, "MoveDown");

            if (State != States.Playing)
                return;
            //Log.Default.WriteLine(LogLevels.Debug, "ENQUEUE DOWN {0} {1}", CurrentPiece.Value, CurrentPiece.Index);
            EnqueueBoardAction(MoveDownAction);
            if (!automatic) 
                _statistics.MoveCount++;
        }

        public void MoveLeft()
        {
            if (State != States.Playing)
                return;
            EnqueueBoardAction(MoveLeftAction);
            _statistics.MoveCount++;
        }

        public void MoveRight()
        {
            if (State != States.Playing)
                return;
            EnqueueBoardAction(MoveRightAction);
            _statistics.MoveCount++;
        }

        public void RotateClockwise()
        {
            if (State != States.Playing)
                return;
            EnqueueBoardAction(RotateClockwiseAction);
            _statistics.MoveCount++;
        }

        public void RotateCounterClockwise()
        {
            if (State != States.Playing)
                return;
            EnqueueBoardAction(RotateCounterClockwiseAction);
            _statistics.MoveCount++;
        }

        public void DiscardFirstSpecial()
        {
            if (State != States.Playing)
                return;
            // Only if client is playing
            if (Player.State == PlayerData.States.Playing)
            {
                // Get first special and discard it
                Specials special;
                if (_inventory.Dequeue(out special))
                {
                    // Update statistics
                    _statistics.SpecialDiscarded[special]++;

                    //
                    InventoryChanged.Do(x => x());
                }
            }
        }

        public bool UseFirstSpecial(int targetId)
        {
            if (State != States.Playing)
                return false;
            bool succeeded = false;
            // Only if client is playing, target exists and is playing
            PlayerData target = GetPlayer(targetId);
            if (Player.State == PlayerData.States.Playing && target != null && target.State == PlayerData.States.Playing)
            {
                if (target.IsImmune || (targetId == PlayerId && _isImmunityActive)) // Cannot target a player with immunity
                    return false;

                // Get first special and send it
                Specials special;
                if (_inventory.Dequeue(out special))
                {
                    // Update statistics
                    _statistics.SpecialUsed[special]++;
                    //
                    _proxy.Do(x => x.UseSpecial(this, targetId, special));
                    succeeded = true;

                    //
                    InventoryChanged.Do(x => x());

                    //
                    UseSpecial.Do(x => x(targetId, target.Name, special));

                    //
                    if (!IsSpectator)
                        _achievementManager.Do(x => x.OnUseSpecial(PlayerId, Team, Board, targetId, target.Team, target.Board, special));
                }
            }
            return succeeded;
        }

        public void ResetAchievements()
        {
            _achievementManager.Do(x => x.Reset());
        }

        #endregion

        private static char ConvertSpecial(Specials special)
        {
            SpecialAttribute attribute = EnumHelper.GetAttribute<SpecialAttribute>(special);
            return attribute == null ? '?' : attribute.ShortName;
        }

        private void TimeoutTask()
        {
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Log.Default.WriteLine(LogLevels.Info, "Stop background task event raised");
                    break;
                }

                if (State == States.Registered || State == States.Playing || State == States.Paused)
                {
                    // Check server timeout
                    TimeSpan timespan = DateTime.Now - _lastActionFromServer;
                    if (timespan.TotalMilliseconds > TimeoutDelay && IsTimeoutDetectionActive)
                    {
                        Log.Default.WriteLine(LogLevels.Debug, "Timeout++");
                        // Update timeout count
                        SetTimeout();
                        if (_timeoutCount >= MaxTimeoutCountBeforeDisconnection)
                            OnConnectionLost(); // timeout
                    }

                    // Send heartbeat if needed
                    if (_proxy != null)
                    {
                        TimeSpan delaySinceLastActionToServer = DateTime.Now - _proxy.LastActionToServer;
                        if (delaySinceLastActionToServer.TotalMilliseconds > HeartbeatDelay)
                            _proxy.Heartbeat(this);
                    }
                    else if (_proxySpectator != null)
                    {
                        TimeSpan delaySinceLastActionToServer = DateTime.Now - _proxySpectator.LastActionToServer;
                        if (delaySinceLastActionToServer.TotalMilliseconds > HeartbeatDelay)
                            _proxySpectator.HeartbeatSpectator(this);
                    }
                }

                if (_proxy != null && _isConfusionActive)
                {
                    if (DateTime.Now > _confusionEndTime)
                    {
                        _isConfusionActive = false;
                        ContinuousEffectToggled.Do(x => x(Specials.Confusion, false, 0));
                        _proxy.FinishContinuousSpecial(this, Specials.Confusion);
                    }
                }

                if (_proxy != null && _isDarknessActive)
                {
                    if (DateTime.Now > _darknessEndTime)
                    {
                        _isDarknessActive = false;
                        ContinuousEffectToggled.Do(x => x(Specials.Darkness, false, 0));
                        _proxy.FinishContinuousSpecial(this, Specials.Darkness);
                    }
                }

                if (_proxy != null && _isImmunityActive)
                {
                    if (DateTime.Now > _immunityEndTime)
                    {
                        _isImmunityActive = false;
                        ContinuousEffectToggled.Do(x => x(Specials.Immunity, false, 0));
                        _proxy.FinishContinuousSpecial(this, Specials.Immunity);
                    }
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

        #region Board Actions

        private void SpecialUsedAction(Specials special)
        {
            switch (special)
            {
                case Specials.AddLines:
                    AddLines(1);
                    break;
                case Specials.ClearLines:
                    ClearLine();
                    break;
                case Specials.NukeField:
                    NukeField();
                    break;
                case Specials.RandomBlocksClear:
                    RandomBlocksClear(10);
                    break;
                case Specials.SwitchFields:
                    // NOP: Handled by Server
                    break;
                case Specials.ClearSpecialBlocks:
                    ClearSpecialBlocks();
                    break;
                case Specials.BlockGravity:
                    BlockGravity();
                    break;
                case Specials.BlockQuake:
                    BlockQuake();
                    break;
                case Specials.BlockBomb:
                    BlockBomb();
                    break;
                case Specials.ClearColumn:
                    ClearColumn();
                    break;
                case Specials.Immunity:
                    Immunity(15);
                    break;
                case Specials.Darkness:
                    Darkness(10);
                    break;
                case Specials.Confusion:
                    Confusion(10);
                    break;
                case Specials.Mutation:
                    Mutation(3);
                    break;
                case Specials.ZebraField:
                    ZebraField();
                    break;
                case Specials.LeftGravity:
                    LeftGravity();
                    break;
            }
        }

        private void HoldAction()
        {
            if (_holdAlreadyUsed)
                return;

            //
            _gameTimer.Stop();

            // Hide current piece
            PieceMoving.Do(x => x());

            // If Hold doesn't exist, set Hold to Current, set Current to Next and set Next to Next's next
            // Else, swap Hold and Current
            if (HoldPiece == null)
            {
                HoldPiece = CurrentPiece;
                CurrentPiece = NextPiece;

                _pieceIndex++;
                Pieces nextPiece = Pieces.TetriminoS;
                if (_pieceIndex + 1 < _pieces.Size)
                    nextPiece = _pieces[_pieceIndex + 1];
                else
                {
                    Log.Default.WriteLine(LogLevels.Warning, "End of PieceProvider reached, server is definitively too slow or we are too fast {0}", _pieceIndex + 1);
                    _statistics.EndOfPieceProviderReached++;
                }
                if (nextPiece == Pieces.Invalid)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Next piece not yet received from server, server is definitively too slow or we are too fast {0}", _pieceIndex + 1);
                    _statistics.NextPieceNotYetReceived++;
                }
                NextPiece = _factory.CreatePiece(nextPiece, Board.PieceSpawnX, Board.PieceSpawnY, SpawnOrientation, _pieceIndex + 1, _mutationCount > 1);

                NextPieceModified.Do(x => x());
            }
            else
            {
                IPiece swap = CurrentPiece;
                CurrentPiece = HoldPiece;
                HoldPiece = swap;
            }

            // Move Current to Spawn location
            CurrentPiece.Move(Board.PieceSpawnX, Board.PieceSpawnY);
            // Move to Board top
            MoveCurrentPieceToBoardTop();

            HoldPieceModified.Do(x => x());

            // Display current piece
            PieceMoved.Do(x => x());

            //
            _gameTimer.Start();

            //
            _holdAlreadyUsed = true;
        }

        private void DropAction()
        {
            //Log.Default.WriteLine(LogLevels.Debug, "DROP {0} {1}", CurrentPiece.Value, CurrentPiece.Index);
            _gameTimer.Stop();

            // Inform UI
            PieceMoving.Do(x => x());
            // Perform move
            Board.Drop(CurrentPiece);
            // Inform UI
            PieceMoved.Do(x => x());

            //
            PlaceCurrentPiece();
            //
            FinishRound();
        }

        private void MoveDownAction()
        {
            //Log.Default.WriteLine(LogLevels.Debug, "DOWN {0} {1}", CurrentPiece.Value, CurrentPiece.Index);

            // Inform UI
            PieceMoving.Do(x => x());
            // Perform move
            bool movedDown = Board.MoveDown(CurrentPiece);
            // Inform UI
            PieceMoved.Do(x => x());

            // If cannot move down anymore, round is finished
            if (!movedDown)
            {
                //
                PlaceCurrentPiece();
                //
                FinishRound();
            }
        }

        private void MoveLeftAction()
        {
            // Inform UI
            PieceMoving.Do(x => x());
            // Perform move
            Board.MoveLeft(CurrentPiece);
            // Inform UI
            PieceMoved.Do(x => x());
        }

        private void MoveRightAction()
        {
            // Inform UI
            PieceMoving.Do(x => x());
            // Perform move
            Board.MoveRight(CurrentPiece);
            // Inform UI
            PieceMoved.Do(x => x());
        }

        private void RotateClockwiseAction()
        {
            // Inform UI
            PieceMoving.Do(x => x());
            // Perform move
            Board.RotateClockwise(CurrentPiece);
            // Inform UI
            PieceMoved.Do(x => x());
        }

        private void RotateCounterClockwiseAction()
        {
            // Inform UI
            PieceMoving.Do(x => x());
            // Perform move
            Board.RotateCounterClockwise(CurrentPiece);
            // Inform UI
            PieceMoved.Do(x => x());
        }

        private void ModifyGridAction(byte[] cells)
        {
            Player.Board.SetCells(cells);
            Board.RemoveCellsHigherThan(16);
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        #region Specials

        private void AddLines(int count)
        {
            if (count <= 0)
                return;
            Board.AddLines(count, () => RangeRandom.Random(Options.PieceOccurancies));
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        private void ClearLine()
        {
            Board.ClearLine();
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        private void NukeField()
        {
            Board.NukeField();
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        private void RandomBlocksClear(int count)
        {
            Board.RandomBlocksClear(count);
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        private void ClearSpecialBlocks()
        {
            Board.ClearSpecialBlocks(() => RangeRandom.Random(Options.PieceOccurancies));
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        private void BlockGravity()
        {
            Board.BlockGravity();
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        private void BlockQuake()
        {
            Board.BlockQuake();
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        private void BlockBomb()
        {
            Board.BlockBomb();
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        private void ClearColumn()
        {
            Board.ClearColumn();
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        private void Mutation(int count)
        {
            _mutationCount += count;
        }

        private void Darkness(int delayInSeconds)
        {
            // If already active, add delay to remaining
            if (_isDarknessActive)
                _darknessEndTime = _darknessEndTime.AddSeconds(delayInSeconds);
            // Else, raise event
            else
            {
                _isDarknessActive = true;
                _darknessEndTime = DateTime.Now.AddSeconds(delayInSeconds);
            }
            ContinuousEffectToggled.Do(x => x(Specials.Darkness, true, (_darknessEndTime - DateTime.Now).TotalSeconds));
        }

        private void Confusion(int delayInSeconds)
        {
            // If already active, add delay to remaining
            if (_isConfusionActive)
                _confusionEndTime = _confusionEndTime.AddSeconds(delayInSeconds);
            // Else, raise event
            else
            {
                _isConfusionActive = true;
                _confusionEndTime = DateTime.Now.AddSeconds(delayInSeconds);
            }
            ContinuousEffectToggled.Do(x => x(Specials.Confusion, true, (_confusionEndTime - DateTime.Now).TotalSeconds));
        }

        private void Immunity(int delayInSeconds)
        {
            // If already active, add delay to remaining
            if (_isImmunityActive)
                _immunityEndTime = _immunityEndTime.AddSeconds(delayInSeconds); // should never happen
            // Else, raise event
            else
            {
                _isImmunityActive = true;
                _immunityEndTime = DateTime.Now.AddSeconds(delayInSeconds);
            }
            ContinuousEffectToggled.Do(x => x(Specials.Immunity, true, (_immunityEndTime - DateTime.Now).TotalSeconds));
        }

        private void ZebraField()
        {
            Board.ZebraField();
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        private void LeftGravity()
        {
            Board.LeftGravity();
            _proxy.Do(x => x.ModifyGrid(this, Player.Board.Cells));

            Redraw.Do(x => x());
        }

        #endregion

        #endregion
    }
}
