using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class BuiltInHost : IHost
    {
        private readonly IPlayerManager _playerManager;
        private readonly Func<string, IPlayer> _createPlayerFunc;

        private ITetriNETCallback _____TODELETE_Callback; // TODO: remove

        public BuiltInHost(IPlayerManager playerManager, Func<string, IPlayer> createPlayerFunc)
        {
            _playerManager = playerManager;
            _createPlayerFunc = createPlayerFunc;
        }

        #region IHost

        public event RegisterPlayerHandler OnPlayerRegistered;
        public event UnregisterPlayerHandler OnPlayerUnregistered;
        public event PublishMessageHandler OnMessagePublished;
        public event PlaceTetriminoHandler OnTetriminoPlaced;
        public event SendAttackHandler OnAttackSent;

        public void Start()
        {
            // NOP
        }

        public void Stop()
        {
            // NOP
        }

        #endregion

        #region ITetriNET

        public void RegisterPlayer(string playerName)
        {
            Log.WriteLine("RegisterPlayer");

            IPlayer player = null;
            int id = -1;
            if (_playerManager[playerName] == null && _playerManager.PlayerCount < _playerManager.MaxPlayers)
            {
                player = _createPlayerFunc(playerName);
                _____TODELETE_Callback = player.Callback; // TODO: remove
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
                // TODO: throw exception
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
                Log.WriteLine("UnregisterPlayer from unknown player");
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
                Log.WriteLine("Ping from unknown player");
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
                Log.WriteLine("PublishMessage from unknown player");
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
                Log.WriteLine("PlaceTetrimino from unknown player");
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
                Log.WriteLine("SendAttack from unknown player {0}");
            }
        }

        #endregion

        private ITetriNETCallback Callback
        {
            get
            {
                StackTrace stackTrace = new StackTrace();           // get call stack
                StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

                // write call stack method names
                foreach (StackFrame stackFrame in stackFrames)
                {
                    Console.WriteLine(stackFrame.GetMethod().Name);   // write method name
                }

                // TODO: get call stack and search for a IPlayer and get callback from IPlayer
                return _____TODELETE_Callback; // TODO: remove
            }
        }

        private IPlayer CallbackPlayer
        {
            get
            {
                ITetriNETCallback callback = Callback;
                return callback == null ? null : _playerManager[callback];
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
