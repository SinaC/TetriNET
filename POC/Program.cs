//using POC.Client_POC;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using POC.JQuery_WCF;
using POC.SignalR;

namespace POC
{
    

    internal class Program
    {
        private static void Main(string[] args)
        {
            //Service1 service = new Service1();

            //Console.ReadLine();

            //System.Threading.Tasks.Task.Run(() =>
            //{
            //    ServiceReference1.Service1Client client = new ServiceReference1.Service1Client("NetTcpBinding_IService1");
            //    string[] users = client.GetUser("2");
            //    if (users == null)
            //        Console.WriteLine("no users");
            //    else
            //        Console.WriteLine(users.Aggregate((n, i) => n + "," + i));
            //    client.Close();
            //});

            SignalRServer server = new SignalRServer();
            server.Start();


            Console.ReadLine();

            //Client client = new Client(callback => new WCFProxy(@"net.tcp://localhost:8765/TetriNET", callback));
            //client.Name = "joel-wpf-client";
            //client._proxy.RegisterPlayer(client, client.Name);

            //bool stopped = false;
            //while (!stopped)
            //{
            //    if (Console.KeyAvailable)
            //    {
            //        ConsoleKeyInfo cki = Console.ReadKey(true);
            //        switch (cki.Key)
            //        {
            //            case ConsoleKey.X:
            //                stopped = true;
            //                break;
            //        }
            //    }
            //    else
            //    {
            //        System.Threading.Thread.Sleep(100);
            //    }
            //}

            //TCPServer server = new TCPServer
            //    {
            //        Port = 5656
            //    };
            //server.Start();

            //Console.WriteLine("Press enter to stop server");

            //Console.ReadLine();

            //server.Stop();
        }
    }

    /*
    internal class Program
    {
        private static void Main(string[] args)
        {
            POC.Server_POC.PlayerManager playerManager = new POC.Server_POC.PlayerManager(6);
            POC.Server_POC.WCFHost host = new POC.Server_POC.WCFHost(playerManager);
            POC.Server_POC.Server server = new POC.Server_POC.Server(host);

            server.StartServer();

            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop server");
            Console.WriteLine("s: Start game");
            Console.WriteLine("t: Stop game");
            Console.WriteLine("m: Send message broadcast");

            bool stopped = false;
            while (!stopped)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        case ConsoleKey.X:
                            stopped = true;
                            break;
                        case ConsoleKey.S:
                            server.StartGame();
                            break;
                        case ConsoleKey.T:
                            server.StopGame();
                            break;
                        case ConsoleKey.M:
                            server.BroadcastRandomMessage();
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(1000);
            }
        }
    }
    */
    /*
    public class BuiltInClientProxyManager : IProxyManager
    {
        public IWCFTetriNET Proxy { private get; set; }

        public IWCFTetriNET CreateProxy(ITetriNETCallback callback, IClient client)
        {
            return Proxy;
        }
    }

    public class BuiltInClientCallback : ITetriNETCallback
    {
        private readonly IPlayerManager _playerManager;
        private readonly ITetriNETCallback _callback;

        public BuiltInClientCallback(ITetriNETCallback callback, IPlayerManager playerManager)
        {
            _callback = callback;
            _playerManager = playerManager;
        }

        private void UpdateTimerOnAction(Action action)
        {
            action();
            IPlayer player = _playerManager[this];
            player.LastAction = DateTime.Now;
        }

        public void OnPingReceived()
        {
            UpdateTimerOnAction(_callback.OnPingReceived);
        }

        public void OnServerStopped()
        {
            UpdateTimerOnAction(_callback.OnServerStopped);
        }

        public void OnPlayerRegistered(bool succeeded, int playerId)
        {
            UpdateTimerOnAction(() => _callback.OnPlayerRegistered(succeeded, playerId));
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, System.Collections.Generic.List<PlayerData> players)
        {
            UpdateTimerOnAction(() => _callback.OnGameStarted(firstTetrimino, secondTetrimino, players));
        }

        public void OnGameFinished()
        {
            UpdateTimerOnAction(_callback.OnGameFinished);
        }

        public void OnServerAddLines(int lineCount)
        {
            UpdateTimerOnAction(() => _callback.OnServerAddLines(lineCount));
        }

        public void OnPlayerAddLines(int lineCount)
        {
            UpdateTimerOnAction(() => _callback.OnPlayerAddLines(lineCount));
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            UpdateTimerOnAction(() => _callback.OnPublishPlayerMessage(playerName, msg));
        }

        public void OnPublishServerMessage(string msg)
        {
            UpdateTimerOnAction(() => _callback.OnPublishServerMessage(msg));
        }

        public void OnAttackReceived(Attacks attack)
        {
            UpdateTimerOnAction(() => _callback.OnAttackReceived(attack));
        }

        public void OnAttackMessageReceived(string msg)
        {
            UpdateTimerOnAction(() => _callback.OnAttackMessageReceived(msg));
        }

        public void OnNextPiece(int index, Tetriminos tetrimino)
        {
            UpdateTimerOnAction(() => _callback.OnNextPiece(index, tetrimino));
        }
    }

    //public class BuiltInClientCallbackManager : ICallbackManager
    //{
    //    private readonly IPlayerManager _playerManager;
    //    private BuiltInClientCallback _callback;

    //    public BuiltInClientCallbackManager(IPlayerManager playerManager)
    //    {
    //        _playerManager = playerManager;
    //    }

    //    public ITetriNETCallback Callback {
    //        get
    //        {
    //            return _callback;
    //        }
    //        set { _callback = new BuiltInClientCallback(value, _playerManager);}
    //    }
    //}

    public class CallbackManager : ICallbackManager
    {
        private readonly ConcurrentDictionary<ITetriNETCallback, ExceptionFreeCallback> _callbacks = new ConcurrentDictionary<ITetriNETCallback, ExceptionFreeCallback>();
        private readonly IPlayerManager _playerManager;
        private BuiltInClientCallback _builtInClientCallback;

        public CallbackManager(IPlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        public void SetBuiltInClientCallback(ITetriNETCallback callback)
        {
            _builtInClientCallback = new BuiltInClientCallback(callback, _playerManager);
        }

        #region ICallbackManager
        public ITetriNETCallback Callback
        {
            get
            {
                if (OperationContext.Current == null)
                {
                    return _builtInClientCallback;
                }
                else
                {
                    ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
                    ExceptionFreeCallback exceptionFreeCallback;
                    bool found = _callbacks.TryGetValue(callback, out exceptionFreeCallback);
                    if (!found)
                    {
                        exceptionFreeCallback = new ExceptionFreeCallback(callback, _playerManager);
                        _callbacks.TryAdd(callback, exceptionFreeCallback);
                        exceptionFreeCallback.OnPlayerDisconnected += OnPlayerDisconnected;
                    }
                    return exceptionFreeCallback;
                }
            }
        }
        public string Endpoint
        {
            get
            {
                if (OperationContext.Current == null)
                    return "BuiltIn";
                else
                {
                    RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                    return clientEndpoint == null ? "???" : clientEndpoint.Address;
                }
            }
        }

        #endregion

        private void OnPlayerDisconnected(object sender, IPlayer player)
        {
            ITetriNETCallback callback = player.Callback;
            if (callback is ExceptionFreeCallback)
            {
                ExceptionFreeCallback tryRemoveResult;
                _callbacks.TryRemove(callback, out tryRemoveResult);
            }
            _playerManager.Remove(player);
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            //
            //string baseAddress = ConfigurationManager.AppSettings["address"];
            //ExceptionFreeProxyManager proxyManager = new ExceptionFreeProxyManager(baseAddress);
            BuiltInClientProxyManager proxyManager = new BuiltInClientProxyManager();
            GameClient client = new GameClient(proxyManager);
            client.PlayerName = "Joel_" + Guid.NewGuid().ToString().Substring(0, 6);

            //
            PlayerManager playerManager = new PlayerManager();
            //ExceptionFreeCallbackManager callbackManager = new ExceptionFreeCallbackManager(playerManager);
            //BuiltInClientCallbackManager callbackManager = new BuiltInClientCallbackManager(playerManager);
            CallbackManager callbackManager = new CallbackManager(playerManager);
            GameServer server = new GameServer(callbackManager, playerManager);

            //
            proxyManager.Proxy = server;
            callbackManager.SetBuiltInClientCallback(client);

            // Start server
            server.StartService();

            // Display available commands
            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop server");
            Console.WriteLine("s: Start game");
            Console.WriteLine("t: Stop game");
            Console.WriteLine("m: Send message broadcast");
            Console.WriteLine("c: Close client");

            bool stopped = false;
            while (!stopped)
            {
                client.Test();

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        case ConsoleKey.X:
                            stopped = true;
                            break;
                        case ConsoleKey.S:
                            server.StartGame();
                            break;
                        case ConsoleKey.T:
                            server.StopGame();
                            break;
                        case ConsoleKey.M:
                            server.BroadcastRandomMessage();
                            break;
                        case ConsoleKey.C:
                            client.DisconnectFromServer();
                            callbackManager.SetBuiltInClientCallback(client); // workaround, we should never be able to disconnect a built-in client
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(250);
            }

            server.StopService();
        }
    }
  */

    ////http://stackoverflow.com/questions/12089879/how-to-block-incoming-connections-from-specific-addresses
    //public class StackOverflow_12089879
    //{
    //    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IStackCalculatorCallback))]
    //    public interface IStackCalculator
    //    {
    //        [OperationContract]
    //        void Enter(double value);
    //        [OperationContract]
    //        double Add();
    //        [OperationContract]
    //        double Subtract();
    //        [OperationContract]
    //        double Multiply();
    //        [OperationContract]
    //        double Divide();
    //    }

    //    public interface IStackCalculatorCallback
    //    {
    //        [OperationContract]
    //        int StackSize();
    //    }

    //    public class StackCalculator : IStackCalculator
    //    {
    //        public Stack<double> stack = new Stack<double>();

    //        public void Enter(double value)
    //        {
    //            Console.WriteLine("Entering {0}", value);
    //            stack.Push(value);
    //        }

    //        public double Add()
    //        {
    //            return Execute("Add", (x, y) => x + y);
    //        }

    //        public double Subtract()
    //        {
    //            return Execute("Subtract", (x, y) => x - y);
    //        }

    //        public double Multiply()
    //        {
    //            return Execute("Multiply", (x, y) => x * y);
    //        }

    //        public double Divide()
    //        {
    //            return Execute("Divide", (x, y) => x / y);
    //        }

    //        private double Execute(string operationName, Func<double, double, double> operation)
    //        {
    //            double first = stack.Pop();
    //            double second = stack.Pop();
    //            double result = operation(first, second);
    //            Console.WriteLine("Executing {0}({1}, {2}), result = {3}", operationName, first, second, result);
    //            stack.Push(result);
    //            return result;
    //        }
    //    }
    //    static Binding GetBinding()
    //    {
    //        var result = new NetTcpBinding(SecurityMode.None);
    //        return result;
    //    }

    //    public class MyInstanceContextInitializer : IEndpointBehavior, IInstanceContextInitializer
    //    {
    //        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    //        {
    //        }

    //        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    //        {
    //        }

    //        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
    //        {
    //            endpointDispatcher.DispatchRuntime.InstanceContextInitializers.Add(this);
    //        }

    //        public void Validate(ServiceEndpoint endpoint)
    //        {
    //        }

    //        public void Initialize(InstanceContext instanceContext, Message message)
    //        {
    //            RemoteEndpointMessageProperty remp = (RemoteEndpointMessageProperty)message.Properties[RemoteEndpointMessageProperty.Name];
    //            Console.WriteLine("Starting new session from {0}:{1}", remp.Address, remp.Port);
    //            Console.WriteLine("If session should not be started, throw an exception here");
    //        }
    //    }

    //    public class TestClient : IStackCalculatorCallback
    //    {
    //        private DuplexChannelFactory<IStackCalculator> _factory;
    //        private IStackCalculator _proxy;

    //        public void Open(string baseAddress) {
    //            //ChannelFactory<IStackCalculator> factory = new ChannelFactory<IStackCalculator>(GetBinding(), new EndpointAddress(baseAddress));
    //            _factory = new DuplexChannelFactory<IStackCalculator>(new InstanceContext(this), GetBinding(), new EndpointAddress(baseAddress));
    //            _proxy = _factory.CreateChannel();
    //            _proxy.Enter(40);
    //            _proxy.Enter(30);
    //            _proxy.Enter(20);
    //            _proxy.Add();
    //            _proxy.Subtract();
    //        }

    //        public void Close()
    //        {
    //            ((IClientChannel)_proxy).Close();
    //            _factory.Close();
    //        }

    //        public int StackSize()
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    public static void Test()
    //    {
    //        string baseAddress = "net.tcp://" + Environment.MachineName + ":8000/Service";
    //        ServiceHost host = new ServiceHost(typeof(StackCalculator), new Uri(baseAddress));
    //        ServiceEndpoint endpoint = host.AddServiceEndpoint(typeof(IStackCalculator), GetBinding(), "");
    //        endpoint.Behaviors.Add(new MyInstanceContextInitializer());
    //        host.Open();
    //        Console.WriteLine("Host opened");

    //        foreach (var endpt in host.Description.Endpoints)
    //        {
    //            Console.WriteLine("Enpoint address:\t{0}", endpt.Address);
    //            Console.WriteLine("Enpoint binding:\t{0}", endpt.Binding);
    //            Console.WriteLine("Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
    //        }

    //        ////ChannelFactory<IStackCalculator> factory = new ChannelFactory<IStackCalculator>(GetBinding(), new EndpointAddress(baseAddress));
    //        //ChannelFactory<IStackCalculator> factory = new DuplexChannelFactory<IStackCalculator>(new InstanceContext(host), GetBinding(), new EndpointAddress(baseAddress));
    //        //IStackCalculator proxy = factory.CreateChannel();
    //        //proxy.Enter(40);
    //        //proxy.Enter(30);
    //        //proxy.Enter(20);
    //        //proxy.Add();
    //        //proxy.Subtract();

    //        //ChannelFactory<IStackCalculator> factory2 = new ChannelFactory<IStackCalculator>(GetBinding(), new EndpointAddress(baseAddress));
    //        //IStackCalculator proxy2 = factory.CreateChannel();
    //        //proxy2.Enter(40);
    //        //proxy2.Enter(30);
    //        //proxy2.Enter(20);
    //        //proxy2.Add();
    //        //proxy2.Subtract();

    //        //((IClientChannel)proxy).Close();
    //        //factory.Close();
    //        //((IClientChannel)proxy2).Close();
    //        //factory2.Close();

    //        TestClient client1 = new TestClient();
    //        client1.Open(baseAddress);
    //        client1.Close();

    //        Console.Write("Press ENTER to close the host");
    //        Console.ReadLine();
    //        host.Close();
    //    }
    //}
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        // StackOverflow_12089879.Test();
    //    }
    //}
}