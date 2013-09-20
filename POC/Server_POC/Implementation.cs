//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.ServiceModel;
//using System.ServiceModel.Channels;
//using System.Threading;
//using System.Threading.Tasks;
//using TetriNET.Common;
//using TetriNET.Common.WCF;

//namespace POC.Server_POC
//{
//    public class RemotePlayer : IPlayer
//    {
//        public RemotePlayer(string name, ITetriNETCallback callback)
//        {
//            Name = name;
//            Callback = callback;
//            PieceIndex = 0;
//            LastAction = DateTime.Now;
//        }

//        private void ExceptionFreeAction(Action action, string actionName)
//        {
//            try
//            {
//                action();
//                LastAction = DateTime.Now; // if action didn't raise an exception, client is still alive
//            }
//            catch (CommunicationObjectAbortedException)
//            {
//                Log.WriteLine("CommunicationObjectAbortedException:" + actionName);
//                if (OnDisconnected != null)
//                    OnDisconnected(this);
//            }
//        }

//        #region IPlayer

//        public event PlayerDisconnectedHandler OnDisconnected;

//        public string Name { get; private set; }
//        public int PieceIndex { get; set; }
//        public DateTime LastAction { get; set; }
//        public ITetriNETCallback Callback { get; private set; }
        
//        public void OnPingReceived()
//        {
//            ExceptionFreeAction(() => Callback.OnPingReceived(), "OnPingReceived");
//        }

//        public void OnServerStopped()
//        {
//            ExceptionFreeAction(() => Callback.OnServerStopped(), "OnServerStopped");
//        }

//        public void OnPlayerRegistered(bool succeeded, int playerId)
//        {
//            ExceptionFreeAction(() => Callback.OnPlayerRegistered(succeeded, playerId), "OnPlayerRegistered");
//        }

//        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, List<PlayerData> players)
//        {
//            ExceptionFreeAction(() => Callback.OnGameStarted(firstTetrimino, secondTetrimino, players), "OnGameStarted");
//        }

//        public void OnGameFinished()
//        {
//            ExceptionFreeAction(() => Callback.OnGameFinished(), "OnGameFinished");
//        }

//        public void OnServerAddLines(int lineCount)
//        {
//            ExceptionFreeAction(() => Callback.OnServerAddLines(lineCount), "OnServerAddLines");
//        }

//        public void OnPlayerAddLines(int lineCount)
//        {
//            ExceptionFreeAction(() => Callback.OnPlayerAddLines(lineCount), "OnPlayerAddLines");
//        }

//        public void OnPublishPlayerMessage(string playerName, string msg)
//        {
//            ExceptionFreeAction(() => Callback.OnPublishPlayerMessage(playerName, msg), "OnPublishPlayerMessage");
//        }

//        public void OnPublishServerMessage(string msg)
//        {
//            ExceptionFreeAction(() => Callback.OnPublishServerMessage(msg), "OnPublishServerMessage");
//        }

//        public void OnAttackReceived(Attacks attack)
//        {
//            ExceptionFreeAction(() => Callback.OnAttackReceived(attack), "OnAttackReceived");
//        }

//        public void OnAttackMessageReceived(string msg)
//        {
//            ExceptionFreeAction(() => Callback.OnAttackMessageReceived(msg), "OnAttackMessageReceived");
//        }

//        public void OnNextPiece(int index, Tetriminos tetrimino)
//        {
//            ExceptionFreeAction(() => Callback.OnNextPiece(index, tetrimino), "OnNextPiece");
//        }

//        #endregion
//    }

//    public class LocalPlayer : IPlayer
//    {
//        public LocalPlayer(string name, ITetriNETCallback callback)
//        {
//            Name = name;
//            Callback = callback;
//            PieceIndex = 0;
//            LastAction = DateTime.Now;
//        }

//        private void UpdateTimerOnAction(Action action)
//        {
//            action();
//            LastAction = DateTime.Now;
//        }

//        #region IPlayer

//        public event PlayerDisconnectedHandler OnDisconnected;

//        public string Name { get; private set; }
//        public int PieceIndex { get; set; }
//        public DateTime LastAction { get; set; }
//        public ITetriNETCallback Callback { get; private set; }

//        public void OnPingReceived()
//        {
//            UpdateTimerOnAction(Callback.OnPingReceived);
//        }

//        public void OnServerStopped()
//        {
//            UpdateTimerOnAction(Callback.OnServerStopped);
//        }

//        public void OnPlayerRegistered(bool succeeded, int playerId)
//        {
//            UpdateTimerOnAction(() => Callback.OnPlayerRegistered(succeeded, playerId));
//        }

//        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, List<PlayerData> players)
//        {
//            UpdateTimerOnAction(() => Callback.OnGameStarted(firstTetrimino, secondTetrimino, players));
//        }

//        public void OnGameFinished()
//        {
//            UpdateTimerOnAction(Callback.OnGameFinished);
//        }

//        public void OnServerAddLines(int lineCount)
//        {
//            UpdateTimerOnAction(() => Callback.OnServerAddLines(lineCount));
//        }

//        public void OnPlayerAddLines(int lineCount)
//        {
//            UpdateTimerOnAction(() => Callback.OnPlayerAddLines(lineCount));
//        }

//        public void OnPublishPlayerMessage(string playerName, string msg)
//        {
//            UpdateTimerOnAction(() => Callback.OnPublishPlayerMessage(playerName, msg));
//        }

//        public void OnPublishServerMessage(string msg)
//        {
//            UpdateTimerOnAction(() => Callback.OnPublishServerMessage(msg));
//        }

//        public void OnAttackReceived(Attacks attack)
//        {
//            UpdateTimerOnAction(() => Callback.OnAttackReceived(attack));
//        }

//        public void OnAttackMessageReceived(string msg)
//        {
//            UpdateTimerOnAction(() => Callback.OnAttackMessageReceived(msg));
//        }

//        public void OnNextPiece(int index, Tetriminos tetrimino)
//        {
//            UpdateTimerOnAction(() => Callback.OnNextPiece(index, tetrimino));
//        }
        
//        #endregion
//    }

//    public class PlayerManager : IPlayerManager
//    {
//        private readonly IPlayer[] _players;

//        public int MaxPlayers { get; private set; }

//        public PlayerManager(int maxPlayers)
//        {
//            MaxPlayers = maxPlayers;
//            _players = new IPlayer[MaxPlayers];
//        }

//        #region IPlayerManager

//        public int Add(IPlayer player)
//        {
//            bool alreadyExists = _players.Any(x => x != null && (x == player || x.Name == player.Name));
//            if (!alreadyExists)
//            {
//                // insert in first empty slot
//                for (int i = 0; i < MaxPlayers; i++)
//                    if (_players[i] == null)
//                    {
//                        _players[i] = player;
//                        return i;
//                    }
//            }
//            return -1;
//        }

//        public bool Remove(IPlayer player)
//        {
//            for (int i = 0; i < MaxPlayers; i++)
//                if (_players[i] == player)
//                {
//                    _players[i] = null;
//                    return true;
//                }
//            return false;
//        }

//        public int PlayerCount
//        {
//            get
//            {
//                return _players.Count(x => x != null);
//            }
//        }

//        public IEnumerable<IPlayer> Players
//        {
//            get
//            {
//                return _players.Where(x => x != null);
//            }
//        }

//        public int GetId(IPlayer player)
//        {
//            return Array.IndexOf(_players, player);
//        }

//        public IPlayer this[string name]
//        {
//            get
//            {
//                return _players.FirstOrDefault(x => x != null && x.Name == name);
//            }
//        }

//        public IPlayer this[int index]
//        {
//            get
//            {
//                if (index >= MaxPlayers)
//                    return null;
//                return _players[index];
//            }
//        }

//        public IPlayer this[ITetriNETCallback callback]
//        {
//            get
//            {
//                return _players.FirstOrDefault(x => x != null && x.Callback == callback);
//            }
//        }
        
//        #endregion
//    }

//    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
//    public class WCFHost : IHost
//    {
//        private bool Started { get; set; }
//        private ServiceHost ServiceHost { get; set; }
//        private IPlayer LocalPlayer { get; set; }

//        public WCFHost(IPlayerManager playerManager)
//        {
//            PlayerManager = playerManager;
//            Started = false;
//        }

//        public ITetriNETCallback LocalPlayerCallback { private get; set; }

//        #region IHost

//        public event RegisterPlayerHandler OnPlayerRegistered;
//        public event UnregisterPlayerHandler OnPlayerUnregistered;
//        public event PublishMessageHandler OnMessagePublished;
//        public event PlaceTetriminoHandler OnPiecePlaced;
//        public event SendAttackHandler OnAttackSent;

//        public IPlayerManager PlayerManager { get; private set; }

//        public void Start(string port)
//        {
//            if (Started)
//                return;

//            Uri baseAddress;
//            if (String.IsNullOrEmpty(port) || port.ToLower() == "auto")
//                baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;
//            else
//                baseAddress = new Uri("net.tcp://localhost:" + port + "/TetriNET");

//            ServiceHost = new ServiceHost(this, baseAddress);
//            ServiceHost.AddServiceEndpoint(typeof(IWCFTetriNET), new NetTcpBinding(SecurityMode.None), "");
//            //Host.Description.Behaviors.Add(new IPFilterServiceBehavior("DenyLocal"));
//            ServiceHost.Open();

//            foreach (var endpt in ServiceHost.Description.Endpoints)
//            {
//                Log.WriteLine("Enpoint address:\t{0}", endpt.Address);
//                Log.WriteLine("Enpoint binding:\t{0}", endpt.Binding);
//                Log.WriteLine("Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
//            }

//            Started = true;
//        }

//        public void Stop()
//        {
//            if (!Started)
//                return;

//            // Inform players
//            foreach (IPlayer p in PlayerManager.Players)
//                p.OnServerStopped();

//            // Close service host
//            ServiceHost.Close();

//            Started = false;
//        }
        
//        #endregion

//        #region IWCFTetriNET

//        public void RegisterPlayer(string playerName)
//        {
//            Log.WriteLine("RegisterPlayer");

//            ITetriNETCallback callback = Callback;

//            IPlayer player = null;
//            int id = -1;
//            if (callback != null && PlayerManager[playerName] == null && PlayerManager.PlayerCount < PlayerManager.MaxPlayers)
//            {
//                if (callback == LocalPlayerCallback)
//                    // Local player
//                    player = new LocalPlayer(playerName, LocalPlayerCallback);
//                else
//                    // Remote player
//                    player = new RemotePlayer(playerName, callback);
//                id = PlayerManager.Add(player);
//            }
//            if (id >= 0)
//            {
//                // Save local player
//                if (player is LocalPlayer)
//                    LocalPlayer = player;
//                //
//                if (OnPlayerRegistered != null)
//                    OnPlayerRegistered(player, id);
//            }
//            else
//            {
//                Log.WriteLine("Register failed for player " + playerName);
//                if (callback != null)
//                    callback.OnPlayerRegistered(false, -1);
//            }
//        }

//        public void UnregisterPlayer()
//        {
//            Log.WriteLine("UnregisterPlayer");

//            IPlayer player = CallbackPlayer;
//            if (player != null)
//            {
//                // Remove player from player list
//                PlayerManager.Remove(player);
//                //
//                if (OnPlayerUnregistered != null)
//                    OnPlayerUnregistered(player);
//            }
//            else
//            {
//                string endpoint = CallbackEndpoint;
//                Log.WriteLine("UnregisterPlayer from unknown player[" + endpoint + "]");
//            }
//        }

//        public void Ping()
//        {
//            Log.WriteLine("Ping");

//            IPlayer player = CallbackPlayer;
//            if (player != null)
//            {
//                Log.WriteLine("Ping from {0}", player.Name);
//                player.LastAction = DateTime.Now; // player alive
//            }
//            else
//            {
//                string endpoint = CallbackEndpoint;
//                Log.WriteLine("Ping from unknown player[" + endpoint + "]");
//            }
//        }

//        public void PublishMessage(string msg)
//        {
//            Log.WriteLine("PublishMessage {0}", msg);

//            IPlayer player = CallbackPlayer;
//            if (player != null)
//            {
//                player.LastAction = DateTime.Now; // player alive
//                //
//                if (OnMessagePublished != null)
//                    OnMessagePublished(player, msg);
//            }
//            else
//            {
//                string endpoint = CallbackEndpoint;
//                Log.WriteLine("PublishMessage from unknown player[" + endpoint + "]");
//            }
//        }

//        public void PlacePiece(int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid)
//        {
//            Log.WriteLine("PlacePiece {0} {1} {2}", tetrimino, orientation, position);

//            IPlayer player = CallbackPlayer;
//            if (player != null)
//            {
//                player.LastAction = DateTime.Now; // player alive
//                //
//                if (OnPiecePlaced != null)
//                    OnPiecePlaced(player, index, tetrimino, orientation, position, grid);
//            }
//            else
//            {
//                string endpoint = CallbackEndpoint;
//                Log.WriteLine("PlacePiece from unknown player[" + endpoint + "]");
//            }
//        }

//        public void SendAttack(int targetId, Attacks attack)
//        {
//            Log.WriteLine("SendAttack:{0} {1}", targetId, attack);

//            IPlayer player = CallbackPlayer;
//            if (player != null)
//            {
//                player.LastAction = DateTime.Now;

//                IPlayer target = PlayerManager[targetId];
//                if (target != null)
//                {
//                    //
//                    if (OnAttackSent != null)
//                        OnAttackSent(player, target, attack);
//                }
//                else
//                    Log.WriteLine("SendAttack to unknown player {0} from {1}", targetId, player.Name);
//            }
//            else
//            {
//                string endpoint = CallbackEndpoint;
//                Log.WriteLine("SendAttack from unknown player {0}", endpoint);
//            }
//        }
        
//        #endregion

//        private ITetriNETCallback Callback
//        {
//            get {
//                return OperationContext.Current == null ? LocalPlayerCallback : OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
//            }
//        }

//        private IPlayer CallbackPlayer
//        {
//            get
//            {
//                if (OperationContext.Current == null)
//                    return LocalPlayer;
//                else
//                {
//                    ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
//                    return PlayerManager[callback];
//                }
//            }
//        }

//        private string CallbackEndpoint
//        {
//            get
//            {
//                if (OperationContext.Current == null)
//                    return "BuiltIn";
//                else
//                {
//                    RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
//                    return clientEndpoint == null ? "???" : clientEndpoint.Address;
//                }
//            }
//        }
//    }

//    public class Server
//    {
//        public enum States
//        {
//            WaitingStartServer, // -> StartingServer
//            StartingServer, // -> WaitingStartGame
//            WaitingStartGame, // -> StartingGame
//            StartingGame, // -> GameStarted
//            GameStarted, // -> GameFinished
//            GameFinished, // -> WaitingStartGame
//            StoppingServer, // -> WaitingStartServer
//        }

//        private const int InactivityTimeoutBeforePing = 500; // in ms

//        private readonly Singleton<TetriminoQueue> _tetriminoQueue = new Singleton<TetriminoQueue>(() => new TetriminoQueue());
//        private readonly IHost _host;

//        public States State { get; private set; }
//        public int AttackId { get; private set; }

//        public Server(IHost host)
//        {
//            _host = host;

//            _host.OnPlayerRegistered += RegisterPlayerHandler;
//            _host.OnPlayerUnregistered += UnregisterPlayerHandler;
//            _host.OnMessagePublished += PublishMessageHandler;
//            _host.OnPiecePlaced += PlaceTetriminoHandler;
//            _host.OnAttackSent += SendAttackHandler;

//            AttackId = 0;

//            Task.Factory.StartNew(TaskResolveActions);

//            State = States.WaitingStartServer;
//        }

//        // TODO: remove following region
//        #region TEST METHODS - TO REMOVE
//        public void BroadcastRandomMessage()
//        {
//            // Send start game to every connected player
//            foreach (IPlayer p in _host.PlayerManager.Players)
//                p.OnPublishServerMessage("Random message");
//        }
//        #endregion

//        public void StartServer()
//        {
//            Log.WriteLine("Starting server");

//            State = States.StartingServer;

//            string port = ConfigurationManager.AppSettings["port"];

//            _host.Start(port);

//            State = States.WaitingStartGame;
            
//            Log.WriteLine("Server started");
//        }

//        public void StopServer()
//        {
//            Log.WriteLine("Stopping server");
//            State = States.StoppingServer;

//            _host.Stop();

//            State = States.WaitingStartServer;

//            Log.WriteLine("Server stopped");
//        }

//        public void StartGame()
//        {
//            Log.WriteLine("Starting game");

//            State = States.GameStarted;

//            // Reset Tetrimino Queue
//            _tetriminoQueue.Instance.Reset(); // TODO: random seed
//            Tetriminos firstTetrimino = _tetriminoQueue.Instance[0];
//            Tetriminos secondTetrimino = _tetriminoQueue.Instance[1];
//            // Build player list
//            List<PlayerData> players = _host.PlayerManager.Players.Select(x => new PlayerData
//                {
//                    Id = _host.PlayerManager.GetId(x),
//                    Name = x.Name
//                }
//                ).ToList();
//            // Send start game to every connected player
//            foreach (IPlayer p in _host.PlayerManager.Players)
//            {
//                p.PieceIndex = 0;
//                p.OnGameStarted(firstTetrimino, secondTetrimino, players);
//            }

//            Log.WriteLine("Game started");
//        }

//        public void StopGame()
//        {
//            Log.WriteLine("Stopping game");

//            State = States.GameFinished;
            
//            // Send start game to every connected player
//            foreach (IPlayer p in _host.PlayerManager.Players)
//                p.OnGameFinished();

//            State = States.WaitingStartGame;

//            Log.WriteLine("Game stopped");
//        }

//        private void RegisterPlayerHandler(IPlayer player, int id)
//        {
//            Log.WriteLine("New player:[{0}][{1}]", id, player.Name);

//            // Send id to player
//            player.OnPlayerRegistered(true, id);
//            // Inform players
//            foreach (IPlayer p in _host.PlayerManager.Players)
//                p.OnPublishServerMessage(String.Format("{0} is now connected", player.Name));
//        }

//        private void UnregisterPlayerHandler(IPlayer player)
//        {
//            Log.WriteLine("Player disconnected:{0}", player.Name);

//            // Inform players
//            foreach (IPlayer p in _host.PlayerManager.Players)
//                p.OnPublishServerMessage(String.Format("{0} has disconnected", player.Name));
//        }

//        private void PublishMessageHandler(IPlayer player, string msg)
//        {
//            Log.WriteLine("PublishMessage:[{0}]:{1}", player.Name, msg);

//            // Send message to players
//            foreach (IPlayer p in _host.PlayerManager.Players)
//                p.OnPublishPlayerMessage(player.Name, msg);
//        }

//        private void PlaceTetriminoHandler(IPlayer player, int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid)
//        {
//            _actionQueue.Enqueue(() => PlacePiece(player, index, tetrimino, orientation, position, grid));
//        }

//        private void SendAttackHandler(IPlayer player, IPlayer target, Attacks attack)
//        {
//            _actionQueue.Enqueue(() => Attack(player, target, attack));
//        }

//        #region Tetrimino queue

//        private class TetriminoQueue
//        {
//            private int _tetriminosCount;
//            private readonly object _lock = new object();
//            private int _size;
//            private int[] _array;
//            private Random _random;

//            public void Reset(int seed = 0)
//            {
//                _tetriminosCount = Enum.GetValues(typeof(Tetriminos)).Length;
//                _random = new Random(seed);
//                Grow(1);
//            }

//            public Tetriminos this[int index]
//            {
//                get
//                {
//                    Tetriminos tetrimino;
//                    lock (_lock)
//                    {
//                        if (index >= _size)
//                            Grow(128);
//                        tetrimino = (Tetriminos)_array[index];
//                    }
//                    return tetrimino;
//                }
//            }

//            private void Grow(int increment)
//            {
//                int newSize = _size + increment;
//                int[] newArray = new int[newSize];
//                if (_size > 0)
//                    Array.Copy(_array, newArray, _size);
//                for (int i = _size; i < newSize; i++)
//                    newArray[i] = _random.Next(_tetriminosCount);
//                _array = newArray;
//                _size = newSize;
//            }
//        }

//        #endregion

//        #region Action queue

//        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

//        private void TaskResolveActions()
//        {
//            while (true)
//            {
//                if (!_actionQueue.IsEmpty)
//                {
//                    Action action;
//                    bool dequeue = _actionQueue.TryDequeue(out action);
//                    if (dequeue)
//                    {
//                        try
//                        {
//                            action();
//                        }
//                        catch (Exception ex)
//                        {
//                            Log.WriteLine("Exception raised in TaskResolveActions. Exception:" + ex.ToString());
//                        }
//                    }
//                }
//                Thread.Sleep(0);
//                foreach (IPlayer p in _host.PlayerManager.Players)
//                {
//                    TimeSpan timespan = DateTime.Now - p.LastAction;
//                    if (timespan.TotalMilliseconds > InactivityTimeoutBeforePing)
//                    {
//                        p.OnPingReceived(); // Check if player is alive
//                        break; // TODO: use round-robin to avoid checking first players before last players
//                    }
//                }
//                Thread.Sleep(0);
//                // TODO: stop event
//            }
//        }

//        #endregion

//        #region Actions

//        private void PlacePiece(IPlayer player, int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid)
//        {
//            Log.WriteLine("PlacePiece[{0}]{1}:{2} {3} at {4},{5}", player.Name, index, tetrimino, orientation, position.X, position.Y);
//            Log.WriteLine("Grid non-empty cell count: {0}", grid.Data.Count(x => x > 0));

//            // TODO: check if index is equal to player.PieceIndex

//            // Get next piece
//            player.PieceIndex++;
//            Tetriminos nextTetrimino = _tetriminoQueue.Instance[player.PieceIndex];
//            // Send next piece
//            player.OnNextPiece(player.PieceIndex, nextTetrimino);
//        }

//        private void Attack(IPlayer player, IPlayer target, Attacks attack)
//        {
//            Log.WriteLine("SendAttack[{0}][{1}]{2}", player.Name, target.Name, attack);

//            // Store attack id locally
//            int attackId = AttackId;
//            // Increment attack
//            AttackId++;
//            // Send attack to target
//            target.Callback.OnAttackReceived(attack);
//            // Send attack message to players
//            string attackString = String.Format("{0}: {1} from {2} to {3}", attackId, attack, player.Name, target.Name);
//            foreach (IPlayer p in _host.PlayerManager.Players)
//                p.OnAttackMessageReceived(attackString);
//        }

//        #endregion
//    }
//}
