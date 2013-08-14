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
        private readonly IHost _host;

        public States State { get; private set; }
        public int AttackId { get; private set; }

        public Server(IHost host)
        {
            _host = host;

            _host.OnPlayerRegistered += RegisterPlayerHandler;
            _host.OnPlayerUnregistered += UnregisterPlayerHandler;
            _host.OnMessagePublished += PublishMessageHandler;
            _host.OnTetriminoPlaced += PlaceTetriminoHandler;
            _host.OnAttackSent += SendAttackHandler;

            AttackId = 0;

            Task.Factory.StartNew(TaskResolveActions);

            State = States.WaitingStartServer;
        }

        // TODO: remove following region
        #region TEST METHODS - TO REMOVE
        public void BroadcastRandomMessage()
        {
            // Send start game to every connected player
            foreach (IPlayer p in _host.PlayerManager.Players)
                p.OnPublishServerMessage("Random message");
        }
        #endregion

        public void StartServer()
        {
            Log.WriteLine("Starting server");

            State = States.StartingServer;

            string port = ConfigurationManager.AppSettings["port"];

            _host.Start(port);

            State = States.WaitingStartGame;

            Log.WriteLine("Server started");
        }

        public void StopServer()
        {
            Log.WriteLine("Stopping server");
            State = States.StoppingServer;

            _host.Stop();

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
            List<PlayerData> players = _host.PlayerManager.Players.Select(x => new PlayerData
            {
                Id = _host.PlayerManager.GetId(x),
                Name = x.Name
            }
                ).ToList();
            // Send start game to every connected player
            foreach (IPlayer p in _host.PlayerManager.Players)
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
            foreach (IPlayer p in _host.PlayerManager.Players)
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
            foreach (IPlayer p in _host.PlayerManager.Players)
                p.OnPublishServerMessage(String.Format("{0} is now connected", player.Name));
        }

        private void UnregisterPlayerHandler(IPlayer player)
        {
            Log.WriteLine("Player disconnected:{0}", player.Name);

            // Inform players
            foreach (IPlayer p in _host.PlayerManager.Players)
                p.OnPublishServerMessage(String.Format("{0} has disconnected", player.Name));
        }

        private void PublishMessageHandler(IPlayer player, string msg)
        {
            Log.WriteLine("PublishMessage:[{0}]:{1}", player.Name, msg);

            // Send message to players
            foreach (IPlayer p in _host.PlayerManager.Players)
                p.OnPublishPlayerMessage(player.Name, msg);
        }

        private void PlaceTetriminoHandler(IPlayer player, Tetriminos tetrimino, Orientations orientation, Position position)
        {
            _actionQueue.Enqueue(() => PlaceTetrimino(player, tetrimino, orientation, position));
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
                            Log.WriteLine("Exception raised in TaskResolveActions. Exception:" + ex.ToString());
                        }
                    }
                }
                Thread.Sleep(0);
                foreach (IPlayer p in _host.PlayerManager.Players)
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

        private void PlaceTetrimino(IPlayer player, Tetriminos tetrimino, Orientations orientation, Position position)
        {
            Log.WriteLine("PlaceTetrimino[{0}]{1} {2} at {3},{4}", player.Name, tetrimino, orientation, position.X, position.Y);

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
            foreach (IPlayer p in _host.PlayerManager.Players)
                p.OnAttackMessageReceived(attackString);
        }

        #endregion
    }
}
