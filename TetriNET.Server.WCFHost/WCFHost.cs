﻿using System;
using System.ServiceModel;
using ServiceModelEx;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.WCFHost
{
    public sealed class WCFHost : HostBase.HostBase
    {
        [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
        public sealed class WCFServiceHost : IWCFTetriNET, IWCFTetriNETSpectator
        {
            private ServiceHost _serviceHost;
            private readonly IHost _host;

            public string Port { get; set; }

            public WCFServiceHost(IHost host)
            {
                if (host == null)
                    throw new ArgumentNullException(nameof(host));
                _host = host;
            }

            public void Start()
            {
                Uri baseAddress;
                if (String.IsNullOrEmpty(Port) || Port.ToLower() == "auto")
                    baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;
                else
                    //baseAddress = new Uri("net.tcp://localhost:" + Port + "/TetriNET");
                    baseAddress = new Uri("net.tcp://localhost:" + Port);

                _serviceHost = new ServiceHost(this, baseAddress);
                _serviceHost.AddServiceEndpoint(typeof(IWCFTetriNET), new NetTcpBinding(SecurityMode.None), "/TetriNET");
                _serviceHost.AddServiceEndpoint(typeof(IWCFTetriNETSpectator), new NetTcpBinding(SecurityMode.None), "/TetriNETSpectator");
                _serviceHost.Description.Behaviors.Add(new IPFilterServiceBehavior(_host.BanManager, _host.PlayerManager));
                _serviceHost.Open();

                Log.Default.WriteLine(LogLevels.Info, "WCF Host opened on {0}", baseAddress);

                foreach (var endpt in _serviceHost.Description.Endpoints)
                {
                    Log.Default.WriteLine(LogLevels.Debug, "Enpoint address:\t{0}", endpt.Address);
                    Log.Default.WriteLine(LogLevels.Debug, "Enpoint binding:\t{0}", endpt.Binding);
                    Log.Default.WriteLine(LogLevels.Debug, "Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
                }
            }

            public void Stop()
            {
                // Close service host
                _serviceHost?.Close();
            }

            #region IWCFTetriNET

            public void RegisterPlayer(Versioning clientVersion, string playerName, string team)
            {
                _host.RegisterPlayer(Callback, clientVersion, playerName, team);
            }

            public void UnregisterPlayer()
            {
                _host.UnregisterPlayer(Callback);
            }

            public void Heartbeat()
            {
                _host.Heartbeat(Callback);
            }

            public void PlayerTeam(string team)
            {
                _host.PlayerTeam(Callback, team);
            }

            public void PublishMessage(string msg)
            {
                _host.PublishMessage(Callback, msg);
            }

            public void PlacePiece(int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
            {
                _host.PlacePiece(Callback, pieceIndex, highestIndex, piece, orientation, posX, posY, grid);
            }

            public void UseSpecial(int targetId, Specials special)
            {
                _host.UseSpecial(Callback, targetId, special);
            }

            public void ModifyGrid(byte[] grid)
            {
                _host.ModifyGrid(Callback, grid);
            }

            public void ClearLines(int count)
            {
                _host.ClearLines(Callback, count);
            }

            public void GameLost()
            {
                _host.GameLost(Callback);
            }

            public void FinishContinuousSpecial(Specials special)
            {
                _host.FinishContinuousSpecial(Callback, special);
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

            public void EarnAchievement(int achievementId, string achievementTitle)
            {
                _host.EarnAchievement(Callback, achievementId, achievementTitle);
            }

            #endregion

            #region IWCFTetriNETSpectator
            // Spectator connexion/deconnexion management
            public void RegisterSpectator(Versioning clientVersion, string spectatorName)
            {
                _host.RegisterSpectator(Callback, clientVersion, spectatorName);
            }

            public void UnregisterSpectator()
            {
                _host.UnregisterSpectator(Callback);
            }

            public void HeartbeatSpectator()
            {
                _host.HeartbeatSpectator(Callback);
            }
            
            // Chat
            public void PublishSpectatorMessage(string msg)
            {
                _host.PublishSpectatorMessage(Callback, msg);
            }

            #endregion

            private ITetriNETCallback Callback => OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
        }

        private bool _started;

        private readonly WCFServiceHost _serviceHost;

        public string Port {
            get { return _serviceHost.Port; }
            set { _serviceHost.Port = value; }
        }

        public WCFHost(IPlayerManager playerManager, ISpectatorManager spectatorManager, IBanManager banManager, IFactory factory)
            : base(playerManager, spectatorManager, banManager, factory)
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

        public override void RemoveSpectator(ISpectator spectator)
        {
            // NOP
        }

        #endregion
    }
}
