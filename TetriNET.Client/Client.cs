using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TetriNET.Common;
using TetriNET.Common.Contracts;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Randomizer;

namespace TetriNET.Client
{
    internal sealed class Player : IOpponent
    {
        public enum States
        {
            Joined,
            Playing,
            Lost,
        }

        public int PlayerId { get; set; }
        public string Name { get; set; }
        public IBoard Board { get; set; }
        public States State { get; set; }
    }

    internal sealed class Inventory
    {
        private readonly object _lock = new object();
        private readonly List<Specials> _queue;
        private int _size;

        public Inventory(int size)
        {
            _lock = new object();
            _size = size;
            _queue = new List<Specials>();
        }

        public void Reset(int size)
        {
            lock (_lock)
            {
                _size = size;
                _queue.Clear();
            }
        }

        public bool Enqueue(Specials special)
        {
            bool result = false;
            lock (_lock)
            {
                if (_queue.Count < _size)
                {
                    _queue.Add(special);
                    result = true;
                }
            }
            return result;
        }

        public void Enqueue(List<Specials> specials)
        {
            lock (_lock)
            {
                foreach (Specials special in specials)
                {
                    bool enqueued = Enqueue(special);
                    if (!enqueued)
                        break;
                }
            }
        }

        public bool Dequeue(out Specials special)
        {
            special = 0;
            bool result = false;
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    special = _queue[0];
                    _queue.RemoveAt(0);
                    result = true;
                }
            }
            return result;
        }

        public List<Specials> Specials()
        {
            List<Specials> specials;
            lock (_lock)
            {
                specials = new List<Specials>(_queue);
            }
            return specials;
        }
    }

    internal sealed class TetriminoArray
    {
        private readonly object _lock = new object();
        private int _size;
        private Tetriminos[] _array;

        public TetriminoArray(int size)
        {
            _size = size;
            _array = new Tetriminos[_size];
        }

        public Tetriminos this[int index]
        {
            get
            {
                Tetriminos tetrimino;
                lock (_lock)
                {
                    tetrimino = _array[index];
                }
                return tetrimino;
            }
            set
            {
                lock (_lock)
                {
                    if (index >= _size)
                        Grow(64);
                    _array[index] = value;
                }
            }
        }

        private void Grow(int increment)
        {
            int newSize = _size + increment;
            Tetriminos[] newArray = new Tetriminos[newSize];
            if (_size > 0)
                Array.Copy(_array, newArray, _size);
            _array = newArray;
            _size = newSize;
        }

        public string Dump(int size)
        {
            return _array.Take(size).Select((t, i) => "[" + i.ToString(CultureInfo.InvariantCulture) + ":" + t.ToString() + "]").Aggregate((s, t) => s + "," + t);
        }
    }

    public sealed class Client : ITetriNETCallback, IClient
    {
        private const int MaxPlayers = 6;
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const bool IsTimeoutDetectionActive = false;

        public enum States
        {
            Created, // -> Registering
            Registering, // --> Registered
            Registered, // --> Playing
            Playing, // --> Paused | Registered
            Paused // --> Playing | Registered
        }

        private readonly Func<Tetriminos, int, int, int, ITetrimino> _createTetriminoFunc;
        private readonly Func<IBoard> _createBoardFunc;
        private readonly Player[] _players = new Player[MaxPlayers];
        private readonly ManualResetEvent _stopBackgroundTaskEvent = new ManualResetEvent(false);
        private readonly TetriminoArray _tetriminos;
        private readonly Inventory _inventory;
        private readonly System.Timers.Timer _gameTimer;

        public States State { get; private set; }

        private IProxy _proxy;
        private GameOptions _options; // TODO: check if an entry for each specials and for each tetriminos (set to dummy/default values otherwise)
        private int _clientPlayerId;
        private DateTime _lastActionFromServer;
        private int _timeoutCount;
        private int _tetriminoIndex;

        private Player Player
        {
            get { return _players[_clientPlayerId]; }
        }

        // TODO: don't create proxy immediately, use connect method
        public Client(Func<Tetriminos, int, int, int, ITetrimino> createTetriminoFunc, Func<IBoard> createBoardFunc)
        {
            if (createTetriminoFunc == null)
                throw new ArgumentNullException("createTetriminoFunc");
            if (createBoardFunc == null)
                throw new ArgumentNullException("createBoardFunc");

            _createTetriminoFunc = createTetriminoFunc;
            _createBoardFunc = createBoardFunc;

            // default options
            _options = new GameOptions(); // TODO: get options from save file

            _tetriminos = new TetriminoArray(64);
            _inventory = new Inventory(_options.InventorySize);

            _gameTimer = new System.Timers.Timer();
            _gameTimer.Interval = 1000; // TODO: use a function to compute interval in function of level
            _gameTimer.Elapsed += GameTimerOnElapsed;

            _lastActionFromServer = DateTime.Now;
            _timeoutCount = 0;

            _clientPlayerId = -1;

           
            State = States.Created;

            Task.Factory.StartNew(TimeoutTask);
        }

        #region IProxy event handler

        private void ConnectionLostHandler()
        {
            if (ClientOnConnectionLost != null)
                ClientOnConnectionLost();
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

        public void OnPlayerRegistered(bool succeeded, int playerId, bool isGameStarted)
        {
            ResetTimeout();
            if (succeeded && State == States.Registering)
            {
                Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Registered as player {0} game started {1}", playerId, isGameStarted);

                if (playerId >= 0)
                {
                    _clientPlayerId = playerId;
                    _players[_clientPlayerId] = new Player
                    {
                        Name = Name,
                        Board = _createBoardFunc(),
                        State = Player.States.Joined
                    };

                    if (ClientOnPlayerRegistered != null)
                        ClientOnPlayerRegistered(true, playerId);

                    if (isGameStarted)
                    {
                        Player.Board.FillWithRandomCells(() => RangeRandom.Random(_options.TetriminoOccurancies));

                        if (ClientOnRedraw != null)
                            ClientOnRedraw();
                    }
                    State = States.Registered;
                }
                else
                {
                    State = States.Created;
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Warning, "Wrong id {0}", playerId);
                }
            }
            else
            {
                State = States.Created;

                if (ClientOnPlayerRegistered != null)
                    ClientOnPlayerRegistered(false, -1);

                Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Registration failed");
            }
        }

        public void OnPlayerJoined(int playerId, string name)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Player {0}[{1}] joined", name, playerId);

            ResetTimeout();
            if (playerId != _clientPlayerId && playerId >= 0)
            {
                // don't update ourself
                _players[playerId] = new Player
                {
                    Name = name,
                    Board = _createBoardFunc(),
                    State = Player.States.Joined
                };

                if (ClientOnPlayerJoined != null)
                    ClientOnPlayerJoined(playerId, name);

                if (IsGameStarted)
                {
                    _players[playerId].Board.FillWithRandomCells(() => RangeRandom.Random(_options.TetriminoOccurancies));

                    if (ClientOnRedrawBoard != null)
                        ClientOnRedrawBoard(playerId, _players[playerId].Board);
                }
            }
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Player {0}[{1}] left ({2})", name, playerId, reason);

            ResetTimeout();
            if (playerId != _clientPlayerId && playerId >= 0)
            {
                _players[playerId] = null;

                if (ClientOnPlayerLeft != null)
                    ClientOnPlayerLeft(playerId, name);
            }
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "{0}:{1}", playerName, msg);

            ResetTimeout();

            if (ClientOnPlayerPublishMessage != null)
                ClientOnPlayerPublishMessage(playerName, msg);
        }

        public void OnPublishServerMessage(string msg)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "{0}", msg);

            ResetTimeout();

            if (ClientOnServerPublishMessage != null)
                ClientOnServerPublishMessage(msg);
        }

        public void OnPlayerLost(int playerId)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Player [{0}] {1} has lost", playerId, _players[playerId].Name);

            ResetTimeout();
            if (playerId != _clientPlayerId && _players[playerId] != null && _players[playerId].State == Player.States.Playing)
            {
                _players[playerId].State = Player.States.Lost;
                _players[playerId].Board.FillWithRandomCells(() => RangeRandom.Random(_options.TetriminoOccurancies));

                if (ClientOnRedrawBoard != null)
                    ClientOnRedrawBoard(playerId, _players[playerId].Board);

                if (ClientOnPlayerLost != null)
                    ClientOnPlayerLost(playerId, _players[playerId].Name);
            }
        }

        public void OnPlayerWon(int playerId)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Player [{0}] {1} has won", playerId, _players[playerId].Name);

            ResetTimeout();
            if (_players[playerId] != null)
            {
                _players[playerId].State = Player.States.Joined;
                if (ClientOnPlayerWon != null)
                    ClientOnPlayerWon(playerId, _players[playerId].Name);
            }
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, Tetriminos thirdTetrimino, GameOptions options)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Game started with {0} {1} {2}", firstTetrimino, secondTetrimino, thirdTetrimino);

            ResetTimeout();
            State = States.Playing;
            _options = options;
            // reset tetriminos
            _tetriminos[0] = firstTetrimino;
            _tetriminos[1] = secondTetrimino;
            _tetriminos[2] = thirdTetrimino;
            _tetriminoIndex = 0;
            CurrentTetrimino = _createTetriminoFunc(firstTetrimino, Board.TetriminoSpawnX, Board.TetriminoSpawnY, 1);
            MoveDownUntilTotallyInBoard(CurrentTetrimino);
            NextTetrimino = _createTetriminoFunc(secondTetrimino, Board.TetriminoSpawnX, Board.TetriminoSpawnY, 1);
            // reset inventory
            _inventory.Reset(_options.InventorySize);
            // reset line and level
            LinesCleared = 0;
            Level = _options.StartingLevel;
            // Reset boards
            for (int i = 0; i < MaxPlayers; i++)
                if (_players[i] != null)
                {
                    if (_players[i].Board != null)
                        _players[i].Board.Clear();
                    _players[i].State = Player.States.Playing;
                }
            // Restart timer
            _gameTimer.Start();

            if (ClientOnGameStarted != null)
                ClientOnGameStarted();

            //Logger.Log.WriteLine("TETRIMINOS:{0}", _tetriminos.Dump(8));
        }

        public void OnGameFinished()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Game finished");

            _gameTimer.Stop();

            ResetTimeout();
            State = States.Registered;
            for (int i = 0; i < MaxPlayers; i++)
                if (_players[i] != null)
                    _players[i].State = Player.States.Joined;

            if (ClientOnGameFinished != null)
                ClientOnGameFinished();
        }

        public void OnGamePaused()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Game paused");

            ResetTimeout();
            if (State == States.Playing)
            {
                State = States.Paused;

                if (ClientOnGamePaused != null)
                    ClientOnGamePaused();
            }
        }

        public void OnGameResumed()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Game resumed");

            ResetTimeout();
            if (State == States.Paused)
            {
                State = States.Playing;

                if (ClientOnGameResumed != null)
                    ClientOnGameResumed();
            }
        }

        public void OnServerAddLines(int lineCount)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Server add {0} lines", lineCount);

            ResetTimeout();

            if (State == States.Playing)
            {
                AddLines(lineCount);
                _proxy.ModifyGrid(this, Player.Board.Cells);
            }
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Player {0} add {1} lines (special [{2}])", playerId, lineCount, specialId);

            ResetTimeout();

            if (State == States.Playing)
            {
                if (playerId != _clientPlayerId)
                {
                    // Don't add line to ourself
                    AddLines(lineCount);
                    _proxy.ModifyGrid(this, Player.Board.Cells);
                }

                if (ClientOnPlayerAddLines != null)
                {
                    Player player = GetPlayer(playerId);
                    string playerName = player == null ? "???" : player.Name;

                    ClientOnPlayerAddLines(playerName, specialId, lineCount);
                }
            }
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Special {0}[{1}] from {2} to {3}", special, specialId, playerId, targetId);

            ResetTimeout();

            if (State == States.Playing)
            {
                if (targetId == _clientPlayerId)
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
                        case Specials.ZebraField:
                            ZebraField();
                            break;
                    }
                }
                if (ClientOnSpecialUsed != null)
                {
                    Player player = GetPlayer(playerId);
                    Player target = GetPlayer(targetId);

                    string playerName = player == null ? "???" : player.Name;
                    string targetName = target == null ? "???" : target.Name;

                    ClientOnSpecialUsed(playerName, targetName, specialId, special);
                }
            }
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            ResetTimeout();

            if (State == States.Playing)
            {
                _tetriminos[index] = tetrimino;
            }
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Player [{0}] {1} modified", playerId, _players[playerId].Name);

            ResetTimeout();

            if (State == States.Playing)
            {
                Player player = GetPlayer(playerId);
                if (playerId == _clientPlayerId)
                {
                    int a = 5;
                }
                if (player != null)
                {
                    player.Board.SetCells(grid);
                    // if modifying own grid, special Switch occured -> remove lines above 16
                    if (playerId == _clientPlayerId)
                    {
                        Board.RemoveCellsHigherThan(16);
                        if (ClientOnRedraw != null)
                            ClientOnRedraw();
                    }
                    else
                    {
                        if (ClientOnRedrawBoard != null)
                            ClientOnRedrawBoard(playerId, _players[playerId].Board);
                    }
                }
            }
        }

        public void OnServerMasterChanged(int playerId)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Server master changed: [{0}]", playerId);

            ResetTimeout();

            if (ClientOnServerMasterModified != null)
                ClientOnServerMasterModified(playerId == _clientPlayerId);
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Win list: {0}", winList.Any() ? winList.Select(x => String.Format("{0}:{1}", x.PlayerName, x.Score)).Aggregate((n, i) => n + "|" + i) : "");

            ResetTimeout();

            if (ClientOnWinListModified != null)
                ClientOnWinListModified(winList);
        }

        #endregion

        private Player GetPlayer(int playerId)
        {
            if (playerId >= 0 && playerId < MaxPlayers)
                return _players[playerId];
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

        private void PlaceCurrentTetrimino()
        {
            Board.CommitTetrimino(CurrentTetrimino);
        }

        private void EndGame()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "End game");
            _gameTimer.Stop();
            //
            State = States.Registered;
            // Send player lost
            _proxy.GameLost(this);

            if (ClientOnGameOver != null)
                ClientOnGameOver();
        }

        private void StartRound()
        {
            // Set new current tetrimino to next, increment tetrimino index and create next tetrimino
            CurrentTetrimino = NextTetrimino;
            MoveDownUntilTotallyInBoard(CurrentTetrimino);
            _tetriminoIndex++;
            NextTetrimino = _createTetriminoFunc(_tetriminos[_tetriminoIndex + 1], Board.TetriminoSpawnX, Board.TetriminoSpawnY, 1);

            //if (ClientOnTetriminoMoved != null)
            //    ClientOnTetriminoMoved();
            // Inform UI
            if (ClientOnRedraw != null)
                ClientOnRedraw();

            //Logger.Log.WriteLine("New tetrimino {0} {1}  next {2}", CurrentTetrimino.TetriminoValue, _tetriminoIndex, NextTetrimino.TetriminoValue);
            // Check game over (if current tetrimino has conflict with another tetrimino)
            if (!Board.CheckNoConflict(CurrentTetrimino))
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
            //Logger.Log.WriteLine("Round finished with tetrimino {0} {1}  next {2}", CurrentTetrimino.TetriminoValue, _tetriminoIndex, NextTetrimino.TetriminoValue);
            // Stop game
            _gameTimer.Stop();
            //Logger.Log.WriteLine("TETRIMINOS:{0}", _tetriminos.Dump(8));
            // Delete rows and get specials
            List<Specials> specials;
            int deletedRows = DeleteRows(out specials);
            LinesCleared += deletedRows;
            if (deletedRows > 0)
            {
                if (ClientOnLinesClearedChanged != null)
                    ClientOnLinesClearedChanged();
                Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "{0} lines deleted -> total {1}", deletedRows, LinesCleared);
            }

            // Check level increase
            if (Level < LinesCleared / 10)
            {
                Level = LinesCleared / 10;
                if (ClientOnLevelChanged != null)
                    ClientOnLevelChanged();
                Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Level increased: {0}", Level);
                // TODO: change game timer interval
            }

            // Add specials to inventory
            if (specials.Count > 0)
                _inventory.Enqueue(specials);

            // Transform cell into special blocks
            if (deletedRows >= _options.LinesToMakeForSpecials && _options.SpecialsAddedEachTime > 0)
            {
                Board.SpawnSpecialBlocks(deletedRows*_options.SpecialsAddedEachTime, () => RangeRandom.Random(_options.SpecialOccurancies));
                //
                if (ClientOnInventoryChanged != null)
                    ClientOnInventoryChanged();
            }

            // Send tetrimino places to server
            _proxy.PlaceTetrimino(this, _tetriminoIndex, CurrentTetrimino.Value, CurrentTetrimino.Orientation, CurrentTetrimino.PosX, CurrentTetrimino.PosY, Board.Cells);

            // Send lines if classic style
            if (_options.ClassicStyleMultiplayerRules && deletedRows > 1)
            {
                int addLines = deletedRows - 1;
                if (deletedRows >= 4) // special case for Tetris and above
                    addLines = 4;
                _proxy.SendLines(this, addLines);
            }

            // UI is updated in StartRound

            //
            if (ClientOnRoundFinished != null)
                ClientOnRoundFinished();

            // Start next round
            StartRound();
        }

        private int DeleteRows(out List<Specials> specials)
        {
            return Board.CollapseCompletedRows(out specials);
        }

        private void GameTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (State == States.Playing)
            {
                MoveDown();
            }
        }

        #region Specials

        // Add junk lines
        private void AddLines(int count)
        {
            if (count <= 0)
                return;
            Board.AddLines(count, () => RangeRandom.Random(_options.TetriminoOccurancies));
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
            Board.ClearSpecialBlocks(() => RangeRandom.Random(_options.TetriminoOccurancies));
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

        private void ZebraField()
        {
            Board.ZebraField();
            _proxy.ModifyGrid(this, Player.Board.Cells);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        // special Switch is handled by server
        #endregion

        #region IClient

        public string Name { get; private set; }

        public int PlayerId
        {
            get { return _clientPlayerId; }
        }

        public int MaxPlayersCount
        {
            get { return MaxPlayers; }
        }

        public ITetrimino CurrentTetrimino { get; private set; }
        public ITetrimino NextTetrimino { get; private set; }

        public IBoard Board
        {
            get { return Player.Board; }
        }
        
        public bool IsGamePaused
        {
            get
            {
                return State == States.Paused;
            }
        }

        public bool IsGameStarted
        {
            get
            {
                return State == States.Playing;
            }
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

        public int InventorySize
        {
            get { return _options.InventorySize; }
        }

        public IEnumerable<IOpponent> Opponents
        {
            get
            {
                return _players.Where(x => x != null && x.PlayerId != _clientPlayerId && x.Board != null && x.State == Player.States.Playing);
            }
        }

        public bool Connect(Func<ITetriNETCallback, IProxy> createProxyFunc)
        {
            if (_proxy != null)
                return false; // should disconnect first

            _proxy = createProxyFunc(this);
            _proxy.OnConnectionLost += ConnectionLostHandler;

            return _proxy.Connect();
        }

        public bool Disconnect()
        {
            if (_proxy == null)
                return false; // should connect first

            _proxy.OnConnectionLost -= ConnectionLostHandler;
            _proxy.Disconnect();
            _proxy = null;

            return true;
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

        private event ClientTetriminoMovingHandler ClientOnTetriminoMoving;
        event ClientTetriminoMovingHandler IClient.OnTetriminoMoving
        {
            add { ClientOnTetriminoMoving += value; }
            remove { ClientOnTetriminoMoving -= value; }
        }

        private event ClientTetriminoMovedHandler ClientOnTetriminoMoved;
        event ClientTetriminoMovedHandler IClient.OnTetriminoMoved
        {
            add { ClientOnTetriminoMoved += value; }
            remove { ClientOnTetriminoMoved -= value; }
        }

        private event ClientPlayerRegisteredHandler ClientOnPlayerRegistered;
        event ClientPlayerRegisteredHandler IClient.OnPlayerRegistered
        {
            add { ClientOnPlayerRegistered += value; }
            remove { ClientOnPlayerRegistered -= value; }
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

        private event ClientSpecialUsedHandler ClientOnSpecialUsed;
        event ClientSpecialUsedHandler IClient.OnSpecialUsed
        {
            add { ClientOnSpecialUsed += value; }
            remove { ClientOnSpecialUsed -= value; }
        }

        private event ClientPlayerAddLines ClientOnPlayerAddLines;
        event ClientPlayerAddLines IClient.OnPlayerAddLines
        {
            add { ClientOnPlayerAddLines += value; }
            remove { ClientOnPlayerAddLines -= value; }
        }

        public void Dump()
        {
            // Players
            for (int i = 0; i < MaxPlayers; i++)
            {
                Player p = _players[i];
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "{0}{1}: {2}", i, i == _clientPlayerId ? "*" : String.Empty, p == null ? String.Empty : p.Name);
            }
            // Inventory
            List<Specials> specials = Inventory;
            StringBuilder sb2 = new StringBuilder();
            foreach (Specials special in specials)
                sb2.Append(ConvertSpecial(special));
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, sb2.ToString());
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
                            Tetriminos cellTetrimino = CellHelper.GetColor(cellValue);
                            Specials cellSpecial = CellHelper.GetSpecial(cellValue);
                            if (cellSpecial == Specials.Invalid)
                                sb.Append((int) cellTetrimino);
                            else
                                sb.Append(ConvertSpecial(cellSpecial));
                        }
                    }
                    sb.Append("|");
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Info, sb.ToString());
                }
                Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "".PadLeft(Board.Width + 2, '-'));
            }
            // TODO: current & next tetrimino
        }

        public void Register(string name)
        {
            State = States.Registering;
            Name = name;
            _proxy.RegisterPlayer(this, Name);
        }

        public void Unregister()
        {
            State = States.Created;
            _proxy.UnregisterPlayer(this);
        }

        public void StartGame()
        {
            if (State == States.Registered)
                _proxy.StartGame(this);
        }

        public void StopGame()
        {
            if (State == States.Playing)
                _proxy.StopGame(this);
        }

        public void PauseGame()
        {
            if (State == States.Playing)
                _proxy.PauseGame(this);
        }

        public void ResumeGame()
        {
            if (State == States.Paused)
                _proxy.ResumeGame(this);
        }

        public void ResetWinList()
        {
            if (State == States.Registered)
                _proxy.ResetWinList(this);
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
            if (State == States.Registered)
                _proxy.PublishMessage(this, msg);
        }

        public void Drop()
        {
            if (State != States.Playing)
                return;
            _gameTimer.Stop();
            // Inform UI
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            // Perform move
            Board.Drop(CurrentTetrimino);
            // Inform UI
            if (ClientOnTetriminoMoved != null)
                ClientOnTetriminoMoved();
            //
            PlaceCurrentTetrimino();
            //
            FinishRound();
        }

        public void MoveDown()
        {
            if (State != States.Playing)
                return;
            // Inform UI
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            // Perform move
            bool movedDown = Board.MoveDown(CurrentTetrimino);
            // Inform UI
            if (ClientOnTetriminoMoved != null)
                ClientOnTetriminoMoved();

            // If cannot move down anymore, round is finished
            if (!movedDown)
            {
                //
                PlaceCurrentTetrimino();
                //
                FinishRound();
            }
        }

        public void MoveLeft()
        {
            if (State != States.Playing)
                return;
            // Inform UI
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            // Perform move
            Board.MoveLeft(CurrentTetrimino);
            // Inform UI
            if (ClientOnTetriminoMoved != null)
                ClientOnTetriminoMoved();
        }

        public void MoveRight()
        {
            if (State != States.Playing)
                return;
            // Inform UI
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            // Perform move
            Board.MoveRight(CurrentTetrimino);
            // Inform UI
            if (ClientOnTetriminoMoved != null)
                ClientOnTetriminoMoved();
        }

        public void RotateClockwise()
        {
            if (State != States.Playing)
                return;
            // Inform UI
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            // Perform move
            Board.RotateClockwise(CurrentTetrimino);
            // Inform UI
            if (ClientOnTetriminoMoved != null)
                ClientOnTetriminoMoved();
        }

        public void RotateCounterClockwise()
        {
            if (State != States.Playing)
                return;
            // Inform UI
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            // Perform move
            Board.RotateCounterClockwise(CurrentTetrimino);
            // Inform UI
            if (ClientOnTetriminoMoved != null)
                ClientOnTetriminoMoved();
        }

        public void DiscardFirstSpecial()
        {
            if (State != States.Playing)
                return;
            // Only if client is playing, target exists and is playing
            if (Player.State == Player.States.Playing)
            {
                // Get first special and discard it
                Specials special;
                _inventory.Dequeue(out special);

                //
                if (ClientOnInventoryChanged != null)
                    ClientOnInventoryChanged();
            }
        }

        public bool UseSpecial(int targetId)
        {
            if (State != States.Playing)
                return false;
            bool succeeded = false;
            // Only if client is playing, target exists and is playing
            Player target = GetPlayer(targetId);
            if (Player.State == Player.States.Playing && target != null && target.State == Player.States.Playing)
            {
                // Get first special and send it
                Specials special;
                if (_inventory.Dequeue(out special))
                {
                    _proxy.UseSpecial(this, targetId, special);
                    succeeded = true;
                }

                //
                if (ClientOnInventoryChanged != null)
                    ClientOnInventoryChanged();
            }
            return succeeded;
        }

        #endregion

        private void MoveDownUntilTotallyInBoard(ITetrimino tetrimino)
        {
            while (!Board.CheckNoConflictWithBoard(tetrimino))
                tetrimino.Translate(0, -1);
        }

        private static char ConvertSpecial(Specials special)
        {
            switch (special)
            {
                case Specials.AddLines:
                    return 'A';
                case Specials.ClearLines:
                    return 'C';
                case Specials.NukeField:
                    return 'N';
                case Specials.RandomBlocksClear:
                    return 'R';
                case Specials.SwitchFields:
                    return 'S';
                case Specials.ClearSpecialBlocks:
                    return 'B';
                case Specials.BlockGravity:
                    return 'G';
                case Specials.BlockQuake:
                    return 'Q';
                case Specials.BlockBomb:
                    return 'O';
                case Specials.ClearColumn:
                    return 'V';
                case Specials.ZebraField:
                    return 'Z';
            }
            return '?';
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
                        Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Timeout++");
                        // Update timeout count
                        SetTimeout();
                        if (_timeoutCount >= MaxTimeoutCountBeforeDisconnection)
                            ConnectionLostHandler(); // timeout
                    }

                    // Send heartbeat if needed
                    TimeSpan delayFromPreviousHeartbeat = DateTime.Now - _proxy.LastActionToServer;
                    if (delayFromPreviousHeartbeat.TotalMilliseconds > HeartbeatDelay)
                    {
                        _proxy.Heartbeat(this);
                    }

                    // Stop task if stop event is raised
                    if (_stopBackgroundTaskEvent.WaitOne(10))
                        break;
                }
            }
        }
    }
}
