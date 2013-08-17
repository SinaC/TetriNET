using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common;

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

        private GameOptions _options;
        
        public States State { get; private set; }
        public int AttackId { get; private set; }

        public Server(IPlayerManager playerManager, params IHost[] hosts)
        {
            if (hosts == null)
                throw new ArgumentNullException("hosts");

            _playerManager = playerManager;
            _hosts = hosts.ToList();
            _options = new GameOptions();

            foreach (IHost host in _hosts)
            {
                host.OnPlayerRegistered += RegisterPlayerHandler;
                host.OnPlayerUnregistered += UnregisterPlayerHandler;
                host.OnMessagePublished += PublishMessageHandler;
                host.OnTetriminoPlaced += PlaceTetriminoHandler;
                host.OnSendAttack += SendAttackHandler;
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
            }

            AttackId = 0;

            Task.Factory.StartNew(TaskResolveGameActions);

            State = States.WaitingStartServer;
        }

        // TODO: remove following region
        #region TEST METHODS - TO REMOVE
        public void BroadcastRandomMessage()
        {
            // Send start game to every connected player
            foreach (IPlayer p in _playerManager.Players)
                p.OnPublishServerMessage("Random message");
        }
        #endregion

        // TODO: set following methods as private
        public void StartServer()
        {
            Log.WriteLine("Starting server");

            State = States.StartingServer;

            foreach (IHost host in _hosts)
                host.Start();

            State = States.WaitingStartGame;

            Log.WriteLine("Server started");
        }

        public void StopServer()
        {
            Log.WriteLine("Stopping server");
            State = States.StoppingServer;

            // Warn players
            foreach(IPlayer p in _playerManager.Players)
                p.OnServerStopped();

            foreach (IHost host in _hosts)
                host.Stop();

            // TODO: clear PlayerManager and other object storing player/callback/...

            State = States.WaitingStartServer;

            Log.WriteLine("Server stopped");
        }

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

        public void PauseGame()
        {
            Log.WriteLine("Pausing game");

            if (State == States.GameStarted)
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

        public void ResumeGame()
        {
            Log.WriteLine("Resuming game");

            if (State == States.GamePaused)
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

        public void ChangeOptions(GameOptions options)
        {
            Log.WriteLine("Changing options");

            if (State == States.WaitingStartGame)
            {
                _options = options; // Options will be sent to players when starting a new game
            }
            else
                Log.WriteLine("Cannot change options");
        }

        public void KickPlayer(int playerId)
        {
            Log.WriteLine("Kick player");

            if (State == States.WaitingStartGame)
            {
                // TODO: remove player from PlayerManager, send ServerStopped, warn other players
                Log.WriteLine("Not yet implemented");
            }
            else
                Log.WriteLine("Cannot kick player");
        }

        public void BanPlayer(int playerId)
        {
            Log.WriteLine("Ban player");

            if (State == States.WaitingStartGame)
            {
                // TODO: remove player from PlayerManager, send ServerStopped, add to BanList, warn other players
                Log.WriteLine("Not yet implemented");
            }
            else
                Log.WriteLine("Cannot ban player");
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
            if (serverMaster != null)
            {
                int serverMasterId = _playerManager.GetId(serverMaster);
                foreach (IPlayer p in _playerManager.Players)
                    p.OnServerMasterChanged(serverMasterId);
            }

        }

        private void UnregisterPlayerHandler(IPlayer player, int playerId)
        {
            Log.WriteLine("Player disconnected:[{0}]{1}", playerId, player.Name);

            // Inform players except disconnected player
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnPlayerLeft(playerId, player.Name, LeaveReasons.ConnectionLost); // TODO: set real reasons

            // Send new server master id
            IPlayer serverMaster = _playerManager.ServerMaster;
            if (serverMaster != null)
            {
                int serverMasterId = _playerManager.GetId(serverMaster);
                foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                    p.OnServerMasterChanged(serverMasterId);
            }
        }

        private void PublishMessageHandler(IPlayer player, string msg)
        {
            Log.WriteLine("PublishMessage:{0}:{1}", player.Name, msg);

            // Send message to players
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnPublishPlayerMessage(player.Name, msg);
        }

        private void PlaceTetriminoHandler(IPlayer player, int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid)
        {
            _actionQueue.Enqueue(() => PlaceTetrimino(player, index, tetrimino, orientation, position, grid));
        }

        private void SendAttackHandler(IPlayer player, IPlayer target, Attacks attack)
        {
            _actionQueue.Enqueue(() => Attack(player, target, attack));
        }

        private void SendLinesHandler(IPlayer player, int count)
        {
            _actionQueue.Enqueue(() => SendLines(player, count));
        }

        private void ModifyGridHandler(IPlayer player, PlayerGrid grid)
        {
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
            if (masterPlayer == player)
                PauseGame();
        }

        private void ResumeGameHandler(IPlayer player)
        {
            Log.WriteLine("ResumeGame:{0}", player.Name);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
                ResumeGame();
        }

        private void GameLostHandler(IPlayer player)
        {
            Log.WriteLine("GameLost:{0}", player.Name);

            // TODO: 
            //  check if there is only one playing player -> winner
            //  keep lost order

            // Set player state
            player.State = PlayerStates.GameLost;
            
            // Inform players
            int id = _playerManager.GetId(player);
            foreach(IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnPlayerLost(id);
        }

        private void ChangeOptionsHandler(IPlayer player, GameOptions options)
        {
            Log.WriteLine("ChangeOptionsHandler:{0} {1}", player.Name, options);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
                ChangeOptions(options);
        }

        private void KickPlayerHandler(IPlayer player, int playerId)
        {
            Log.WriteLine("KickPlayer:{0} [{1}]", player.Name, playerId);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
                KickPlayer(playerId);
        }

        private void BanPlayerHandler(IPlayer player, int playerId)
        {
            Log.WriteLine("BanPlayer:{0} [{1}]", player.Name, playerId);

            IPlayer masterPlayer = _playerManager.ServerMaster;
            if (masterPlayer == player)
                BanPlayer(playerId);
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
                foreach (IPlayer p in _playerManager.Players)
                {
                    TimeSpan timespan = DateTime.Now - p.LastAction;
                    if (timespan.TotalMilliseconds > InactivityTimeoutBeforePing)
                    {
                        p.OnHeartbeatReceived(); // Check if player is alive
                        break; // TODO: use round-robin to avoid checking first players before last players
                    }
                }
                if (_stopActionTaskEvent.WaitOne(0))
                    break;
            }
        }
        
        #endregion

        #region Game actions

        private void PlaceTetrimino(IPlayer player, int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid)
        {
            Log.WriteLine("PlaceTetrimino[{0}]{1}:{2} {3} at {4},{5}", player.Name, index, tetrimino, orientation, position.X, position.Y);
            Log.WriteLine("Grid non-empty cell count: {0}", grid.Data.Count(x => x > 0));

            // TODO: check if index is equal to player.TetriminoIndex

            // Set grid
            player.Grid = grid;
            // Get next piece
            player.TetriminoIndex++;
            Tetriminos nextTetrimino = _tetriminoQueue[player.TetriminoIndex];
            // Send next piece
            player.OnNextTetrimino(player.TetriminoIndex, nextTetrimino);
        }

        private void Attack(IPlayer player, IPlayer target, Attacks attack)
        {
            Log.WriteLine("SendAttack[{0}][{1}]{2}", player.Name, target.Name, attack);

            // Store attack id locally
            int attackId = AttackId;
            // Increment attack
            AttackId++;
            // Send attack to target
            target.OnAttackReceived(attackId, attack);
            // If attack was a switch, call OnGridModified with switched grids
            if (attack == Attacks.Switch)
            {
                // Save player data
                PlayerGrid playerGrid = new PlayerGrid
                {
                    Height = player.Grid.Height,
                    Width = player.Grid.Width,
                };
                Array.Copy(player.Grid.Data, playerGrid.Data, player.Grid.Data.Length);
                // Save target data
                PlayerGrid targetGrid = new PlayerGrid
                {
                    Height = target.Grid.Height,
                    Width = target.Grid.Width,
                };
                Array.Copy(target.Grid.Data, targetGrid.Data, target.Grid.Data.Length);
                // Switch locally
                target.Grid = playerGrid;
                player.Grid = targetGrid;
                // Send copies to everyone
                int playerId = _playerManager.GetId(player);
                int targetId = _playerManager.GetId(target);
                foreach (IPlayer p in _playerManager.Players)
                {
                    p.OnGridModified(targetId, targetGrid);
                    p.OnGridModified(playerId, playerGrid);
                }
            }
            // Send attack message to everyone
            string msg = String.Format("{0}: {1} sends {2} on {3}", attackId, player.Name, attack, target.Name);
            foreach(IPlayer p in _playerManager.Players)
                p.OnPublishAttackMessage(msg);
        }

        private void SendLines(IPlayer player, int count)
        {
            Log.WriteLine("SendLines[{0}]:{1}", player, count);

            // Store attack id locally
            int attackId = AttackId;
            // Increment attack
            AttackId++;
            // Send lines to everyone except sender
            foreach (IPlayer p in _playerManager.Players.Where(x => x != player))
                p.OnPlayerAddLines(attackId, count);
        }

        private void ModifyGrid(IPlayer player, PlayerGrid grid)
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
