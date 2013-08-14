﻿using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET.Common;
using TetriNET.Common.WCF;

namespace TetriNET.Server
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public class WCFHost : IHost
    {
        private bool Started { get; set; }
        private ServiceHost ServiceHost { get; set; }

        private readonly IPlayerManager _playerManager;
        private readonly Func<string, ITetriNETCallback, IPlayer> _createPlayerFunc;

        public string Port { get; set; }

        public WCFHost(IPlayerManager playerManager, Func<string, ITetriNETCallback, IPlayer> createPlayerFunc)
        {
            _playerManager = playerManager;
            _createPlayerFunc = createPlayerFunc;

            Started = false;
        }

        #region IHost

        public event RegisterPlayerHandler OnPlayerRegistered;
        public event UnregisterPlayerHandler OnPlayerUnregistered;
        public event PublishMessageHandler OnMessagePublished;
        public event PlaceTetriminoHandler OnTetriminoPlaced;
        public event SendAttackHandler OnAttackSent;

        public void Start()
        {
            if (Started)
                return;

            Uri baseAddress;
            if (String.IsNullOrEmpty(Port) || Port.ToLower() == "auto")
                baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;
            else
                baseAddress = new Uri("net.tcp://localhost:" + Port + "/TetriNET");

            ServiceHost = new ServiceHost(this, baseAddress);
            ServiceHost.AddServiceEndpoint(typeof(ITetriNET), new NetTcpBinding(SecurityMode.None), "");
            //ServiceHost.AddDefaultEndpoints();
            //Host.Description.Behaviors.Add(new IPFilterServiceBehavior("DenyLocal"));
            ServiceHost.Open();

            foreach (var endpt in ServiceHost.Description.Endpoints)
            {
                Log.WriteLine("Enpoint address:\t{0}", endpt.Address);
                Log.WriteLine("Enpoint binding:\t{0}", endpt.Binding);
                Log.WriteLine("Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
            }

            Started = true;
        }

        public void Stop()
        {
            if (!Started)
                return;

            // Inform players
            foreach (IPlayer p in _playerManager.Players)
                p.OnServerStopped();

            // Close service host
            ServiceHost.Close();

            Started = false;
        }

        #endregion

        #region ITetriNET

        public void RegisterPlayer(string playerName)
        {
            Log.WriteLine("RegisterPlayer");

            ITetriNETCallback callback = Callback;

            IPlayer player = null;
            int id = -1;
            if (callback != null && _playerManager[playerName] == null && _playerManager.PlayerCount < _playerManager.MaxPlayers)
            {
                player = _createPlayerFunc(playerName, callback);
                //
                player.OnConnectionLost += PlayerConnectionLost;
                //
                id = _playerManager.Add(player);
            }
            if (id >= 0)
            {
                //
                if (OnPlayerRegistered != null)
                    OnPlayerRegistered(player, id);
            }
            else
            {
                Log.WriteLine("Register failed for player " + playerName);
                if (callback != null)
                    callback.OnPlayerRegistered(false, -1);
            }
        }

        public void UnregisterPlayer()
        {
            Log.WriteLine("UnregisterPlayer");

            IPlayer player = CallbackPlayer;
            if (player != null)
            {
                // Remove player from player list
                _playerManager.Remove(player);
                //
                if (OnPlayerUnregistered != null)
                    OnPlayerUnregistered(player);
            }
            else
            {
                string endpoint = CallbackEndpoint;
                Log.WriteLine("UnregisterPlayer from unknown player[" + endpoint + "]");
            }
        }

        public void Ping()
        {
            Log.WriteLine("Ping");

            IPlayer player = CallbackPlayer;
            if (player != null)
            {
                Log.WriteLine("Ping from {0}", player.Name);
                player.LastAction = DateTime.Now; // player alive
            }
            else
            {
                string endpoint = CallbackEndpoint;
                Log.WriteLine("Ping from unknown player[" + endpoint + "]");
            }
        }

        public void PublishMessage(string msg)
        {
            Log.WriteLine("PublishMessage {0}", msg);

            IPlayer player = CallbackPlayer;
            if (player != null)
            {
                player.LastAction = DateTime.Now; // player alive
                //
                if (OnMessagePublished != null)
                    OnMessagePublished(player, msg);
            }
            else
            {
                string endpoint = CallbackEndpoint;
                Log.WriteLine("PublishMessage from unknown player[" + endpoint + "]");
            }
        }

        public void PlaceTetrimino(Tetriminos tetrimino, Orientations orientation, Position position)
        {
            Log.WriteLine("PlaceTetrimino {0} {1} {2}", tetrimino, orientation, position);

            IPlayer player = CallbackPlayer;
            if (player != null)
            {
                player.LastAction = DateTime.Now; // player alive
                //
                if (OnTetriminoPlaced != null)
                    OnTetriminoPlaced(player, tetrimino, orientation, position);
            }
            else
            {
                string endpoint = CallbackEndpoint;
                Log.WriteLine("PlaceTetrimino from unknown player[" + endpoint + "]");
            }
        }

        public void SendAttack(int targetId, Attacks attack)
        {
            Log.WriteLine("SendAttack:{0} {1}", targetId, attack);

            IPlayer player = CallbackPlayer;
            if (player != null)
            {
                player.LastAction = DateTime.Now;

                IPlayer target = _playerManager[targetId];
                if (target != null)
                {
                    //
                    if (OnAttackSent != null)
                        OnAttackSent(player, target, attack);
                }
                else
                    Log.WriteLine("SendAttack to unknown player {0} from {1}", targetId, player.Name);
            }
            else
            {
                string endpoint = CallbackEndpoint;
                Log.WriteLine("SendAttack from unknown player {0}", endpoint);
            }
        }

        #endregion

        private ITetriNETCallback Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
            }
        }

        private IPlayer CallbackPlayer
        {
            get
            {
                ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
                return _playerManager[callback];
            }
        }

        private string CallbackEndpoint
        {
            get
            {
                RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                return clientEndpoint == null ? "???" : clientEndpoint.Address;
            }
        }

        private void PlayerConnectionLost(IPlayer player)
        {
            Log.WriteLine("PlayerConnectionLost:{0}", player.Name);
            // Remove player from player list
            _playerManager.Remove(player);
            //
            if (OnPlayerUnregistered != null)
                OnPlayerUnregistered(player);
        }
    }
}
