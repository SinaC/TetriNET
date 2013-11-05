using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Attributes;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Logger;
using TetriNET.Common.Randomizer;

namespace TetriNET.Client
{
    public sealed class Client : ITetriNETCallback, IClient
    {
        private const int MaxPlayers = 6;
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

        private readonly Func<Pieces, int, int, int, int, bool, IPiece> _createPieceFunc;
        private readonly Func<IBoard> _createBoardFunc;
        private readonly PlayerData[] _playersData = new PlayerData[MaxPlayers];
        private readonly ManualResetEvent _stopBackgroundTaskEvent = new ManualResetEvent(false);
        private readonly PieceArray _pieces;
        private readonly Inventory _inventory;
        private readonly Statistics _statistics;
        private readonly IAchievementManager _achievementManager;
        private readonly System.Timers.Timer _gameTimer;

        public States State { get; private set; }
        public ServerStates ServerState { get; private set; }

        private IProxy _proxy;
        private int _clientPlayerId;
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

        private PlayerData Player
        {
            get { return _clientPlayerId >= 0 && _clientPlayerId <= MaxPlayers ? _playersData[_clientPlayerId] : null; }
        }

        public Client(Func<Pieces, int, int, int, int, bool, IPiece> createPieceFunc, Func<IBoard> createBoardFunc)
        {
            if (createPieceFunc == null)
                throw new ArgumentNullException("createPieceFunc");
            if (createBoardFunc == null)
                throw new ArgumentNullException("createBoardFunc");

            _createPieceFunc = createPieceFunc;
            _createBoardFunc = createBoardFunc;

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

            _clientPlayerId = -1;
           
            State = States.Created;
            ServerState = ServerStates.Waiting;
            IsServerMaster = false;
            _isDarknessActive = false;
            _isConfusionActive = false;
            _isImmunityActive = false;
            _pauseStartTime = DateTime.Now;
            _mutationCount = 0;

            Task.Factory.StartNew(TimeoutTask);
            Task.Factory.StartNew(BoardActionTask);
        }

        public Client(Func<Pieces, int, int, int, int, bool, IPiece> createPieceFunc, Func<IBoard> createBoardFunc, Func<IAchievementManager> createAchievementManagerFunc)
            : this(createPieceFunc, createBoardFunc)
        {
            if (createAchievementManagerFunc == null)
                throw new ArgumentNullException("createAchievementManagerFunc");

            _achievementManager = createAchievementManagerFunc();
            _achievementManager.Achieved += AchievementEarned;
        }

        #region IAchievementManager event handler

        private void AchievementEarned(IAchievement achievement, bool firstTime)
        {
            if (firstTime)
            {
                _proxy.EarnAchievement(this, achievement.Id, achievement.Title);
                //string msg = String.Format("You have earned [{0}]", achievement.Title);
                //ClientOnServerPublishMessage(msg);
            }

            if (ClientOnAchievementEarned != null)
                ClientOnAchievementEarned(achievement, firstTime);
        }
        
        #endregion

        #region IProxy event handler

        private void ConnectionLostHandler()
        {
            States previousState = State;
            State = States.Created;
            IsServerMaster = false;
            _clientPlayerId = -1;

            Disconnect();

            if (ClientOnConnectionLost != null)
            {
                if (previousState == States.Registering)
                    ClientOnConnectionLost(ConnectionLostReasons.ServerNotFound);
                else
                    ClientOnConnectionLost(ConnectionLostReasons.Other);
            }
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
            ConnectionLostHandler();
        }

        public void OnPlayerRegistered(RegistrationResults result, int playerId, bool isGameStarted, bool isServerMaster, GameOptions options)
        {
            ResetTimeout();
            if (result == RegistrationResults.RegistrationSuccessful && State == States.Registering)
            {
                Log.WriteLine(Log.LogLevels.Info, "Registered as player {0} game started {1}", playerId, isGameStarted);

                if (playerId >= 0 && playerId < MaxPlayers)
                {
                    _clientPlayerId = playerId;
                    PlayerData player = new PlayerData
                    {
                        Name = Name,
                        PlayerId = playerId,
                        Board = _createBoardFunc(),
                        State = PlayerData.States.Joined
                    };
                    _playersData[_clientPlayerId] = player;

                    State = States.Registered;
                    ServerState = isGameStarted ? ServerStates.Playing : ServerStates.Waiting;// TODO: handle server paused
                    IsServerMaster = isServerMaster;
                    Options = options;

                    if (ClientOnPlayerRegistered != null)
                        ClientOnPlayerRegistered(RegistrationResults.RegistrationSuccessful, playerId, isServerMaster);

                    if (isGameStarted)
                    {
                        player.Board.FillWithRandomCells(() => RangeRandom.Random(Options.PieceOccurancies));

                        if (ClientOnRedraw != null)
                            ClientOnRedraw();
                    }
                }
                else
                {
                    State = States.Created;
                    IsServerMaster = false;
                    Log.WriteLine(Log.LogLevels.Warning, "Wrong id {0}", playerId);

                    if (ClientOnPlayerRegistered != null)
                        ClientOnPlayerRegistered(RegistrationResults.RegistrationFailedInvalidId, -1, false);
                }
            }
            else
            {
                State = States.Created;
                IsServerMaster = false;

                if (ClientOnPlayerRegistered != null)
                    ClientOnPlayerRegistered(result, -1, false);

                Log.WriteLine(Log.LogLevels.Info, "Registration failed {0}", result);
            }
        }

        public void OnPlayerJoined(int playerId, string name)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Player {0}[{1}] joined", name, playerId);

            ResetTimeout();
            // Don't update ourself
            if (playerId != _clientPlayerId && playerId >= 0 && playerId < MaxPlayers)
            {
                PlayerData playerData = new PlayerData
                {
                    Name = name,
                    PlayerId = playerId,
                    Board = _createBoardFunc(),
                    IsImmune = false,
                    State = PlayerData.States.Joined
                };
                _playersData[playerId] = playerData;

                if (ClientOnPlayerJoined != null)
                    ClientOnPlayerJoined(playerId, name);

                if (IsGameStarted)
                {
                    playerData.Board.FillWithRandomCells(() => RangeRandom.Random(Options.PieceOccurancies));

                    if (ClientOnRedrawBoard != null)
                        ClientOnRedrawBoard(playerId, playerData.Board);
                }
            }
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Player {0}[{1}] left ({2})", name, playerId, reason);

            ResetTimeout();
            if (playerId != _clientPlayerId && playerId >= 0 && playerId < MaxPlayers)
            {
                PlayerData player = _playersData[playerId];
                if (player != null)
                {
                    player.Board.Clear();
                    if (ClientOnRedrawBoard != null)
                        ClientOnRedrawBoard(playerId, player.Board);
                }

                _playersData[playerId] = null;

                if (ClientOnPlayerLeft != null)
                    ClientOnPlayerLeft(playerId, name, reason);
            }
        }

        public void OnPlayerTeamChanged(int playerId, string team)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Player [{0}] team ({1})", playerId, team);

            ResetTimeout();

            if (playerId >= 0 && playerId < MaxPlayers)
            {
                PlayerData player = _playersData[playerId];
                if (player != null)
                {
                    player.Team = team;

                    if (ClientOnPlayerTeamChanged != null)
                        ClientOnPlayerTeamChanged(playerId, team);
                }
            }
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            Log.WriteLine(Log.LogLevels.Debug, "{0}:{1}", playerName, msg);

            ResetTimeout();

            if (ClientOnPlayerPublishMessage != null)
                ClientOnPlayerPublishMessage(playerName, msg);
        }

        public void OnPublishServerMessage(string msg)
        {
            Log.WriteLine(Log.LogLevels.Debug, "{0}", msg);

            ResetTimeout();

            if (ClientOnServerPublishMessage != null)
                ClientOnServerPublishMessage(msg);
        }

        public void OnPlayerLost(int playerId)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Player [{0}] has lost", playerId);

            ResetTimeout();
            if (playerId != _clientPlayerId)
            {
                PlayerData playerData = GetPlayer(playerId);
                if (playerData != null && playerData.State == PlayerData.States.Playing)
                {
                    playerData.State = PlayerData.States.Lost;
                    playerData.IsImmune = false;
                    playerData.Board.FillWithRandomCells(() => RangeRandom.Random(Options.PieceOccurancies));

                    if (ClientOnRedrawBoard != null)
                        ClientOnRedrawBoard(playerId, playerData.Board);

                    if (ClientOnPlayerLost != null)
                        ClientOnPlayerLost(playerId, playerData.Name);
                }
            }
        }

        public void OnPlayerWon(int playerId)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Player [{0}] has won", playerId);

            ResetTimeout();

            PlayerData playerData = _playersData[playerId];
            if (playerData != null)
            {
                playerData.State = PlayerData.States.Joined;
                if (ClientOnPlayerWon != null)
                    ClientOnPlayerWon(playerId, playerData.Name);

                if (playerId == _clientPlayerId)
                    _statistics.GameWon++;

                if (_achievementManager != null && playerId == _clientPlayerId)
                    _achievementManager.OnGameWon(_statistics.MoveCount, LinesCleared, PlayingOpponentsInCurrentGame);
            }
        }

        public void OnGameStarted(List<Pieces> pieces, GameOptions options)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Game started with {0}", pieces.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i));

            ResetTimeout();

            // Reset board action list
            //Action outAction;
            //while (!_boardActionQueue.IsEmpty)
            //    _boardActionQueue.TryDequeue(out outAction);
            while (_boardActionBlockingCollection.Count > 0)
            {
                Action item;
                _boardActionBlockingCollection.TryTake(out item);
            }

            // Set state
            State = States.Playing;
            ServerState = ServerStates.Playing;
            // Reset statistics
            _statistics.Reset();
            // Reset options
            Options = options;
            // Reset pieces
            _pieces.Reset();
            for (int i = 0; i < pieces.Count; i++)
                _pieces[i] = pieces[i];
            _pieceIndex = 0;
            CurrentPiece = _createPieceFunc(_pieces[0], Board.PieceSpawnX, Board.PieceSpawnY, SpawnOrientation, 0, false);
            //MoveDownUntilTotallyInBoard(CurrentPiece);
            NextPiece = _createPieceFunc(_pieces[1], Board.PieceSpawnX, Board.PieceSpawnY, SpawnOrientation, 1, false);
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
            _proxy.ModifyGrid(this, Player.Board.Cells);
            // Restart timer
            _gameTimer.Start();

            if (ClientOnGameStarted != null)
                ClientOnGameStarted();

            if (_achievementManager != null)
                _achievementManager.OnGameStarted(Options);

            //Log.WriteLine("PIECES:{0}", _pieces.Dump(8));
        }

        public void OnGameFinished()
        {
            Log.WriteLine(Log.LogLevels.Debug, "Game finished");

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
            //Action outAction;
            //while (!_boardActionQueue.IsEmpty)
            //    _boardActionQueue.TryDequeue(out outAction);
            while (_boardActionBlockingCollection.Count > 0)
            {
                Action item;
                _boardActionBlockingCollection.TryTake(out item);
            }

            if (ClientOnGameFinished != null)
                ClientOnGameFinished();

            if (_achievementManager != null)
                _achievementManager.OnGameFinished();
        }

        public void OnGamePaused()
        {
            Log.WriteLine(Log.LogLevels.Debug, "Game paused");

            ResetTimeout();

            ServerState = ServerStates.Paused;
            _pauseStartTime = DateTime.Now;

            if (State == States.Playing)
                State = States.Paused;

            if (ClientOnGamePaused != null)
                ClientOnGamePaused();
        }

        public void OnGameResumed()
        {
            Log.WriteLine(Log.LogLevels.Debug, "Game resumed");

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

            if (ClientOnGameResumed != null)
                ClientOnGameResumed();
        }

        public void OnServerAddLines(int lineCount)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Server add {0} lines", lineCount);

            ResetTimeout();

            if (State == States.Playing)
            {
                EnqueueBoardAction(() => AddLines(lineCount));
            }
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Player {0} add {1} lines (special [{2}])", playerId, lineCount, specialId);

            ResetTimeout();

            //if (State == States.Playing)
            //{
                // Don't add line to ourself
                if (playerId != _clientPlayerId && State == States.Playing)
                {
                    EnqueueBoardAction(() => AddLines(lineCount));
                }

                if (ClientOnPlayerAddLines != null)
                {
                    PlayerData playerData = GetPlayer(playerId);
                    string playerName = playerData == null ? "???" : playerData.Name;

                    ClientOnPlayerAddLines(playerId, playerName, specialId, lineCount);
                }
            //}
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Special {0}[{1}] from {2} to {3}", special, specialId, playerId, targetId);

            ResetTimeout();
            //if (State == States.Playing)
            //{
            if (targetId == _clientPlayerId && State == States.Playing)
            {
                EnqueueBoardAction(() => SpecialUsed(special));
            }

            if (targetId != _clientPlayerId && special == Specials.Immunity)
            {
                PlayerData target = GetPlayer(targetId);
                if (target != null && target.State == PlayerData.States.Playing)
                    target.IsImmune = true;
            }

            // Messages are displayed immediately, only board action is enqueued
            if (ClientOnSpecialUsed != null)
            {
                PlayerData playerData = GetPlayer(playerId);
                PlayerData targetData = GetPlayer(targetId);

                string playerName = playerData == null ? "???" : playerData.Name;
                string targetName = targetData == null ? "???" : targetData.Name;

                ClientOnSpecialUsed(playerId, playerName, targetId, targetName, specialId, special);

                //
                if (_achievementManager != null)
                    _achievementManager.OnSpecialUsed(_clientPlayerId, playerId, playerData == null ? null : playerData.Team, playerData == null ? null : playerData.Board, targetId, targetData == null ? null : targetData.Team, targetData == null ? null : targetData.Board, special);
            }

            //}
        }

        public void OnNextPiece(int index, List<Pieces> pieces)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Next piece: {0} {1}", index, pieces.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i));

            ResetTimeout();

            if (State == States.Playing)
            {
                //_pieces[index] = piece;
                for (int i = 0; i < pieces.Count; i++)
                    _pieces[index + i] = pieces[i];
            }
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Player [{0}] modified", playerId);

            ResetTimeout();

            //if (State == States.Playing)
            //{
                PlayerData playerData = GetPlayer(playerId);
                if (playerData != null)
                {
                    // if modifying own grid, special Switch occured -> remove lines above 16
                    if (playerId == _clientPlayerId)
                    {
                        EnqueueBoardAction(() => ModifyGrid(grid));
                    }
                    else
                    {
                        playerData.Board.SetCells(grid);
                        if (ClientOnRedrawBoard != null)
                            ClientOnRedrawBoard(playerId, playerData.Board);
                    }
                }
            //}
        }

        public void OnServerMasterChanged(int playerId)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Server master changed: [{0}]", playerId);

            ResetTimeout();

            IsServerMaster = playerId == _clientPlayerId;

            if (ClientOnServerMasterModified != null)
                ClientOnServerMasterModified(playerId);
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Win list: {0}", winList.Any() ? winList.Select(x => String.Format("{0}[{1}]:{2}", x.PlayerName, x.Team, x.Score)).Aggregate((n, i) => n + "|" + i) : "");

            ResetTimeout();

            if (ClientOnWinListModified != null)
                ClientOnWinListModified(winList);
        }

        public void OnContinuousSpecialFinished(int playerId, Specials special)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Continous special {0} finished on {1}", special, playerId);

            ResetTimeout();
            if (playerId != _clientPlayerId)
            {
                if (special == Specials.Immunity)
                {
                    PlayerData target = GetPlayer(playerId);
                    if (target != null)
                        target.IsImmune = false;
                }
                if (ClientOnContinuousSpecialFinished != null)
                    ClientOnContinuousSpecialFinished(playerId, special);
            }
        }

        public void OnAchievementEarned(int playerId, int achievementId, string achievementTitle)
        {
            Log.WriteLine(Log.LogLevels.Debug, "Achievement {0}|{1} earned by {2}", playerId, achievementId, achievementTitle);

            ResetTimeout();
            if (playerId != _clientPlayerId)
            {
                //if (ClientOnServerPublishMessage != null)
                //{
                //    PlayerData player = GetPlayer(playerId);
                //    if (player != null)
                //    {
                //        string msg = String.Format("{0} has earned [{1}]", player.Name, achievementTitle);
                //        ClientOnServerPublishMessage(msg);
                //    }
                //}
                if (ClientOnPlayerAchievementEarned != null)
                {
                    PlayerData player = GetPlayer(playerId);
                    ClientOnPlayerAchievementEarned(playerId, player == null ? "???" : player.Name, achievementId, achievementTitle);
                }
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
            //Log.WriteLine(Log.LogLevels.Debug, "Place current piece {0} {1}", CurrentPiece.Value, CurrentPiece.Index);

            Board.CommitPiece(CurrentPiece);
        }

        private void EndGame()
        {
            Log.WriteLine(Log.LogLevels.Info, "End game");
            _gameTimer.Stop();

            // Reset board action list
            //Action outAction;
            //while (!_boardActionQueue.IsEmpty)
            //    _boardActionQueue.TryDequeue(out outAction);
            while (_boardActionBlockingCollection.Count > 0)
            {
                Action item;
                _boardActionBlockingCollection.TryTake(out item);
            }

            //
            State = States.Registered;
            // Send player lost
            _proxy.GameLost(this);

            if (_isConfusionActive)
            {
                _isConfusionActive = false;
                if (ClientOnContinuousEffectToggled != null)
                    ClientOnContinuousEffectToggled(Specials.Confusion, false, 0);
                _proxy.FinishContinuousSpecial(this, Specials.Confusion);
            }

            if (_isDarknessActive)
            {
                _isDarknessActive = false;
                if (ClientOnContinuousEffectToggled != null)
                    ClientOnContinuousEffectToggled(Specials.Darkness, false, 0);
                    _proxy.FinishContinuousSpecial(this, Specials.Darkness);
            }

            if (_isImmunityActive)
            {
                _isImmunityActive = false;
                if (ClientOnContinuousEffectToggled != null)
                    ClientOnContinuousEffectToggled(Specials.Immunity, false, 0);
                _proxy.FinishContinuousSpecial(this, Specials.Immunity);
            }

            if (ClientOnGameOver != null)
                ClientOnGameOver();

            _statistics.GameLost++;

            if (_achievementManager != null)
            {
                int opponentsLeft = _playersData.Count(x => x != null && x.PlayerId != _clientPlayerId && x.State == PlayerData.States.Playing);
                _achievementManager.OnGameOver(_statistics.MoveCount, LinesCleared, PlayingOpponentsInCurrentGame, opponentsLeft, Inventory);
            }
        }

        private void StartRound()
        {
            // Set new current piece to next, increment piece index and create next piece
            if (_mutationCount > 0)
            {
                CurrentPiece = _createPieceFunc(NextPiece.Value, Board.PieceSpawnX, Board.PieceSpawnY, SpawnOrientation, NextPiece.Index + 1, true);
                _mutationCount--;
            }
            else
            {
                CurrentPiece = NextPiece;
                CurrentPiece.Move(Board.PieceSpawnX, Board.PieceSpawnY);
            }
            //
            if (_statistics.PieceCount.ContainsKey(CurrentPiece.Value)) // Update statistics
                _statistics.PieceCount[CurrentPiece.Value]++;
            //
            _pieceIndex++;
            Pieces nextPiece = Pieces.TetriminoS;
            if (_pieceIndex + 1 < _pieces.Size)
                nextPiece = _pieces[_pieceIndex + 1];
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "End of PieceArray reached, server is definitively too slow or we are too fast {0}", _pieceIndex+1);
                _statistics.EndOfPieceQueueReached++;
            }
            if (nextPiece == Pieces.Invalid)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Next piece not yet received from server, server is definitively too slow or we are too fast {0}", _pieceIndex + 1);
                _statistics.NextPieceNotYetReceived++;
            }
            NextPiece = _createPieceFunc(nextPiece, Board.PieceSpawnX, Board.PieceSpawnY, SpawnOrientation, _pieceIndex + 1, _mutationCount > 0);

            if (ClientOnNextPieceModified != null)
                ClientOnNextPieceModified();

            // Inform UI
            if (ClientOnRedraw != null)
                ClientOnRedraw();

            //Log.WriteLine("New piece {0} {1}  next {2}", CurrentPiece.PieceValue, _pieceIndex, NextPiece.PieceValue);
            // Check game over (if current piece has conflict with another piece)
            if (!Board.CheckNoConflict(CurrentPiece))
                EndGame();
            else
            {
                // Start round
                _gameTimer.Start();

                if (ClientOnRoundStarted != null)
                    ClientOnRoundStarted();
            }
        }

        private void FinishRound()
        {
            //Log.WriteLine("Round finished with piece {0} {1}  next {2}", CurrentPiece.PieceValue, _pieceIndex, NextPiece.PieceValue);
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
                if (ClientOnLinesClearedChanged != null)
                    ClientOnLinesClearedChanged(LinesCleared);
                Log.WriteLine(Log.LogLevels.Debug, "{0} lines deleted -> total {1}", deletedRows, LinesCleared);
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
                    if (ClientOnScoreChanged != null)
                        ClientOnScoreChanged(Score);
                    break;
                case 2:
                    _statistics.DoubleCount++;
                    Score += Level * 300;
                    if (ClientOnScoreChanged != null)
                        ClientOnScoreChanged(Score);
                    break;
                case 3:
                    _statistics.TripleCount++;
                    Score += Level * 500;
                    if (ClientOnScoreChanged != null)
                        ClientOnScoreChanged(Score);
                    break;
                case 4:
                    _statistics.TetrisCount++;
                    Score += Level * 800;
                    if (ClientOnScoreChanged != null)
                        ClientOnScoreChanged(Score);
                    break;
            }

            // Check level increase
            if (Level < LinesCleared / 10 && Level < MaxLevel)
            {
                Level = LinesCleared / 10;
                double newInterval = ComputeGameTimerInterval(Level);
                _gameTimer.Interval = newInterval;
                if (ClientOnLevelChanged != null)
                    ClientOnLevelChanged(Level);
                Log.WriteLine(Log.LogLevels.Debug, "Level increased: {0}", Level);
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
                if (ClientOnInventoryChanged != null)
                    ClientOnInventoryChanged();
            }

            // Send piece places to server
            _proxy.PlacePiece(this, _pieceIndex, _pieces.HighestIndex+1, CurrentPiece.Value, CurrentPiece.Orientation, CurrentPiece.PosX, CurrentPiece.PosY, Board.Cells);

            // Send lines if classic style
            if (Options.ClassicStyleMultiplayerRules && deletedRows > 1)
            {
                int addLines = deletedRows - 1;
                if (deletedRows >= 4)
                    // special case for Tetris and above
                    addLines = 4;
                _proxy.SendLines(this, addLines);
            }

            // UI is updated in StartRound

            //
            if (ClientOnRoundFinished != null)
                ClientOnRoundFinished(deletedRows);

            if (_achievementManager != null)
                _achievementManager.OnRoundFinished(deletedRows, Level, _statistics.MoveCount, Score, Board, collapsedPieces);

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
            _clientPlayerId = -1;

            _proxy.OnConnectionLost -= ConnectionLostHandler;
            _proxy.Disconnect();
            _proxy = null;
        }

        private void GameTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (State == States.Playing && AutomaticallyMoveDown)
            {
                MoveDown(true);
            }
        }

        #region IClient

        public string Name { get; private set; }

        public string Team { get; private set; }

        public int PlayerId
        {
            get { return _clientPlayerId; }
        }

        public int MaxPlayersCount
        {
            get { return MaxPlayers; }
        }

        public IPiece CurrentPiece { get; private set; }

        public IPiece NextPiece { get; private set; }

        public IPiece HoldPiece { get; private set; }

        public IBoard Board
        {
            get { return Player == null ? null : Player.Board; }
        }
        
        public bool IsGamePaused
        {
            get
            {
                return ServerState == ServerStates.Paused;
            }
        }

        public bool IsGameStarted
        {
            get
            {
                return ServerState == ServerStates.Playing;
            }
        }

        public bool IsPlaying 
        {
            get
            {
                return State == States.Playing;
            }
        }

        public bool IsRegistered
        {
            get { return State != States.Created && State != States.Registering; }
        }

        public List<Specials> Inventory
        {
            get
            {
                return _inventory.Specials();
            }
        }

        public int LinesCleared { get; private set; }

        public int Level { get; private set; }

        public int Score { get; private set; }

        public int InventorySize
        {
            get { return Options.InventorySize; }
        }

        public GameOptions Options { get; private set; }

        public bool IsServerMaster { get; private set; }

        public int PlayingOpponentsInCurrentGame { get; private set; }

        public IEnumerable<IOpponent> Opponents
        {
            get
            {
                return _playersData.Where(x => x != null && x.PlayerId != _clientPlayerId && x.Board != null && x.State == PlayerData.States.Playing);
            }
        }

        public IClientStatistics Statistics
        {
            get { return _statistics; }
        }

        public IEnumerable<IAchievement> Achievements
        {
            get
            {
                return _achievementManager == null ? null : _achievementManager.Achievements;
            }
        }

        private event ClientConnectionLostHandler ClientOnConnectionLost;
        event ClientConnectionLostHandler IClient.OnConnectionLost
        {
            add { ClientOnConnectionLost += value; }
            remove { ClientOnConnectionLost -= value; }
        }

        private event ClientRoundStartedHandler ClientOnRoundStarted;
        event ClientRoundStartedHandler IClient.OnRoundStarted
        {
            add { ClientOnRoundStarted += value; }
            remove { ClientOnRoundStarted -= value; }
        }

        private event ClientRoundFinishedHandler ClientOnRoundFinished;
        event ClientRoundFinishedHandler IClient.OnRoundFinished
        {
            add { ClientOnRoundFinished += value; }
            remove { ClientOnRoundFinished -= value; }
        }

        private event ClientStartGameHandler ClientOnGameStarted;
        event ClientStartGameHandler IClient.OnGameStarted
        {
            add { ClientOnGameStarted += value; }
            remove { ClientOnGameStarted -= value; }
        }

        private event ClientFinishGameHandler ClientOnGameFinished;
        event ClientFinishGameHandler IClient.OnGameFinished
        {
            add { ClientOnGameFinished += value; }
            remove { ClientOnGameFinished -= value; }
        }

        private event ClientGameOverHandler ClientOnGameOver;
        event ClientGameOverHandler IClient.OnGameOver
        {
            add { ClientOnGameOver += value; }
            remove { ClientOnGameOver -= value; }
        }

        private event ClientPauseGameHandler ClientOnGamePaused;
        event ClientPauseGameHandler IClient.OnGamePaused
        {
            add { ClientOnGamePaused += value; }
            remove { ClientOnGamePaused -= value; }
        }

        private event ClientResumeGameHandler ClientOnGameResumed;
        event ClientResumeGameHandler IClient.OnGameResumed
        {
            add { ClientOnGameResumed += value; }
            remove { ClientOnGameResumed -= value; }
        }

        private event ClientRedrawHandler ClientOnRedraw;
        event ClientRedrawHandler IClient.OnRedraw
        {
            add { ClientOnRedraw += value; }
            remove { ClientOnRedraw -= value; }
        }

        private event ClientRedrawBoardHandler ClientOnRedrawBoard;
        event ClientRedrawBoardHandler IClient.OnRedrawBoard
        {
            add { ClientOnRedrawBoard += value; }
            remove { ClientOnRedrawBoard -= value; }
        }

        private event ClientPieceMovingHandler ClientOnPieceMoving;
        event ClientPieceMovingHandler IClient.OnPieceMoving
        {
            add { ClientOnPieceMoving += value; }
            remove { ClientOnPieceMoving -= value; }
        }

        private event ClientPieceMovedHandler ClientOnPieceMoved;
        event ClientPieceMovedHandler IClient.OnPieceMoved
        {
            add { ClientOnPieceMoved += value; }
            remove { ClientOnPieceMoved -= value; }
        }

        private event ClientNextPieceModifiedHandler ClientOnNextPieceModified;
        event ClientNextPieceModifiedHandler IClient.OnNextPieceModified
        {
            add { ClientOnNextPieceModified += value; }
            remove { ClientOnNextPieceModified -= value; }
        }

        private event ClientHoldPieceModifiedHandler ClientOnHoldPieceModified;
        event ClientHoldPieceModifiedHandler IClient.OnHoldPieceModified
        {
            add { ClientOnHoldPieceModified += value; }
            remove { ClientOnHoldPieceModified -= value; }
        }

        private event ClientPlayerRegisteredHandler ClientOnPlayerRegistered;
        event ClientPlayerRegisteredHandler IClient.OnPlayerRegistered
        {
            add { ClientOnPlayerRegistered += value; }
            remove { ClientOnPlayerRegistered -= value; }
        }

        private event ClientPlayerUnregisteredHandler ClientOnPlayerUnregistered;
        event ClientPlayerUnregisteredHandler IClient.OnPlayerUnregistered
        {
            add { ClientOnPlayerUnregistered += value; }
            remove { ClientOnPlayerUnregistered -= value; }
        }

        private event ClientWinListModifiedHandler ClientOnWinListModified;
        event ClientWinListModifiedHandler IClient.OnWinListModified
        {
            add { ClientOnWinListModified += value; }
            remove { ClientOnWinListModified -= value; }
        }

        private event ClientServerMasterModifiedHandler ClientOnServerMasterModified;
        event ClientServerMasterModifiedHandler IClient.OnServerMasterModified
        {
            add { ClientOnServerMasterModified += value; }
            remove { ClientOnServerMasterModified -= value; }
        }

        private event ClientPlayerLostHandler ClientOnPlayerLost;
        event ClientPlayerLostHandler IClient.OnPlayerLost
        {
            add { ClientOnPlayerLost += value; }
            remove { ClientOnPlayerLost -= value; }
        }

        private event ClientPlayerWonHandler ClientOnPlayerWon;
        event ClientPlayerWonHandler IClient.OnPlayerWon
        {
            add { ClientOnPlayerWon += value; }
            remove { ClientOnPlayerWon -= value; }
        }

        private event ClientPlayerJoinedHandler ClientOnPlayerJoined;
        event ClientPlayerJoinedHandler IClient.OnPlayerJoined
        {
            add { ClientOnPlayerJoined += value; }
            remove { ClientOnPlayerJoined -= value; }
        }

        private event ClientPlayerLeftHandler ClientOnPlayerLeft;
        event ClientPlayerLeftHandler IClient.OnPlayerLeft
        {
            add { ClientOnPlayerLeft += value; }
            remove { ClientOnPlayerLeft -= value; }
        }

        private event ClientPlayerTeamChangedHandler ClientOnPlayerTeamChanged;
        event ClientPlayerTeamChangedHandler IClient.OnPlayerTeamChanged
        {
            add { ClientOnPlayerTeamChanged += value; }
            remove { ClientOnPlayerTeamChanged -= value; }
        }

        private event ClientPlayerPublishMessageHandler ClientOnPlayerPublishMessage;
        event ClientPlayerPublishMessageHandler IClient.OnPlayerPublishMessage
        {
            add { ClientOnPlayerPublishMessage += value; }
            remove { ClientOnPlayerPublishMessage -= value; }
        }

        private event ClientServerPublishMessageHandler ClientOnServerPublishMessage;
        event ClientServerPublishMessageHandler IClient.OnServerPublishMessage
        {
            add { ClientOnServerPublishMessage += value; }
            remove { ClientOnServerPublishMessage -= value; }
        }

        private event ClientInventoryChangedHandler ClientOnInventoryChanged;
        event ClientInventoryChangedHandler IClient.OnInventoryChanged
        {
            add { ClientOnInventoryChanged += value; }
            remove { ClientOnInventoryChanged -= value; }
        }

        private event ClientLinesClearedChangedHandler ClientOnLinesClearedChanged;
        event ClientLinesClearedChangedHandler IClient.OnLinesClearedChanged
        {
            add { ClientOnLinesClearedChanged += value; }
            remove { ClientOnLinesClearedChanged -= value; }
        }

        private event ClientLevelChangedHandler ClientOnLevelChanged;
        event ClientLevelChangedHandler IClient.OnLevelChanged
        {
            add { ClientOnLevelChanged += value; }
            remove { ClientOnLevelChanged -= value; }
        }

        private event ClientScoreChangedHandler ClientOnScoreChanged;
        event ClientScoreChangedHandler IClient.OnScoreChanged
        {
            add { ClientOnScoreChanged += value; }
            remove { ClientOnScoreChanged -= value; }
        }

        private event ClientSpecialUsedHandler ClientOnSpecialUsed;
        event ClientSpecialUsedHandler IClient.OnSpecialUsed
        {
            add { ClientOnSpecialUsed += value; }
            remove { ClientOnSpecialUsed -= value; }
        }

        private event ClientUseSpecialHandler ClientOnUseSpecial;
        event ClientUseSpecialHandler IClient.OnUseSpecial
        {
            add { ClientOnUseSpecial += value; }
            remove { ClientOnUseSpecial -= value; }
        }

        private event ClientPlayerAddLinesHandler ClientOnPlayerAddLines;
        event ClientPlayerAddLinesHandler IClient.OnPlayerAddLines
        {
            add { ClientOnPlayerAddLines += value; }
            remove { ClientOnPlayerAddLines -= value; }
        }

        private event ClientContinuousSpecialToggledHandler ClientOnContinuousEffectToggled;
        event ClientContinuousSpecialToggledHandler IClient.OnContinuousEffectToggled
        {
            add { ClientOnContinuousEffectToggled += value; }
            remove { ClientOnContinuousEffectToggled -= value; }
        }

        private event ClientContinuousSpecialFinishedHandler ClientOnContinuousSpecialFinished;
        event ClientContinuousSpecialFinishedHandler IClient.OnContinuousSpecialFinished
        {
            add { ClientOnContinuousSpecialFinished += value; }
            remove { ClientOnContinuousSpecialFinished -= value; }
        }

        private event ClientAchievementEarnedHandler ClientOnAchievementEarned;
        event ClientAchievementEarnedHandler IClient.OnAchievementEarned
        {
            add { ClientOnAchievementEarned += value; }
            remove { ClientOnAchievementEarned -= value; }
        }

        private event ClientPlayerAchievementEarnedHandler ClientOnPlayerAchievementEarned;
        event ClientPlayerAchievementEarnedHandler IClient.OnPlayerAchievementEarned
        {
            add { ClientOnPlayerAchievementEarned += value; }
            remove { ClientOnPlayerAchievementEarned -= value; }
        }

        public void Dump()
        {
            // Players
            for (int i = 0; i < MaxPlayers; i++)
            {
                PlayerData p = _playersData[i];
                Log.WriteLine(Log.LogLevels.Info, "{0}{1}: {2}", i, i == _clientPlayerId ? "*" : String.Empty, p == null ? String.Empty : p.Name);
            }
            // Inventory
            List<Specials> specials = Inventory;
            StringBuilder sb2 = new StringBuilder();
            foreach (Specials special in specials)
                sb2.Append(ConvertSpecial(special));
            Log.WriteLine(Log.LogLevels.Info, sb2.ToString());
            // Board
            if (_clientPlayerId >= 0 && State == States.Playing)
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
                    Log.WriteLine(Log.LogLevels.Info, sb.ToString());
                }
                Log.WriteLine(Log.LogLevels.Info, "".PadLeft(Board.Width + 2, '-'));
            }
            // TODO: current & next piece
        }

        public bool ConnectAndRegister(Func<ITetriNETCallback, IProxy> createProxyFunc, string name)
        {
            if (createProxyFunc == null)
                throw new ArgumentNullException("createProxyFunc");

            if (_proxy != null)
                return false; // should disconnect first

            try
            {
                _proxy = createProxyFunc(this);
                _proxy.OnConnectionLost += ConnectionLostHandler;

                State = States.Registering;
                Name = name;
                _proxy.RegisterPlayer(this, Name);

                return true;
            }
            catch(Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Problem in ConnectAndRegister. Exception:{0}", ex.ToString());
                return false;
            }
        }
        
        public bool UnregisterAndDisconnect()
        {
            State = States.Created;
            IsServerMaster = false;
            _clientPlayerId = -1;

            if (_proxy != null)
                _proxy.UnregisterPlayer(this);

            if (ClientOnPlayerUnregistered != null)
                ClientOnPlayerUnregistered();

            Disconnect();

            return true;
        }

        public void StartGame()
        {
            if (State == States.Registered && ServerState == ServerStates.Waiting)
                _proxy.StartGame(this);
        }

        public void StopGame()
        {
            if (ServerState == ServerStates.Playing)
                _proxy.StopGame(this);
        }

        public void PauseGame()
        {
            if (ServerState == ServerStates.Playing)
                _proxy.PauseGame(this);
        }

        public void ResumeGame()
        {
            if (ServerState == ServerStates.Paused)
                _proxy.ResumeGame(this);
        }

        public void ResetWinList()
        {
            if (State == States.Registered)
                _proxy.ResetWinList(this);
        }

        public void ChangeTeam(string team)
        {
            Team = team;
            if (State == States.Registered)
                _proxy.PlayerTeam(this, team);
        }

        public void ChangeOptions(GameOptions options)
        {
            if (State == States.Registered)
                _proxy.ChangeOptions(this, options);
        }

        public void KickPlayer(int playerId)
        {
            if (State == States.Registered)
                _proxy.KickPlayer(this, playerId);
        }

        public void BanPlayer(int playerId)
        {
            if (State == States.Registered)
                _proxy.BanPlayer(this, playerId);
        }

        public void PublishMessage(string msg)
        {
            if (State == States.Registered || State == States.Playing)
                _proxy.PublishMessage(this, msg);
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
            //Log.WriteLine(Log.LogLevels.Debug, "DROP {0}", CurrentPiece.Value);

            if (State != States.Playing)
                return;

            //Log.WriteLine(Log.LogLevels.Debug, "ENQUEUE DROP {0} {1}", CurrentPiece.Value, CurrentPiece.Index);
            EnqueueBoardAction(DropAction);
            _statistics.MoveCount++;
        }

        public void MoveDown(bool automatic = false)
        {
            //Log.WriteLine(Log.LogLevels.Debug, "MoveDown");

            if (State != States.Playing)
                return;
            //Log.WriteLine(Log.LogLevels.Debug, "ENQUEUE DOWN {0} {1}", CurrentPiece.Value, CurrentPiece.Index);
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
                    if (ClientOnInventoryChanged != null)
                        ClientOnInventoryChanged();
                }
            }
        }

        public bool UseSpecial(int targetId)
        {
            if (State != States.Playing)
                return false;
            bool succeeded = false;
            // Only if client is playing, target exists and is playing
            PlayerData target = GetPlayer(targetId);
            if (Player.State == PlayerData.States.Playing && target != null && target.State == PlayerData.States.Playing)
            {
                if (target.IsImmune || (targetId == _clientPlayerId && _isImmunityActive)) // Cannot target a player with immunity
                    return false;

                // Get first special and send it
                Specials special;
                if (_inventory.Dequeue(out special))
                {
                    // Update statistics
                    _statistics.SpecialUsed[special]++;
                    //
                    _proxy.UseSpecial(this, targetId, special);
                    succeeded = true;

                    //
                    if (ClientOnInventoryChanged != null)
                        ClientOnInventoryChanged();

                    //
                    if (ClientOnUseSpecial != null)
                        ClientOnUseSpecial(targetId, target.Name, special);

                    //
                    if (_achievementManager != null)
                        _achievementManager.OnUseSpecial(_clientPlayerId, Team, Board, targetId, target.Team, target.Board, special);
                }
            }
            return succeeded;
        }

        public void ResetAchievements()
        {
            if (_achievementManager != null)
                _achievementManager.Reset();
        }

        #endregion

        //private void MoveDownUntilTotallyInBoard(IPiece piece)
        //{
        //    while (!Board.CheckNoConflictWithBoard(piece, true))
        //        piece.Translate(0, -1);
        //}

        private static char ConvertSpecial(Specials special)
        {
            SpecialAttribute attribute = EnumHelper.GetAttribute<SpecialAttribute>(special);
            return attribute == null ? '?' : attribute.ShortName;
        }

        private void TimeoutTask()
        {
            while (true)
            {
                if (State == States.Registered || State == States.Playing || State == States.Paused)
                {
                    // Check server timeout
                    TimeSpan timespan = DateTime.Now - _lastActionFromServer;
                    if (timespan.TotalMilliseconds > TimeoutDelay && IsTimeoutDetectionActive)
                    {
                        Log.WriteLine(Log.LogLevels.Debug, "Timeout++");
                        // Update timeout count
                        SetTimeout();
                        if (_timeoutCount >= MaxTimeoutCountBeforeDisconnection)
                            ConnectionLostHandler(); // timeout
                    }

                    // Send heartbeat if needed
                    TimeSpan delaySinceLastActionToServer = DateTime.Now - _proxy.LastActionToServer;
                    if (delaySinceLastActionToServer.TotalMilliseconds > HeartbeatDelay)
                    {
                        _proxy.Heartbeat(this);
                    }

                }

                if (_isConfusionActive)
                {
                    if (DateTime.Now > _confusionEndTime)
                    {
                        _isConfusionActive = false;
                        if (ClientOnContinuousEffectToggled != null)
                            ClientOnContinuousEffectToggled(Specials.Confusion, false, 0);
                        _proxy.FinishContinuousSpecial(this, Specials.Confusion);
                    }
                }

                if (_isDarknessActive)
                {
                    if (DateTime.Now > _darknessEndTime)
                    {
                        _isDarknessActive = false;
                        if (ClientOnContinuousEffectToggled != null)
                            ClientOnContinuousEffectToggled(Specials.Darkness, false, 0);
                        _proxy.FinishContinuousSpecial(this, Specials.Darkness);
                    }
                }

                if (_isImmunityActive)
                {
                    if (DateTime.Now > _immunityEndTime)
                    {
                        _isImmunityActive = false;
                        if (ClientOnContinuousEffectToggled != null)
                            ClientOnContinuousEffectToggled(Specials.Immunity, false, 0);
                        _proxy.FinishContinuousSpecial(this, Specials.Immunity);
                    }
                }

                // Stop task if stop event is raised
                if (_stopBackgroundTaskEvent.WaitOne(10))
                {
                    Log.WriteLine(Log.LogLevels.Info, "Stop background task event raised");
                    break;
                }
            }
        }

        #region Board Action Queue
        //private readonly ManualResetEvent _actionEnqueuedEvent = new ManualResetEvent(false);
        //private readonly ConcurrentQueue<Action> _boardActionQueue = new ConcurrentQueue<Action>();

        private readonly BlockingCollection<Action> _boardActionBlockingCollection = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
        private CancellationTokenSource _cancellationTokenSource;

        private void EnqueueBoardAction(Action action)
        {
            //_boardActionQueue.Enqueue(action);
            //_actionEnqueuedEvent.Set();

            _boardActionBlockingCollection.Add(action);
        }

        private void BoardActionTask()
        {
            Log.WriteLine(Log.LogLevels.Info, "BoardActionTask started");

            //WaitHandle[] waitHandles =
            //{
            //    _stopBackgroundTaskEvent,
            //    _actionEnqueuedEvent
            //};

            //while (true)
            //{
            //    int handle = WaitHandle.WaitAny(waitHandles, 100);
            //    if (handle == 0) // _stopBackgroundTaskEvent
            //        break; // Stop here
            //    // Even if WaitAny returned WaitHandle.WaitTimeout, we check action queue
            //    _actionEnqueuedEvent.Reset();
            //    // Perform board actions
            //    if (State == States.Playing && !_boardActionQueue.IsEmpty)
            //    {
            //        while (!_boardActionQueue.IsEmpty)
            //        {
            //            Action action;
            //            bool dequeue = _boardActionQueue.TryDequeue(out action);
            //            if (dequeue)
            //            {
            //                try
            //                {
            //                    action();
            //                    Thread.Sleep(1);
            //                }
            //                catch (Exception ex)
            //                {
            //                    Log.WriteLine(Log.LogLevels.Error, "Exception raised in BoardActionTask. Exception:{0}", ex);
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
                    bool taken = _boardActionBlockingCollection.TryTake(out action, 10, _cancellationTokenSource.Token);
                    if (taken)
                    {
                        try
                        {
                            Log.WriteLine(Log.LogLevels.Debug, "Dequeue, item in queue {0}", _boardActionBlockingCollection.Count);
                            action();
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine(Log.LogLevels.Error, "Exception raised in BoardActionTask. Exception:{0}", ex);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.WriteLine(Log.LogLevels.Info, "Taking cancelled");
                    break;
                }
            }

            Log.WriteLine(Log.LogLevels.Info, "BoardActionTask stopped");
        }
        #endregion

        #region Board Actions
        private void SpecialUsed(Specials special)
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
                    // NOP: Managed by Server
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
            if (ClientOnPieceMoving != null)
                ClientOnPieceMoving();

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
                    Log.WriteLine(Log.LogLevels.Warning, "End of PieceArray reached, server is definitively too slow or we are too fast {0}", _pieceIndex + 1);
                    _statistics.EndOfPieceQueueReached++;
                }
                if (nextPiece == Pieces.Invalid)
                {
                    Log.WriteLine(Log.LogLevels.Warning, "Next piece not yet received from server, server is definitively too slow or we are too fast {0}", _pieceIndex + 1);
                    _statistics.NextPieceNotYetReceived++;
                }
                NextPiece = _createPieceFunc(nextPiece, Board.PieceSpawnX, Board.PieceSpawnY, SpawnOrientation, _pieceIndex + 1, _mutationCount > 1);

                if (ClientOnNextPieceModified != null)
                    ClientOnNextPieceModified();
            }
            else
            {
                IPiece swap = CurrentPiece;
                CurrentPiece = HoldPiece;
                HoldPiece = swap;
            }

            // Move Current to Spawn location
            CurrentPiece.Move(Board.PieceSpawnX, Board.PieceSpawnY);

            if (ClientOnHoldPieceModified != null)
                ClientOnHoldPieceModified();

            // Display current piece
            if (ClientOnPieceMoved != null)
                ClientOnPieceMoved();

            //
            _gameTimer.Start();

            //
            _holdAlreadyUsed = true;
        }

        private void DropAction()
        {
            //Log.WriteLine(Log.LogLevels.Debug, "DROP {0} {1}", CurrentPiece.Value, CurrentPiece.Index);
            _gameTimer.Stop();

            // Inform UI
            if (ClientOnPieceMoving != null)
                ClientOnPieceMoving();
            // Perform move
            Board.Drop(CurrentPiece);
            // Inform UI
            if (ClientOnPieceMoved != null)
                ClientOnPieceMoved();

            //
            PlaceCurrentPiece();
            //
            FinishRound();
        }

        private void MoveDownAction()
        {
            //Log.WriteLine(Log.LogLevels.Debug, "DOWN {0} {1}", CurrentPiece.Value, CurrentPiece.Index);

            // Inform UI
            if (ClientOnPieceMoving != null)
                ClientOnPieceMoving();
            // Perform move
            bool movedDown = Board.MoveDown(CurrentPiece);
            // Inform UI
            if (ClientOnPieceMoved != null)
                ClientOnPieceMoved();

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
            if (ClientOnPieceMoving != null)
                ClientOnPieceMoving();
            // Perform move
            Board.MoveLeft(CurrentPiece);
            // Inform UI
            if (ClientOnPieceMoved != null)
                ClientOnPieceMoved();
        }

        private void MoveRightAction()
        {
            // Inform UI
            if (ClientOnPieceMoving != null)
                ClientOnPieceMoving();
            // Perform move
            Board.MoveRight(CurrentPiece);
            // Inform UI
            if (ClientOnPieceMoved != null)
                ClientOnPieceMoved();
        }

        private void RotateClockwiseAction()
        {
            // Inform UI
            if (ClientOnPieceMoving != null)
                ClientOnPieceMoving();
            // Perform move
            Board.RotateClockwise(CurrentPiece);
            // Inform UI
            if (ClientOnPieceMoved != null)
                ClientOnPieceMoved();
        }

        private void RotateCounterClockwiseAction()
        {
            // Inform UI
            if (ClientOnPieceMoving != null)
                ClientOnPieceMoving();
            // Perform move
            Board.RotateCounterClockwise(CurrentPiece);
            // Inform UI
            if (ClientOnPieceMoved != null)
                ClientOnPieceMoved();
        }

        private void ModifyGrid(byte[] cells)
        {
            Player.Board.SetCells(cells);
            Board.RemoveCellsHigherThan(16);
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        #region Specials
        private void AddLines(int count)
        {
            if (count <= 0)
                return;
            Board.AddLines(count, () => RangeRandom.Random(Options.PieceOccurancies));
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        private void ClearLine()
        {
            Board.ClearLine();
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        private void NukeField()
        {
            Board.NukeField();
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        private void RandomBlocksClear(int count)
        {
            Board.RandomBlocksClear(count);
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        private void ClearSpecialBlocks()
        {
            Board.ClearSpecialBlocks(() => RangeRandom.Random(Options.PieceOccurancies));
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        private void BlockGravity()
        {
            Board.BlockGravity();
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        private void BlockQuake()
        {
            Board.BlockQuake();
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        private void BlockBomb()
        {
            Board.BlockBomb();
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        private void ClearColumn()
        {
            Board.ClearColumn();
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
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
            if (ClientOnContinuousEffectToggled != null)
                ClientOnContinuousEffectToggled(Specials.Darkness, true, (_darknessEndTime - DateTime.Now).TotalSeconds);
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
            if (ClientOnContinuousEffectToggled != null)
                ClientOnContinuousEffectToggled(Specials.Confusion, true, (_confusionEndTime - DateTime.Now).TotalSeconds);
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
            if (ClientOnContinuousEffectToggled != null)
                ClientOnContinuousEffectToggled(Specials.Immunity, true, (_immunityEndTime - DateTime.Now).TotalSeconds);
        }

        private void ZebraField()
        {
            Board.ZebraField();
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        private void LeftGravity()
        {
            Board.LeftGravity();
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        #endregion

        #endregion
    }
}
