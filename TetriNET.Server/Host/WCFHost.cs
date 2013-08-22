﻿using System;
using System.ServiceModel;
using TetriNET.Common;
using TetriNET.Common.Contracts;
using TetriNET.Common.WCF;
using TetriNET.Server.Ban;
using TetriNET.Server.Player;

namespace TetriNET.Server.Host
{
    public sealed class WCFHost : GenericHost
    {
        [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
        public sealed class WCFServiceHost : IWCFTetriNET
        {
            private ServiceHost _serviceHost;
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

                _serviceHost = new ServiceHost(this, baseAddress);
                _serviceHost.AddServiceEndpoint(typeof(IWCFTetriNET), new NetTcpBinding(SecurityMode.None), "");
                //ServiceHost.AddDefaultEndpoints();
                _serviceHost.Description.Behaviors.Add(new IPFilterServiceBehavior(_host.BanManager));
                _serviceHost.Open();

                foreach (var endpt in _serviceHost.Description.Endpoints)
                {
                    Log.WriteLine("Enpoint address:\t{0}", endpt.Address);
                    Log.WriteLine("Enpoint binding:\t{0}", endpt.Binding);
                    Log.WriteLine("Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
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

            // Inform players
            foreach (IPlayer p in PlayerManager.Players)
                p.OnServerStopped();
            
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
