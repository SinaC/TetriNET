using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class Server
    {
        public enum States
        {
            WaitingStartServer, // -> StartingServer
            StartingServer, // -> WaitingStartGame
            WaitingStartGame, // -> StartingGame
            StartingGame, // -> GameStarted
            GameStarted, // -> GameFinished
            GameFinished, // -> WaitingStartGame
            StoppingServer, // -> WaitingStartServer
        }

        private const int InactivityTimeoutBeforePing = 500; // in ms

        private readonly Singleton<TetriminoQueue> _tetriminoQueue = new Singleton<TetriminoQueue>(() => new TetriminoQueue());
        private readonly IPlayerManager _playerManager;
        private readonly List<IHost> _hosts;

        public States State { get; private set; }
        public int AttackId { get; private set; }

        public Server(IPlayerManager playerManager, params IHost[] hosts)
        {
            if (hosts == null)
                throw new ArgumentNullException("hosts");

            _playerManager = playerManager;
            _hosts = hosts.ToList();

            foreach (IHost host in _hosts)
            {
                host.OnPlayerRegistered += RegisterPlayerHandler;
                host.OnPlayerUnregistered += UnregisterPlayerHandler;
                host.OnMessagePublished += PublishMessageHandler;
                host.OnTetriminoPlaced += PlaceTetriminoHandler;
                host.OnAttackSent += SendAttackHandler;
            }

            AttackId = 0;

            Task.Factory.StartNew(TaskResolveActions);

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
            foreach (IHost host in _hosts)
                host.Stop();

            State = States.WaitingStartServer;

            Log.WriteLine("Server stopped");
        }

        public void StartGame()
        {
            Log.WriteLine("Starting game");

            State = States.GameStarted;

            // Reset Tetrimino Queue
            _tetriminoQueue.Instance.Reset(); // TODO: random seed
            Tetriminos firstTetrimino = _tetriminoQueue.Instance[0];
            Tetriminos secondTetrimino = _tetriminoQueue.Instance[1];
            // Build player list
            List<PlayerData> players = _playerManager.Players.Select(x => new PlayerData
            {
                Id = _playerManager.GetId(x),
                Name = x.Name
            }
                ).ToList();
            // Send start game to every connected player
            foreach (IPlayer p in _playerManager.Players)
            {
                p.TetriminoIndex = 0;
                p.OnGameStarted(firstTetrimino, secondTetrimino, players);
            }

            Log.WriteLine("Game started");
        }

        public void StopGame()
        {
            Log.WriteLine("Stopping game");

            State = States.GameFinished;

            // Send start game to every connected player
            foreach (IPlayer p in _playerManager.Players)
                p.OnGameFinished();

            State = States.WaitingStartGame;

            Log.WriteLine("Game stopped");
        }

        private void RegisterPlayerHandler(IPlayer player, int id)
        {
            Log.WriteLine("New player:[{0}][{1}]", id, player.Name);

            // Send id to player
            player.OnPlayerRegistered(true, id);
            // Inform players
            foreach (IPlayer p in _playerManager.Players)
                p.OnPublishServerMessage(String.Format("{0} is now connected", player.Name));
        }

        private void UnregisterPlayerHandler(IPlayer player)
        {
            Log.WriteLine("Player disconnected:{0}", player.Name);

            // Inform players
            foreach (IPlayer p in _playerManager.Players)
                p.OnPublishServerMessage(String.Format("{0} has disconnected", player.Name));
        }

        private void PublishMessageHandler(IPlayer player, string msg)
        {
            Log.WriteLine("PublishMessage:[{0}]:{1}", player.Name, msg);

            // Send message to players
            foreach (IPlayer p in _playerManager.Players)
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
                Grow(1);
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

        #region Action queue

        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

        private void TaskResolveActions()
        {
            while (true)
            {
                if (!_actionQueue.IsEmpty)
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
                            Log.WriteLine("Exception raised in TaskResolveActions. Exception:{0}", ex);
                        }
                    }
                }
                Thread.Sleep(0);
                foreach (IPlayer p in _playerManager.Players)
                {
                    TimeSpan timespan = DateTime.Now - p.LastAction;
                    if (timespan.TotalMilliseconds > InactivityTimeoutBeforePing)
                    {
                        p.OnPingReceived(); // Check if player is alive
                        break; // TODO: use round-robin to avoid checking first players before last players
                    }
                }
                Thread.Sleep(0);
                // TODO: stop event
            }
        }

        #endregion

        #region Actions

        private void PlaceTetrimino(IPlayer player, int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid)
        {
            Log.WriteLine("PlaceTetrimino[{0}]{1}:{2} {3} at {4},{5}", player.Name, index, tetrimino, orientation, position.X, position.Y);
            Log.WriteLine("Grid non-empty cell count: {0}", grid.Data.Count(x => x > 0));

            // TODO: check if index is equal to player.TetriminoIndex

            // Set grid
            player.Grid = grid.Data;
            // Get next piece
            player.TetriminoIndex++;
            Tetriminos nextTetrimino = _tetriminoQueue.Instance[player.TetriminoIndex];
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
            target.Callback.OnAttackReceived(attack);
            // Send attack message to players
            string attackString = String.Format("{0}: {1} from {2} to {3}", attackId, attack, player.Name, target.Name);
            foreach (IPlayer p in _playerManager.Players)
                p.OnAttackMessageReceived(attackString);
        }

        #endregion
    }
}
