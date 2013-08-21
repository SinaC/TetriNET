using System;
using System.Linq;
using TetriNET.Common;
using TetriNET.Common.Contracts;
using TetriNET.Server.Ban;
using TetriNET.Server.Player;

namespace TetriNET.Server.Host
{
    public abstract class GenericHost : IHost
    {
        protected readonly Func<string, ITetriNETCallback, IPlayer> CreatePlayerFunc;
        protected readonly IPlayerManager PlayerManager;

        protected GenericHost(IPlayerManager playerManager, IBanManager banManager, Func<string, ITetriNETCallback, IPlayer> createPlayerFunc)
        {
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

        public event RegisterPlayerHandler OnPlayerRegistered;
        public event UnregisterPlayerHandler OnPlayerUnregistered;
        public event PublishMessageHandler OnMessagePublished;
        public event PlaceTetriminoHandler OnTetriminoPlaced;
        public event UseSpecialHandler OnUseSpecial;
        public event SendLinesHandler OnSendLines;
        public event ModifyGridHandler OnGridModified;
        public event StartGameHandler OnStartGame;
        public event StopGameHandler OnStopGame;
        public event PauseGameHandler OnPauseGame;
        public event ResumeGameHandler OnResumeGame;
        public event GameLostHandler OnGameLost;
        public event ChangeOptionsHandler OnChangeOptions;
        public event KickPlayerHandler OnKickPlayer;
        public event BanPlayerHandler OnBanPlayer;
        public event ResetWinListHandler OnResetWinList;

        public event PlayerLeftHandler OnPlayerLeft;

        public IBanManager BanManager { get; private set; }

        public abstract void Start();
        public abstract void Stop();
        public abstract void RemovePlayer(IPlayer player);
        public abstract void BanPlayer(IPlayer player);

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
            if (id >= 0)
            {
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
            Log.WriteLine("PlaceTetrimino {0} {1} {2} {3} {4}", index, tetrimino, orientation, position, grid.Count(x => x > 0));

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
                player.ResetTimeout();

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
                if (OnSendLines != null)
                    OnSendLines(player, count);
            }
        }

        public virtual void ModifyGrid(ITetriNETCallback callback, byte[] grid)
        {
            Log.WriteLine("ModifyGrid {0}", grid.Count(x => x > 0));

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
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
                player.ResetTimeout();
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
                player.ResetTimeout();
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
                player.ResetTimeout();
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
                player.ResetTimeout();
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
                player.ResetTimeout();
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
                player.ResetTimeout();
                //
                if (OnChangeOptions != null)
                    OnChangeOptions(player, options);
            }
        }

        public virtual void KickPlayer(ITetriNETCallback callback, int playerId)
        {
            Log.WriteLine("KickPlayer");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                player.ResetTimeout();
                //
                if (OnKickPlayer != null)
                    OnKickPlayer(player, playerId);
            }
        }

        public virtual void BanPlayer(ITetriNETCallback callback, int playerId)
        {
            Log.WriteLine("BanPlayer");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                player.ResetTimeout();
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
                player.ResetTimeout();
                //
                if (OnResetWinList != null)
                    OnResetWinList(player);
            }
        }

        #endregion
    }
}
