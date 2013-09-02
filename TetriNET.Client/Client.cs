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
using TetriNET.Common.Interfaces;

namespace TetriNET.Client
{
    public class Player
    {
        public string Name { get; set; }
        public IBoard Board { get; set; }
        // Status: Joined, Playing, Lost
    }

    public class TetriminoArray
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
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const int TetriminosCount = 7;

        public enum States
        {
            Created, // -> Registering
            Registering, // --> Registered
            Registered, // --> Playing
            Playing, // --> Paused | Registered
            Paused // --> Playing | Registered
        }

        private readonly IProxy _proxy;
        private readonly Random _random;
        private readonly Func<Tetriminos, int, int, int, ITetrimino> _createTetriminoFunc;
        private readonly Func<IBoard> _createBoardFunc;
        private readonly Player[] _players = new Player[6];
        private readonly ManualResetEvent _stopBackgroundTaskEvent = new ManualResetEvent(false);
        private readonly TetriminoArray _tetriminos;
        private readonly System.Timers.Timer _gameTimer;

        public States State { get; private set; }
        public ITetrimino CurrentTetrimino { get; private set; }
        public ITetrimino NextTetrimino { get; private set; }

        private List<WinEntry> _winList;
        private GameOptions _options;
        private int _playerId;
        private bool _isServerMaster;

        private DateTime _lastActionFromServer;
        private int _timeoutCount;

        private int _tetriminoIndex;
        private int _lineCount;
        private int _level;

        public IBoard Board
        {
            get { return Player.Board; }
        }

        private Player Player
        {
            get { return _players[_playerId]; }
        }

        public string Name { get; private set; }

        public Client(Func<ITetriNETCallback, IProxy> createProxyFunc, Func<Tetriminos, int, int, int, ITetrimino> createTetriminoFunc, Func<IBoard> createBoardFunc)
        {
            if (createProxyFunc == null)
                throw new ArgumentNullException("createProxyFunc");
            if (createTetriminoFunc == null)
                throw new ArgumentNullException("createTetriminoFunc");

            _proxy = createProxyFunc(this);
            _proxy.OnConnectionLost += ConnectionLostHandler;

            _createTetriminoFunc = createTetriminoFunc;
            _createBoardFunc = createBoardFunc;

            _random = new Random();

            _tetriminos = new TetriminoArray(64);

            _gameTimer = new System.Timers.Timer();
            _gameTimer.Interval = 200; // TODO: use a function to compute interval in function of level
            _gameTimer.Elapsed += GameTimerOnElapsed;

            _lastActionFromServer = DateTime.Now;
            _timeoutCount = 0;

            _playerId = -1;
            _isServerMaster = false;

            State = States.Created;

            Task.Factory.StartNew(TimeoutTask);
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
            ResetTimeout();
        }

        public void OnServerStopped()
        {
            ResetTimeout();
            ConnectionLostHandler();
        }

        public void OnPlayerRegistered(bool succeeded, int playerId, bool gameStarted)
        {
            ResetTimeout();
            if (succeeded && State == States.Registering)
            {
                Log.WriteLine("Registered as player {0} game started {1}", playerId, gameStarted);

                _playerId = playerId;
                _players[_playerId] = new Player
                    {
                        Name = Name,
                        Board = _createBoardFunc()
                    };
                if (gameStarted)
                {
                    _players[_playerId].Board.FillWithRandomCells();

                    if (ClientOnRedraw != null)
                        ClientOnRedraw();
                }
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

            ResetTimeout();
            if (playerId != _playerId && playerId >= 0)
            {
                // don't update ourself
                _players[playerId] = new Player
                {
                    Name = name,
                    Board = _createBoardFunc()
                };
                if (IsGameStarted)
                {
                    _players[_playerId].Board.FillWithRandomCells();

                    if (ClientOnRedrawBoard != null)
                        ClientOnRedrawBoard(_playerId);
                }
            }
            // TODO: update chat list + display msg in out-game chat
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            Log.WriteLine("Player {0}[{1}] left ({2})", name, playerId, reason);

            ResetTimeout();
            if (playerId != _playerId && playerId >= 0)
                _players[playerId] = null;
            // TODO: update chat list + display msg in out-game chat
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            Log.WriteLine("{0}:{1}", playerName, msg);

            ResetTimeout();
            // TODO: display msg in out-game chat
        }

        public void OnPublishServerMessage(string msg)
        {
            Log.WriteLine("{0}", msg);

            ResetTimeout();
            // TODO: display msg in out-game chat
        }

        public void OnPlayerLost(int playerId)
        {
            Log.WriteLine("Player [{0}] {1} has lost", playerId, _players[playerId].Name);

            ResetTimeout();
            // TODO: fill player screen with random blocks + display msg in in-game chat + update player status
        }

        public void OnPlayerWon(int playerId)
        {
            Log.WriteLine("Player [{0}] {1} has won", playerId, _players[playerId].Name);

            ResetTimeout();
            // TODO: display msg in in-game chat + out-game chat + update player status
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, Tetriminos thirdTetrimino, GameOptions options)
        {
            Log.WriteLine("Game started with {0} {1} {2}", firstTetrimino, secondTetrimino, thirdTetrimino);

            ResetTimeout();
            State = States.Playing;
            _options = options;
            //
            _tetriminos[0] = firstTetrimino;
            _tetriminos[1] = secondTetrimino;
            _tetriminos[2] = thirdTetrimino;
            _tetriminoIndex = 0;
            CurrentTetrimino = _createTetriminoFunc(firstTetrimino, Board.TetriminoSpawnX, Board.TetriminoSpawnY, 1);
            NextTetrimino = _createTetriminoFunc(secondTetrimino, Board.TetriminoSpawnX, Board.TetriminoSpawnY, 1);
            //
            _lineCount = 0;
            _level = 0;
            // Reset boards
            for (int i = 0; i < 6; i++)
                if (_players[i] != null && _players[i].Board != null)
                    _players[i].Board.Clear();
            // Restart timer
            _gameTimer.Start();

            if (ClientOnGameStarted != null)
                ClientOnGameStarted();

            //Log.WriteLine("TETRIMINOS:{0}", _tetriminos.Dump(8));
        }

        public void OnGameFinished()
        {
            Log.WriteLine("Game finished");

            ResetTimeout();
            State = States.Registered;
            _gameTimer.Stop();
            // TODO:
            if (ClientOnGameFinished != null)
                ClientOnGameFinished();
        }

        public void OnGamePaused()
        {
            Log.WriteLine("Game paused");

            ResetTimeout();
            State = States.Paused;
            // TODO
            if (ClientOnGamePaused != null)
                ClientOnGamePaused();
        }

        public void OnGameResumed()
        {
            Log.WriteLine("Game resumed");

            ResetTimeout();
            State = States.Playing;
            // TODO
            if (ClientOnGameResumed != null)
                ClientOnGameResumed();
        }

        public void OnServerAddLines(int lineCount)
        {
            Log.WriteLine("Server add {0} lines", lineCount);

            ResetTimeout();
            AddLines(lineCount);
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            Log.WriteLine("Player {0} add {1} lines (special [{2}])", playerId, lineCount, specialId);

            ResetTimeout();
            if (playerId == _playerId) // Perform attack only on ourself
                AddLines(lineCount);
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            Log.WriteLine("Special {0}[{1}] from {2} to {3}", special, specialId, playerId, targetId);

            ResetTimeout();
            // TODO: if targetId == own id, perform attack + display in-game msg
            if (targetId == _playerId)
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
                }
            }
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            ResetTimeout();
            _tetriminos[index] = tetrimino;
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            Log.WriteLine("Player [{0}] {1} modified", playerId, _players[playerId].Name);

            ResetTimeout();
            if (_players[playerId] != null)
                _players[playerId].Board.SetCells(grid);
            
            if (ClientOnRedrawBoard != null)
                ClientOnRedrawBoard(playerId);
        }

        public void OnServerMasterChanged(int playerId)
        {
            ResetTimeout();
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

            ResetTimeout();
            _winList = winList;
            // TODO: update win list
        }

        #endregion

        public void ResetTimeout()
        {
            _timeoutCount = 0;
            _lastActionFromServer = DateTime.Now;
        }

        public void SetTimeout()
        {
            _timeoutCount++;
            _lastActionFromServer = DateTime.Now;
        }

        private void PlaceCurrentTetrimino()
        {
            Board.CommitPiece(CurrentTetrimino);
        }

        private void EndGame()
        {
            Log.WriteLine("End game");
            _gameTimer.Stop();
            // Send player lost
            _proxy.GameLost(this);

            if (ClientOnGameOver != null)
                ClientOnGameOver();
        }

        private void FinishRound()
        {
            //
            //Log.WriteLine("Round finished with tetrimino {0} {1}  next {2}", CurrentTetrimino.TetriminoValue, _tetriminoIndex, NextTetrimino.TetriminoValue);
            // Stop game
            _gameTimer.Stop();
            //Log.WriteLine("TETRIMINOS:{0}", _tetriminos.Dump(8));
            // Delete rows
            int deletedRows = DeleteRows();
            _lineCount += deletedRows;
            if (deletedRows > 0)
                Log.WriteLine("{0} lines deleted -> total {1}", deletedRows, _lineCount);
            // Check level increase
            if (_level < _lineCount/10)
            {
                _level = _lineCount/10;
                Log.WriteLine("Level increased: {0}", _level);
                // TODO: change game timer interval
            }
            // Send tetrimino places to server
            _proxy.PlaceTetrimino(this, _tetriminoIndex, (Tetriminos)CurrentTetrimino.Color/*TODO handle this in a better way*/, CurrentTetrimino.Orientation, CurrentTetrimino.PosX, CurrentTetrimino.PosY, Board.Cells);
            // Set new current tetrimino to next, increment tetrimino index and create next tetrimino
            CurrentTetrimino = NextTetrimino;
            _tetriminoIndex++;
            NextTetrimino = _createTetriminoFunc(_tetriminos[_tetriminoIndex + 1], Board.TetriminoSpawnX, Board.TetriminoSpawnY, 1);
            //Log.WriteLine("New tetrimino {0} {1}  next {2}", CurrentTetrimino.TetriminoValue, _tetriminoIndex, NextTetrimino.TetriminoValue);
            // Check game over (if current tetrimino has conflict)
            if (!Board.CheckNoConflict(CurrentTetrimino))
                EndGame();
            else
            {
                // Inform UI
                if (ClientOnRedraw != null)
                    ClientOnRedraw();

                // TODO: transform cell into special blocks

                // Send lines if classic style
                if (_options.ClassicStyleMultiplayerRules && deletedRows > 1)
                {
                    int addLines = deletedRows - 1;
                    if (deletedRows == 4) // special case for Tetris
                        addLines = 4;
                    _proxy.SendLines(this, addLines);
                }

                // Start round
                _gameTimer.Start();

                if (ClientOnTetriminoPlaced != null)
                    ClientOnTetriminoPlaced();
            }
        }

        private int DeleteRows()
        {
            return Board.CollapseCompletedRows();
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
        public void AddLines(int count)
        {
            if (count <= 0)
                return;
            Board.AddLines(count);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        public void ClearLine()
        {
            Board.ClearLine();

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        public void NukeField()
        {
            Board.NukeField();

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        public void RandomBlocksClear(int count)
        {
            Board.RandomBlocksClear(count);

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        public void ClearSpecialBlocks()
        {
            Board.ClearSpecialBlocks();

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        public void BlockGravity()
        {
            Board.BlockGravity();

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        public void BlockQuake()
        {
            Board.BlockQuake();

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        public void BlockBomb()
        {
            Board.BlockBomb();

            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        // special Switch is handled by server
        #endregion

        #region IClient

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

        private event ClientTetriminoPlacedHandler ClientOnTetriminoPlaced;
        event ClientTetriminoPlacedHandler IClient.OnTetriminoPlaced
        {
            add { ClientOnTetriminoPlaced += value; }
            remove { ClientOnTetriminoPlaced -= value; }
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

        public IBoard GetBoard(int playerId)
        {
            return _players[playerId].Board;
        }

        public void Dump()
        {
            // Players
            for (int i = 0; i < 6; i++)
            {
                Player p = _players[i];
                Log.WriteLine("{0}{1}: {2}", i, i == _playerId ? "*" : String.Empty, p == null ? String.Empty : p.Name);
            }
            // Board
            if (_playerId >= 0 && State == States.Playing)
            {
                for (int y = Board.Height; y >= 1; y--)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int x = 1; x <= Board.Width; x++)
                        sb.Append(Board[x, y]);
                    Log.WriteLine(sb.ToString());
                }
                Log.WriteLine("========================");
            }
            // TODO: current & next tetrimino
        }

        public void Register(string name)
        {
            State = States.Registering;
            Name = name;
            _proxy.RegisterPlayer(this, Name);
        }

        public void Drop()
        {
            if (State != States.Playing)
                return;
            _gameTimer.Stop();
            //
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            // Perform move
            Board.Drop(CurrentTetrimino);
            //
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
            //
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            //
            bool movedDown = Board.MoveDown(CurrentTetrimino);
            //
            if (ClientOnTetriminoMoved != null)
                ClientOnTetriminoMoved();

            if (!movedDown)
            {
                //
                PlaceCurrentTetrimino();
                //
                FinishRound();
            }

            // Inform UI
            if (ClientOnRedraw != null)
                ClientOnRedraw();
        }

        public void MoveLeft()
        {
            if (State != States.Playing)
                return;
            //
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            //
            Board.MoveLeft(CurrentTetrimino);
            //
            if (ClientOnTetriminoMoved != null)
                ClientOnTetriminoMoved();

            // Inform UI
            if (ClientOnRedraw != null)
                ClientOnRedraw();

        }

        public void MoveRight()
        {
            if (State != States.Playing)
                return;
            //
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            //
            Board.MoveRight(CurrentTetrimino);
            //
            if (ClientOnTetriminoMoved != null)
                ClientOnTetriminoMoved();

            // Inform UI
            if (ClientOnRedraw != null)
                ClientOnRedraw();

        }

        public void RotateClockwise()
        {
            if (State != States.Playing)
                return;
            //
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            //
            Board.RotateClockwise(CurrentTetrimino);
            //
            if (ClientOnTetriminoMoved != null)
                ClientOnTetriminoMoved();

            // Inform UI
            if (ClientOnRedraw != null)
                ClientOnRedraw();

        }

        public void RotateCounterClockwise()
        {
            if (State != States.Playing)
                return;
            //
            if (ClientOnTetriminoMoving != null)
                ClientOnTetriminoMoving();
            //
            Board.RotateCounterClockwise(CurrentTetrimino);
            //
            if (ClientOnTetriminoMoved != null)
                ClientOnTetriminoMoved();

            // Inform UI
            if (ClientOnRedraw != null)
                ClientOnRedraw();

        }

        #endregion

        private void TimeoutTask()
        {
            while (true)
            {
                if (State == States.Registered || State == States.Playing)
                {
                    // Check server timeout
                    TimeSpan timespan = DateTime.Now - _lastActionFromServer;
                    if (timespan.TotalMilliseconds > TimeoutDelay)
                    {
                        Log.WriteLine("Timeout++");
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
