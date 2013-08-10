using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
//using IPFiltering;
using TetriNET.Common;
using TetriNET.Common.WCF;

namespace TetriNET.Server
{
    //[ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Single)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    internal class GameServer : ITetriNET
    {
        private const int MaxPlayerCount = 6;
        private const int MaxSpam = 5;
        private const int SpamThreshold = 50;

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

        private readonly ThreadSafeSingleton<TetriminoQueue> _tetriminoQueue = new ThreadSafeSingleton<TetriminoQueue>(() => new TetriminoQueue());
        public States State { get; private set; }
        private ServiceHost Host { get; set; }

        public GameServer()
        {
            Log.WriteLine("***GameServer:ctor***");

            State = States.WaitingStartServer;
            
            AttackId = 0;

            Task.Factory.StartNew(TaskResolveActions);
        }

        public void StartService()
        {
            Log.WriteLine("StartService");
            if (State != States.WaitingStartServer)
                return; // TODO: error
            
            State = States.StartingServer;

            string port = ConfigurationManager.AppSettings["port"];
            Uri baseAddress;
            if (String.IsNullOrEmpty(port) || port.ToLower() == "auto")
                baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;
            else
                baseAddress =  new Uri("net.tcp://localhost:"+port+"/TetriNET");

            //Host = new ServiceHost(typeof(GameServer), baseAddress);
            Host = new ServiceHost(this, baseAddress);
            Host.AddServiceEndpoint(typeof(ITetriNET), new NetTcpBinding(SecurityMode.None), "");
            //Host.Description.Behaviors.Add(new IPFilterServiceBehavior("DenyLocal"));
            Host.Open();

            foreach (var endpt in Host.Description.Endpoints)
            {
                Log.WriteLine("Enpoint address:\t{0}", endpt.Address);
                Log.WriteLine("Enpoint binding:\t{0}", endpt.Binding);
                Log.WriteLine("Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
            }
            
            State = States.WaitingStartGame;
        }

        public void StopService()
        {
            Log.WriteLine("StopService");

            State = States.StoppingServer;

            foreach (Player p in _players.Where(p => p != null))
                p.Callback.OnServerStopped();

            // Close service host
            Host.Close();

            State = States.WaitingStartServer;
        }

        public void StartGame()
        {
            Log.WriteLine("Start game");
            // Reset Tetrimino Queue
            _tetriminoQueue.Instance.Reset(); // TODO: random seed
            Tetriminos firstTetrimino = _tetriminoQueue.Instance[0];
            Tetriminos secondTetrimino = _tetriminoQueue.Instance[1];
            // Send start game to every connected player
            foreach (Player p in _players.Where(p => p != null))
            {
                p.TetriminoIndex = 0;
                p.Callback.OnGameStarted(firstTetrimino, secondTetrimino);
            }
        }

        public void StopGame()
        {
            Log.WriteLine("Stop game");
            // Send start game to every connected player
            foreach (Player p in _players.Where(p => p != null))
                p.Callback.OnGameFinished();
        }

        #region ITetriNET

        public void RegisterPlayer(string playerName)
        {
            Log.WriteLine("RegisterPlayer:" + playerName);

            ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();

            // Get first empty slot
            int emptySlot = GetEmptySlot(playerName, callback);
            if (emptySlot >= 0)
            {
                // Save player
                Player player = SetPlayer(emptySlot, playerName, callback);
                // Send playerId to player
                if (player != null)
                    player.Callback.OnPlayerRegistered(true, emptySlot);
                // Inform players
                // Send register message to players
                foreach (Player p in _players.Where(p => p != null))
                    p.Callback.OnPublishServerMessage(playerName + "[" + emptySlot + "] is now connected");
                //
                Log.WriteLine("New player:[" + emptySlot + "] " + playerName);
            }
            else
            {
                Log.WriteLine("Register failed for player " + playerName);
                callback.OnPlayerRegistered(false, -1);
            }
        }

        public void PublishMessage(string msg)
        {
            ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
            Player player = GetPlayer(callback);
            if (player != null)
            {
                Log.WriteLine("PublishMessage:[" + player.Name + "]:" + msg);

                bool isSpammer = player.CheckSpam();
                foreach (Player p in _players.Where(p => p != null))
                    p.Callback.OnPublishPlayerMessage(player.Name, msg);
            }
            else
            {
                RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                Log.WriteLine("PublishMessage from unknown player[" + (clientEndpoint == null ? callback.ToString() : clientEndpoint.Address) + "]");
            }
        }

        public void PlaceTetrimino(Tetriminos tetrimino, Orientations orientation, Position position)
        {
            ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
            Player player = GetPlayer(callback);
            Debug.Assert(player == GetPlayer(OperationContext.Current.GetCallbackChannel<ITetriNETCallback>()));
            if (player != null)
            {
                Log.WriteLine("PlaceTetrimino:[" + player.Name + "]" + tetrimino + " " + orientation + " at " + position.X + "," + position.Y);

                bool isSpammer = player.CheckSpam();
                _actionQueue.Enqueue(() => PlaceTetrimino(player, tetrimino, orientation, position));
            }
            else
            {
                RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                Log.WriteLine("PlaceTetrimino from unknown player[" + (clientEndpoint == null ? callback.ToString() : clientEndpoint.Address) + "]");
            }
        }

        public void SendAttack(int targetId, Attacks attack)
        {
            ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
            Player player = GetPlayer(callback);
            Debug.Assert(player == GetPlayer(OperationContext.Current.GetCallbackChannel<ITetriNETCallback>()));
            if (player != null)
            {
                bool isSpammer = player.CheckSpam();
                Player target = GetPlayer(targetId);
                if (target != null)
                {
                    Log.WriteLine("SendAttack:[" + player.Name + "] -> [" + targetId + "]:" + attack);

                    _actionQueue.Enqueue(() => Attack(player, target, attack));
                }
                else
                    Log.WriteLine("SendAttack to unknown player[" + targetId + "] from [" + player.Name + "]");
            }
            else
            {
                RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                Log.WriteLine("SendAttack from unknown player[" + (clientEndpoint == null ? callback.ToString() : clientEndpoint.Address) + "]");
            }
        }

        #endregion

        #region Attack Id

        public int AttackId
        {
            get;
            private set;
        }

        #endregion

        #region Player

        private readonly Player[] _players = new Player[MaxPlayerCount]; // TODO: replace with an array or dictionary or free list

        private class Player
        {
            public Player(string name, ITetriNETCallback callback)
            {
                Name = name;
                Callback = callback;
                TetriminoIndex = 0;
                SpamCount = 0;
                LastActionTime = DateTime.Now;
            }

            public string Name { get; private set; }
            public ITetriNETCallback Callback { get; private set; }
            private DateTime LastActionTime { get; set; }
            private int SpamCount { get; set; }
            public int TetriminoIndex { get; set; }

            public bool CheckSpam() // returns true if spam is detected
            {
                if (SpamCount >= MaxSpam)
                    return true;
                TimeSpan timeSpan = DateTime.Now - LastActionTime;
                LastActionTime = DateTime.Now;
                if (timeSpan.TotalMilliseconds <= SpamThreshold)
                    SpamCount++;
                return SpamCount >= MaxSpam;
            }
        }

        private Player SetPlayer(int playerId, string playerName, ITetriNETCallback callback)
        {
            Player player = new Player(playerName, callback);
            _players[playerId] = player;
            return player;
        }

        private Player GetPlayer(int playerId)
        {
            Player player = null;
            if (playerId < MaxPlayerCount)
                player = _players[playerId];
            return player;
        }

        private Player GetPlayer(ITetriNETCallback callback)
        {
            return _players.FirstOrDefault(p => p != null && p.Callback == callback);
        }

        private int GetEmptySlot(string playerName, ITetriNETCallback callback)
        {
            // player already registered
            if (_players.Any(x => x != null && (x.Name == playerName || x.Callback == callback)))
                return -1;
            // get first empty slot
            int emptySlot = -1;
            for (int i = 0; i < MaxPlayerCount; i++)
                if (_players[i] == null)
                {
                    emptySlot = i;
                    break;
                }
            return emptySlot;
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
                _tetriminosCount = Enum.GetValues(typeof (Tetriminos)).Length;
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
                        action();
                }
                Thread.Sleep(0);
                // TODO: break
            }
        }

        #endregion

        #region Actions
        private void Attack(Player player, Player target, Attacks attack)
        {
            Log.WriteLine("SendAttack[" + player.Name + "][" + target.Name + "]" + attack);

            // Store attack id locally
            int attackId = AttackId;
            AttackId++;
            // Send attack to target
            target.Callback.OnAttackReceived(attack);
            // Send attack message to players
            string attackString = attackId + ": " + attack + " from " + player.Name + " to " + target.Name;
            foreach (Player p in _players.Where(p => p != null))
                p.Callback.OnAttackMessageReceived(attackString);
        }

        private void PlaceTetrimino(Player player, Tetriminos tetrimino, Orientations orientation, Position position)
        {
            Log.WriteLine("PlaceTetrimino[" + player.Name + "]" + tetrimino + " " + orientation + " at " + position.X + "," + position.Y);
            player.TetriminoIndex++;
            Tetriminos nextTetrimino = _tetriminoQueue.Instance[player.TetriminoIndex];
            player.Callback.OnNextTetrimino(player.TetriminoIndex, nextTetrimino);
        }
        #endregion
    }
}