using System;
using System.Linq;
using TetriNET.Common;
using TetriNET.Common.Contracts;
using TetriNET.Common.Interfaces;

namespace TetriNET.Server
{
    public abstract class GenericHost : IHost
    {
        protected readonly Func<string, ITetriNETCallback, IPlayer> CreatePlayerFunc;
        protected readonly IPlayerManager PlayerManager;

        protected GenericHost(IPlayerManager playerManager, IBanManager banManager, Func<string, ITetriNETCallback, IPlayer> createPlayerFunc)
        {
            if (playerManager == null)
                throw new ArgumentNullException("playerManager");
            if (banManager == null)
                throw new ArgumentNullException("banManager");
            if (createPlayerFunc == null)
                throw new ArgumentNullException("createPlayerFunc");

            PlayerManager = playerManager;
            BanManager = banManager;
            CreatePlayerFunc = createPlayerFunc;
        }

        protected virtual void PlayerConnectionLost(IPlayer player)
        {
            if (OnPlayerLeft != null)
                OnPlayerLeft(player, LeaveReasons.ConnectionLost);
        }

        #region IHost

        public event HostRegisterPlayerHandler OnPlayerRegistered;
        public event HostUnregisterPlayerHandler OnPlayerUnregistered;
        public event HostPublishMessageHandler OnMessagePublished;
        public event HostPlaceTetriminoHandler OnTetriminoPlaced;
        public event HostUseSpecialHandler OnUseSpecial;
        public event HostSendLinesHandler OnSendLines;
        public event HostModifyGridHandler OnGridModified;
        public event HostStartGameHandler OnStartGame;
        public event HostStopGameHandler OnStopGame;
        public event HostPauseGameHandler OnPauseGame;
        public event HostResumeGameHandler OnResumeGame;
        public event HostGameLostHandler OnGameLost;
        public event HostChangeOptionsHandler OnChangeOptions;
        public event HostKickPlayerHandler OnKickPlayer;
        public event HostBanPlayerHandler OnBanPlayer;
        public event HostResetWinListHandler OnResetWinList;

        public event PlayerLeftHandler OnPlayerLeft;

        public IBanManager BanManager { get; private set; }

        public abstract void Start();
        public abstract void Stop();
        public abstract void RemovePlayer(IPlayer player);

        #endregion

        #region ITetriNET

        public virtual void RegisterPlayer(ITetriNETCallback callback, string playerName)
        {
            Log.WriteLine("RegisterPlayer");

            // TODO: check ban list

            IPlayer player = null;
            int id = -1;
            lock (PlayerManager.LockObject)
            {
                if (!String.IsNullOrEmpty(playerName) && PlayerManager[playerName] == null && PlayerManager.PlayerCount < PlayerManager.MaxPlayers)
                {
                    player = CreatePlayerFunc(playerName, callback);
                    //
                    player.OnConnectionLost += PlayerConnectionLost;
                    //
                    id = PlayerManager.Add(player);
                }
            }
            if (id >= 0 && player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnPlayerRegistered != null)
                    OnPlayerRegistered(player, id);
            }
            else
            {
                Log.WriteLine("Register failed for player {0}", playerName);
                //
                callback.OnPlayerRegistered(false, -1, false);
            }
        }

        public virtual void UnregisterPlayer(ITetriNETCallback callback)
        {
            Log.WriteLine("UnregisterPlayer");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnPlayerUnregistered != null)
                    OnPlayerUnregistered(player);
            }
            else
            {
                Log.WriteLine("UnregisterPlayer from unknown player");
            }
        }

        public virtual void Heartbeat(ITetriNETCallback callback)
        {
            //Log.WriteLine("Heartbeat");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //Log.WriteLine("Heartbeat from {0}", player.Name);
                player.ResetTimeout(); // player alive
            }
            else
            {
                Log.WriteLine("Heartbeat from unknown player");
            }
        }

        public virtual void PublishMessage(ITetriNETCallback callback, string msg)
        {
            Log.WriteLine("PublishMessage {0}", msg);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnMessagePublished != null)
                    OnMessagePublished(player, msg);
            }
            else
            {
                Log.WriteLine("PublishMessage from unknown player");
            }
        }

        public virtual void PlaceTetrimino(ITetriNETCallback callback, int index, Tetriminos tetrimino, Orientations orientation, Position position, byte[] grid)
        {
            Log.WriteLine("PlaceTetrimino {0} {1} {2} {3} {4}", index, tetrimino, orientation, position, grid == null ? -1 : grid.Count(x => x > 0));

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                player.ResetTimeout(); // player alive
                //
                if (OnTetriminoPlaced != null)
                    OnTetriminoPlaced(player, index, tetrimino, orientation, position, grid);
            }
            else
            {
                Log.WriteLine("PlaceTetrimino from unknown player");
            }
        }

        public virtual void UseSpecial(ITetriNETCallback callback, int targetId, Specials special)
        {
            Log.WriteLine("UseSpecial {0} {1}", targetId, special);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                IPlayer target = PlayerManager[targetId];
                if (target != null)
                {
                    //
                    if (OnUseSpecial != null)
                        OnUseSpecial(player, target, special);
                }
                else
                    Log.WriteLine("UseSpecial to unknown player {0} from {1}", targetId, player.Name);
            }
            else
            {
                Log.WriteLine("UseSpecial from unknown player {0}");
            }
        }

        public virtual void SendLines(ITetriNETCallback callback, int count)
        {
            Log.WriteLine("SendLines {0}", count);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnSendLines != null)
                    OnSendLines(player, count);
            }
        }

        public virtual void ModifyGrid(ITetriNETCallback callback, byte[] grid)
        {
            Log.WriteLine("ModifyGrid {0}", grid == null ? -1 : grid.Count(x => x > 0));

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnGridModified != null)
                    OnGridModified(player, grid);
            }
        }

        public virtual void StartGame(ITetriNETCallback callback)
        {
            Log.WriteLine("StartGame");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnStartGame != null)
                    OnStartGame(player);
            }
        }

        public virtual void StopGame(ITetriNETCallback callback)
        {
            Log.WriteLine("StopGame");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnStopGame != null)
                    OnStopGame(player);
            }
        }

        public virtual void PauseGame(ITetriNETCallback callback)
        {
            Log.WriteLine("PauseGame");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnPauseGame != null)
                    OnPauseGame(player);
            }
        }

        public virtual void ResumeGame(ITetriNETCallback callback)
        {
            Log.WriteLine("ResumeGame");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnResumeGame != null)
                    OnResumeGame(player);
            }
        }

        public virtual void GameLost(ITetriNETCallback callback)
        {
            Log.WriteLine("GameLost");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnGameLost != null)
                    OnGameLost(player);
            }
        }

        public virtual void ChangeOptions(ITetriNETCallback callback, GameOptions options)
        {
            Log.WriteLine("ChangeOptions {0}", options);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnChangeOptions != null)
                    OnChangeOptions(player, options);
            }
        }

        public virtual void KickPlayer(ITetriNETCallback callback, int playerId)
        {
            Log.WriteLine("KickPlayer {0}", playerId);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnKickPlayer != null)
                    OnKickPlayer(player, playerId);
            }
        }

        public virtual void BanPlayer(ITetriNETCallback callback, int playerId)
        {
            Log.WriteLine("BanPlayer {0}", playerId);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnBanPlayer != null)
                    OnBanPlayer(player, playerId);
            }
        }

        public virtual void ResetWinList(ITetriNETCallback callback)
        {
            Log.WriteLine("ResetWinList");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnResetWinList != null)
                    OnResetWinList(player);
            }
        }

        #endregion
    }
}
