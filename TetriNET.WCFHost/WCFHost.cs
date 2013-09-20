using System;
using System.ServiceModel;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.Common.WCF;

namespace TetriNET.WCFHost
{
    public sealed class WCFHost : GenericHost.GenericHost
    {
        [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
        public sealed class WCFServiceHost : IWCFTetriNET
        {
            private ServiceHost _serviceHost;
            private readonly IHost _host;

            public string Port { get; set; }

            public WCFServiceHost(IHost host)
            {
                if (host == null)
                    throw new ArgumentNullException("host");
                _host = host;
            }

            public void Start()
            {
                Uri baseAddress;
                if (String.IsNullOrEmpty(Port) || Port.ToLower() == "auto")
                    baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;
                else
                    baseAddress = new Uri("net.tcp://localhost:" + Port + "/TetriNET");

                _serviceHost = new ServiceHost(this, baseAddress);
                _serviceHost.AddServiceEndpoint(typeof(IWCFTetriNET), new NetTcpBinding(SecurityMode.None), "");
                //ServiceHost.AddDefaultEndpoints();
                _serviceHost.Description.Behaviors.Add(new IPFilterServiceBehavior(_host.BanManager, _host.PlayerManager));
                _serviceHost.Open();

                foreach (var endpt in _serviceHost.Description.Endpoints)
                {
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Enpoint address:\t{0}", endpt.Address);
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Enpoint binding:\t{0}", endpt.Binding);
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
                }
            }

            public void Stop()
            {
                // Close service host
                _serviceHost.Close();
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

            public void PlacePiece(int index, Pieces piece, int orientation, int posX, int posY, byte[] grid)
            {
                _host.PlacePiece(Callback, index, piece, orientation, posX, posY, grid);
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

        private bool _started;

        private readonly WCFServiceHost _serviceHost;

        public string Port {
            get { return _serviceHost.Port; }
            set { _serviceHost.Port = value; }
        }

        public WCFHost(IPlayerManager playerManager, IBanManager banManager, Func<string, ITetriNETCallback, IPlayer> createPlayerFunc)
            : base(playerManager, banManager, createPlayerFunc)
        {
            _serviceHost = new WCFServiceHost(this);

            _started = false;
        }

        #region IHost

        public override void Start()
        {
            if (_started)
                return;

            _serviceHost.Start();

            _started = true;
        }

        public override void Stop()
        {
            if (!_started)
                return;
            
            _serviceHost.Stop();

            _started = false;
        }

        public override void RemovePlayer(IPlayer player)
        {
            // NOP
        }

        #endregion
    }
}
