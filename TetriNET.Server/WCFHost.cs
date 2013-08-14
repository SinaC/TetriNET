using System;
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
        private IPlayer LocalPlayer { get; set; }

        public WCFHost(IPlayerManager playerManager)
        {
            PlayerManager = playerManager;
            Started = false;
        }

        public ITetriNETCallback LocalPlayerCallback { private get; set; }

        #region IHost

        public event RegisterPlayerHandler OnPlayerRegistered;
        public event UnregisterPlayerHandler OnPlayerUnregistered;
        public event PublishMessageHandler OnMessagePublished;
        public event PlaceTetriminoHandler OnTetriminoPlaced;
        public event SendAttackHandler OnAttackSent;

        public IPlayerManager PlayerManager { get; private set; }

        public void Start(string port)
        {
            if (Started)
                return;

            Uri baseAddress;
            if (String.IsNullOrEmpty(port) || port.ToLower() == "auto")
                baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;
            else
                baseAddress = new Uri("net.tcp://localhost:" + port + "/TetriNET");

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
            foreach (IPlayer p in PlayerManager.Players)
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
            if (callback != null && PlayerManager[playerName] == null && PlayerManager.PlayerCount < PlayerManager.MaxPlayers)
            {
                if (callback == LocalPlayerCallback)
                    // Local player
                    player = new LocalPlayer(playerName, LocalPlayerCallback);
                else
                    // Remote player
                    player = new RemotePlayer(playerName, callback);
                //
                player.OnDisconnected += PlayerDisconnected;
                //
                id = PlayerManager.Add(player);
            }
            if (id >= 0)
            {
                // Save local player
                if (player is LocalPlayer)
                    LocalPlayer = player;
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
                PlayerManager.Remove(player);
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

                IPlayer target = PlayerManager[targetId];
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
                return OperationContext.Current == null ? LocalPlayerCallback : OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
            }
        }

        private IPlayer CallbackPlayer
        {
            get
            {
                if (OperationContext.Current == null)
                    return LocalPlayer;
                else
                {
                    ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
                    return PlayerManager[callback];
                }
            }
        }

        private string CallbackEndpoint
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

        private void PlayerDisconnected(IPlayer player)
        {
            Log.WriteLine("PlayerDisconnected:{0}", player.Name);
            // Remove player from player list
            PlayerManager.Remove(player);
            //
            if (OnPlayerUnregistered != null)
                OnPlayerUnregistered(player);
        }
    }
}
