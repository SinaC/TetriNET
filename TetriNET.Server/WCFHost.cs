using System;
using System.ServiceModel;
using TetriNET.Common;
using TetriNET.Common.WCF;

namespace TetriNET.Server
{
    public sealed class WCFHost : GenericHost
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

            public void Heartbeat()
            {
                _host.Heartbeat(Callback);
            }

            public void PublishMessage(string msg)
            {
                _host.PublishMessage(Callback, msg);
            }

            public void PlaceTetrimino(int index, Tetriminos tetrimino, Orientations orientation, Position position, byte[] grid)
            {
                _host.PlaceTetrimino(Callback, index, tetrimino, orientation, position, grid);
            }

            public void UseSpecial(int targetId, Specials special)
            {
                _host.UseSpecial(Callback, targetId, special);
            }

            public void ModifyGrid(byte[] grid)
            {
                _host.ModifyGrid(Callback, grid);
            }

            public void SendLines(int count)
            {
                _host.SendLines(Callback, count);
            }

            public void StartGame()
            {
                _host.StartGame(Callback);
            }

            public void StopGame()
            {
                _host.StopGame(Callback);
            }

            public void PauseGame()
            {
                _host.PauseGame(Callback);
            }

            public void ResumeGame()
            {
                _host.ResumeGame(Callback);
            }

            public void GameLost()
            {
                _host.GameLost(Callback);
            }

            public void ChangeOptions(GameOptions options)
            {
                _host.ChangeOptions(Callback, options);
            }

            public void KickPlayer(int playerId)
            {
                _host.KickPlayer(Callback, playerId);
            }

            public void BanPlayer(int playerId)
            {
                _host.BanPlayer(Callback, playerId);
            }

            public void ResetWinList()
            {
                _host.ResetWinList(Callback);
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

        public override void RemovePlayer(IPlayer player)
        {
            // NOP
        }

        public override void BanPlayer(IPlayer player)
        {
            // TODO
        }

        #endregion
       
    }
}
