using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public event SendAttackHandler OnSendAttack;
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

        public abstract void Start();
        public abstract void Stop();

        #endregion

        #region ITetriNET

        public virtual void RegisterPlayer(ITetriNETCallback callback, string playerName)
        {
            Log.WriteLine("RegisterPlayer");

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
                //int id;
                //lock (PlayerManager.LockObject)
                //{
                //    // Get id
                //    id = PlayerManager.GetId(player);
                //    // Remove player from player list
                //    PlayerManager.Remove(player);
                //}
                ////
                //if (OnPlayerUnregistered != null)
                //    OnPlayerUnregistered(player, id);
                PlayerConnectionLost(player);
            }
            else
            {
                Log.WriteLine("UnregisterPlayer from unknown player");
            }
        }

        public virtual void Heartbeat(ITetriNETCallback callback)
        {
            Log.WriteLine("Heartbeat");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                Log.WriteLine("Heartbeat from {0}", player.Name);
                player.LastAction = DateTime.Now; // player alive
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
            Log.WriteLine("PlaceTetrimino {0} {1} {2} {3} {4}", index, tetrimino, orientation, position, grid.Data.Count(x => x > 0));

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
            Log.WriteLine("SendAttack {0} {1}", targetId, attack);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                player.LastAction = DateTime.Now;

                IPlayer target = PlayerManager[targetId];
                if (target != null)
                {
                    //
                    if (OnSendAttack != null)
                        OnSendAttack(player, target, attack);
                }
                else
                    Log.WriteLine("SendAttack to unknown player {0} from {1}", targetId, player.Name);
            }
            else
            {
                Log.WriteLine("SendAttack from unknown player {0}");
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

        public virtual void ModifyGrid(ITetriNETCallback callback, PlayerGrid grid)
        {
            Log.WriteLine("ModifyGrid {0}", grid.Data.Count(x => x > 0));

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
                player.LastAction = DateTime.Now;
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
                player.LastAction = DateTime.Now;
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
                player.LastAction = DateTime.Now;
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
                player.LastAction = DateTime.Now;
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
                player.LastAction = DateTime.Now;
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
                player.LastAction = DateTime.Now;
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
                player.LastAction = DateTime.Now;
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
                player.LastAction = DateTime.Now;
                //
                if (OnBanPlayer != null)
                    OnBanPlayer(player, playerId);
            }
        }

        #endregion

        protected virtual void PlayerConnectionLost(IPlayer player)
        {
            Log.WriteLine("PlayerConnectionLost:{0}", player.Name);
            lock (PlayerManager.LockObject)
            {
                // Get id
                int id = PlayerManager.GetId(player);
                // Remove player from player list
                PlayerManager.Remove(player);
                //
                if (OnPlayerUnregistered != null)
                    OnPlayerUnregistered(player, id);
            }
        }
    }
}
