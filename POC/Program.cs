using System;
using System.Configuration;
using TetriNET.Client;
using TetriNET.Server;

namespace POC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //
            string baseAddress = ConfigurationManager.AppSettings["address"];
            ExceptionFreeProxyManager proxyManager = new ExceptionFreeProxyManager(baseAddress);
            GameClient client = new GameClient(proxyManager);
            client.PlayerName = "Joel_" + Guid.NewGuid().ToString().Substring(0, 6);

            //
            PlayerManager playerManager = new PlayerManager();
            ExceptionFreeCallbackManager callbackManager = new ExceptionFreeCallbackManager(playerManager);
            GameServer server = new GameServer(callbackManager, playerManager);

            // Start server
            server.StartService();

            // Display available commands
            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop server");
            Console.WriteLine("s: Start game");
            Console.WriteLine("t: Stop game");
            Console.WriteLine("m: Send message broadcast");

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
                    }
                }
                else
                    System.Threading.Thread.Sleep(250);
            }

            server.StopService();
        }
    }

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