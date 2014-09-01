using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.WCF.Service
{
    public partial class Service : ServiceBase
    {
        private const string EventLogSource = "TetriNET";
        private const string EventLogName = "TetriNET";

        private Task _mainLoopTask;
        private CancellationTokenSource _cancellationTokenSource;

        public Service()
        {
            InitializeComponent();

            //An event log source should not be created and immediately used. There is a latency time to enable the source.
            if (!EventLog.SourceExists(EventLogSource))
            {
                EventLog.CreateEventSource(EventLogSource, EventLogName);
                while(true)
                {
                    if (EventLog.SourceExists(EventLogSource))
                        break;

                    Thread.Sleep(250);
                }
            }
            //
            eventLog = new EventLog
                {
                    Source = EventLogSource,
                };
            //
            Log.Default.Initialize(ConfigurationManager.AppSettings["logpath"], "serverservice.log");
        }

        protected override void OnStart(string[] args)
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            string company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCompanyAttribute), false)).Company;
            string product = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute), false)).Product;
            Log.Default.WriteLine(LogLevels.Info, "{0} {1}.{2} by {3}", product, version.Major, version.Minor, company);
            Log.Default.WriteLine(LogLevels.Info, "Starting");

            _cancellationTokenSource = new CancellationTokenSource();
            _mainLoopTask = Task.Factory.StartNew(ServiceMainLoop, _cancellationTokenSource.Token);
        }

        protected override void OnStop()
        {
            Log.Default.WriteLine(LogLevels.Info, "Stopping");

            _cancellationTokenSource.Cancel();

            _mainLoopTask.Wait(1000); // Wait max 1 second
        }

        private void ServiceMainLoop()
        {
            //
            IFactory factory = new Factory();
            //
            IBanManager banManager = factory.CreateBanManager();
            //
            IPlayerManager playerManager = factory.CreatePlayerManager(6);
            ISpectatorManager spectatorManager = factory.CreateSpectatorManager(10);

            //
            IHost wcfHost = new Server.WCFHost.WCFHost(
                playerManager,
                spectatorManager,
                banManager,
                factory)
            {
                Port = ConfigurationManager.AppSettings["port"]
            };

            //
            IPieceProvider pieceProvider = factory.CreatePieceProvider();

            //
            IServer server = new Server.Server(playerManager, spectatorManager, pieceProvider);
            server.AddHost(wcfHost);

            //
            server.StartServer();

            while(!_cancellationTokenSource.IsCancellationRequested)
                Thread.Sleep(250);

            server.StopServer();
        }
    }
}
