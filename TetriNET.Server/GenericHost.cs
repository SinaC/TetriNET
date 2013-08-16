using System;
using TetriNET.Common;

namespace TetriNET.Server
{
    public abstract class GenericHost : IHost
    {
        protected readonly IPlayerManager PlayerManager;
        protected readonly Func<string, ITetriNETCallback, IPlayer> CreatePlayerFunc;

        protected GenericHost(IPlayerManager playerManager, Func<string, ITetriNETCallback, IPlayer> createPlayerFunc)
        {
            PlayerManager = playerManager;
            CreatePlayerFunc = createPlayerFunc;
        }

        #region IHost

        public event RegisterPlayerHandler OnPlayerRegistered;
        public event UnregisterPlayerHandler OnPlayerUnregistered;
        public event PublishMessageHandler OnMessagePublished;
        public event PlaceTetriminoHandler OnTetriminoPlaced;
        public event SendAttackHandler OnAttackSent;

        public abstract void Start();
        public abstract void Stop();

        #endregion

        #region ITetriNET

        public virtual void RegisterPlayer(ITetriNETCallback callback, string playerName)
        {
            Log.WriteLine("RegisterPlayer");

            IPlayer player = null;
            int id = -1;
            if (!String.IsNullOrEmpty(playerName) && PlayerManager[playerName] == null && PlayerManager.PlayerCount < PlayerManager.MaxPlayers)
            {
                player = CreatePlayerFunc(playerName, callback);
                //
                player.OnConnectionLost += PlayerConnectionLost;
                //
                id = PlayerManager.Add(player);
            }
            if (id >= 0)
            {
                //
                if (OnPlayerRegistered != null)
                    OnPlayerRegistered(player, id);
            }
            else
            {
                Log.WriteLine("Register failed for player {0}", playerName);
                // TODO: throw exception
            }
        }

        public virtual void UnregisterPlayer(ITetriNETCallback callback)
        {
            Log.WriteLine("UnregisterPlayer");

            IPlayer player = PlayerManager[callback];
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
                Log.WriteLine("UnregisterPlayer from unknown player");
            }
        }

        public virtual void Ping(ITetriNETCallback callback)
        {
            Log.WriteLine("Ping");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                Log.WriteLine("Ping from {0}", player.Name);
                player.LastAction = DateTime.Now; // player alive
            }
            else
            {
                Log.WriteLine("Ping from unknown player");
            }
        }

        public virtual void PublishMessage(ITetriNETCallback callback, string msg)
        {
            Log.WriteLine("PublishMessage {0}", msg);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                player.LastAction = DateTime.Now; // player alive
                //
                if (OnMessagePublished != null)
                    OnMessagePublished(player, msg);
            }
            else
            {
                Log.WriteLine("PublishMessage from unknown player");
            }
        }

        public virtual void PlaceTetrimino(ITetriNETCallback callback, int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid)
        {
            Log.WriteLine("PlaceTetrimino {0} {1} {2} {3}", index, tetrimino, orientation, position);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                player.LastAction = DateTime.Now; // player alive
                //
                if (OnTetriminoPlaced != null)
                    OnTetriminoPlaced(player, index, tetrimino, orientation, position, grid);
            }
            else
            {
                Log.WriteLine("PlaceTetrimino from unknown player");
            }
        }

        public virtual void SendAttack(ITetriNETCallback callback, int targetId, Attacks attack)
        {
            Log.WriteLine("SendAttack:{0} {1}", targetId, attack);

            IPlayer player = PlayerManager[callback];
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
                Log.WriteLine("SendAttack from unknown player {0}");
            }
        }

        #endregion

        protected virtual void PlayerConnectionLost(IPlayer player)
        {
            Log.WriteLine("PlayerConnectionLost:{0}", player.Name);
            // Remove player from player list
            PlayerManager.Remove(player);
            //
            if (OnPlayerUnregistered != null)
                OnPlayerUnregistered(player);
        }
    }
}
