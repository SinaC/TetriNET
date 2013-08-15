using System;
using System.ServiceModel;
using TetriNET.Common;
using TetriNET.Common.WCF;

namespace TetriNET.Server
{
    public class WCFHost : GenericHost
    {
        [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
        private class WCFServiceHost : IWCFTetriNET
        {
            private ServiceHost ServiceHost { get; set; }
            private readonly IHost _host;
            public string Port { get; set; }

            public WCFServiceHost(IHost host)
            {
                _host = host;
            }

            public void Start()
            {
                Uri baseAddress;
                if (String.IsNullOrEmpty(Port) || Port.ToLower() == "auto")
                    baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;
                else
                    baseAddress = new Uri("net.tcp://localhost:" + Port + "/TetriNET");

                ServiceHost = new ServiceHost(this, baseAddress);
                ServiceHost.AddServiceEndpoint(typeof(IWCFTetriNET), new NetTcpBinding(SecurityMode.None), "");
                //ServiceHost.AddDefaultEndpoints();
                //Host.Description.Behaviors.Add(new IPFilterServiceBehavior("DenyLocal"));
                ServiceHost.Open();

                foreach (var endpt in ServiceHost.Description.Endpoints)
                {
                    Log.WriteLine("Enpoint address:\t{0}", endpt.Address);
                    Log.WriteLine("Enpoint binding:\t{0}", endpt.Binding);
                    Log.WriteLine("Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
                }
            }

            public void Stop()
            {
                // Close service host
                ServiceHost.Close();
            }

            #region IWCFTetriNET

            public void RegisterPlayer(string playerName)
            {
                _host.RegisterPlayer(Callback, playerName);
            }

            public void UnregisterPlayer()
            {
                _host.UnregisterPlayer(Callback);
            }

            public void Ping()
            {
                _host.Ping(Callback);
            }

            public void PublishMessage(string msg)
            {
                _host.PublishMessage(Callback, msg);
            }

            public void PlaceTetrimino(Tetriminos tetrimino, Orientations orientation, Position position)
            {
                _host.PlaceTetrimino(Callback, tetrimino, orientation, position);
            }

            public void SendAttack(int targetId, Attacks attack)
            {
                _host.SendAttack(Callback, targetId, attack);
            }

            #endregion

            private ITetriNETCallback Callback
            {
                get
                {
                    return OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
                }
            }
        }

        private bool Started { get; set; }

        private readonly WCFServiceHost _serviceHost;

        public string Port {
            get { return _serviceHost.Port; }
            set { _serviceHost.Port = value; }
        }

        public WCFHost(IPlayerManager playerManager, Func<string, ITetriNETCallback, IPlayer> createPlayerFunc) : base(playerManager, createPlayerFunc)
        {
            _serviceHost = new WCFServiceHost(this);

            Started = false;
        }

        #region IHost

        public override void Start()
        {
            if (Started)
                return;

            _serviceHost.Start();

            Started = true;
        }

        public override void Stop()
        {
            if (!Started)
                return;

            // Inform players
            foreach (IPlayer p in PlayerManager.Players)
                p.OnServerStopped();
            
            _serviceHost.Stop();

            Started = false;
        }

        #endregion
       
    }
}
