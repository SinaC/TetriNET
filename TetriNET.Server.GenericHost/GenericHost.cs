using System;
using System.Linq;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.GenericHost
{
    public abstract class GenericHost : IHost
    {
        protected readonly Func<string, ITetriNETCallback, IPlayer> CreatePlayerFunc;

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

        protected virtual void PlayerOnConnectionLost(IPlayer player)
        {
            if (OnPlayerLeft != null)
                OnPlayerLeft(player, LeaveReasons.ConnectionLost);
        }

        #region IHost

        public event HostRegisterPlayerHandler OnPlayerRegistered;
        public event HostUnregisterPlayerHandler OnPlayerUnregistered;
        public event HostPlayerTeamHandler OnPlayerTeamChanged;
        public event HostPublishMessageHandler OnMessagePublished;
        public event HostPlacePieceHandler OnPiecePlaced;
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
        public event HostFinishContinuousSpecialHandler OnFinishContinuousSpecial;
        public event HostEarnAchievementHandler OnEarnAchievement;

        public event PlayerLeftHandler OnPlayerLeft;

        public IBanManager BanManager { get; private set; }
        public IPlayerManager PlayerManager { get; private set; }

        public abstract void Start();
        public abstract void Stop();
        public abstract void RemovePlayer(IPlayer player);

        #endregion

        #region ITetriNET

        public virtual void RegisterPlayer(ITetriNETCallback callback, string playerName)
        {
            Log.WriteLine(Log.LogLevels.Debug, "RegisterPlayer");

            // TODO: check ban list

            RegistrationResults result = RegistrationResults.RegistrationSuccessful;
            IPlayer player = null;
            int id = -1;
            lock (PlayerManager.LockObject)
            {
                if (String.IsNullOrEmpty(playerName) || playerName.Length > 20)
                {
                    result = RegistrationResults.RegistrationFailedInvalidName;
                    Log.WriteLine(Log.LogLevels.Warning, "Cannot register {0} because name is invalid", playerName);
                }
                else if (PlayerManager[playerName] != null)
                {
                    result = RegistrationResults.RegistrationFailedPlayerAlreadyExists;
                    Log.WriteLine(Log.LogLevels.Warning, "Cannot register {0} because it already exists", playerName);
                }
                else if (PlayerManager.PlayerCount >= PlayerManager.MaxPlayers)
                {
                    result = RegistrationResults.RegistrationFailedTooManyPlayers;
                    Log.WriteLine(Log.LogLevels.Warning, "Cannot register {0} because too many players are already connected", playerName);
                }
                else
                {
                    player = CreatePlayerFunc(playerName, callback);
                    //
                    player.OnConnectionLost += PlayerOnConnectionLost;
                    //
                    id = PlayerManager.Add(player);
                }
            }
            if (id >= 0 && player != null && result == RegistrationResults.RegistrationSuccessful)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnPlayerRegistered != null)
                    OnPlayerRegistered(player, id);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Info, "Register failed for player {0}", playerName);
                //
                callback.OnPlayerRegistered(result, -1, false, false, null);
            }
        }

        public virtual void UnregisterPlayer(ITetriNETCallback callback)
        {
            Log.WriteLine(Log.LogLevels.Debug, "UnregisterPlayer");

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
                Log.WriteLine(Log.LogLevels.Warning, "UnregisterPlayer from unknown player");
            }
        }

        public virtual void Heartbeat(ITetriNETCallback callback)
        {
            //Log.WriteLine(Log.LogLevels.Debug, "Heartbeat");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //Log.WriteLine("Heartbeat from {0}", player.Name);
                player.ResetTimeout(); // player alive
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "Heartbeat from unknown player");
            }
        }

        public virtual void PlayerTeam(ITetriNETCallback callback, string team)
        {
            Log.WriteLine(Log.LogLevels.Debug, "PlayerTeam");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnPlayerTeamChanged != null)
                    OnPlayerTeamChanged(player, team);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "Heartbeat from unknown player");
            }
        }

        public virtual void PublishMessage(ITetriNETCallback callback, string msg)
        {
            Log.WriteLine(Log.LogLevels.Debug, "PublishMessage {0}", msg);

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
                Log.WriteLine(Log.LogLevels.Warning, "PublishMessage from unknown player");
            }
        }

        public virtual void PlacePiece(ITetriNETCallback callback, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            Log.WriteLine(Log.LogLevels.Debug, "PlacePiece {0} {1} {2} {3} {4},{5} {6}", pieceIndex, highestIndex, piece, orientation, posX, posY, grid == null ? -1 : grid.Count(x => x > 0));

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                player.ResetTimeout(); // player alive
                //
                if (OnPiecePlaced != null)
                    OnPiecePlaced(player, pieceIndex, highestIndex, piece, orientation, posX, posY, grid);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "PlacePiece from unknown player");
            }
        }

        public virtual void UseSpecial(ITetriNETCallback callback, int targetId, Specials special)
        {
            Log.WriteLine(Log.LogLevels.Debug, "UseSpecial {0} {1}", targetId, special);

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
                    Log.WriteLine(Log.LogLevels.Warning, "UseSpecial to unknown player {0} from {1}", targetId, player.Name);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "UseSpecial from unknown player");
            }
        }

        public virtual void SendLines(ITetriNETCallback callback, int count)
        {
            Log.WriteLine(Log.LogLevels.Debug, "SendLines {0}", count);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnSendLines != null)
                    OnSendLines(player, count);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "SendLines from unknown player");
            }
        }

        public virtual void ModifyGrid(ITetriNETCallback callback, byte[] grid)
        {
            Log.WriteLine(Log.LogLevels.Debug, "ModifyGrid {0}", grid == null ? -1 : grid.Count(x => x > 0));

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnGridModified != null)
                    OnGridModified(player, grid);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "ModifyGrid from unknown player");
            }
        }

        public virtual void StartGame(ITetriNETCallback callback)
        {
            Log.WriteLine(Log.LogLevels.Debug, "StartGame");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnStartGame != null)
                    OnStartGame(player);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "StartGame from unknown player");
            }
        }

        public virtual void StopGame(ITetriNETCallback callback)
        {
            Log.WriteLine(Log.LogLevels.Debug, "StopGame");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnStopGame != null)
                    OnStopGame(player);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "StopGame from unknown player");
            }
        }

        public virtual void PauseGame(ITetriNETCallback callback)
        {
            Log.WriteLine(Log.LogLevels.Debug, "PauseGame");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnPauseGame != null)
                    OnPauseGame(player);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "PauseGame from unknown player");
            }
        }

        public virtual void ResumeGame(ITetriNETCallback callback)
        {
            Log.WriteLine(Log.LogLevels.Debug, "ResumeGame");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnResumeGame != null)
                    OnResumeGame(player);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "ResumeGame from unknown player");
            }
        }

        public virtual void GameLost(ITetriNETCallback callback)
        {
            Log.WriteLine(Log.LogLevels.Debug, "GameLost");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnGameLost != null)
                    OnGameLost(player);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "GameLost from unknown player");
            }
        }

        public virtual void FinishContinuousSpecial(ITetriNETCallback callback, Specials special)
        {
            Log.WriteLine(Log.LogLevels.Debug, "FinishContinuousSpecial {0}", special);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnFinishContinuousSpecial != null)
                    OnFinishContinuousSpecial(player, special);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "FinishContinuousSpecial from unknown player");
            }
        }

        public virtual void ChangeOptions(ITetriNETCallback callback, GameOptions options)
        {
            Log.WriteLine(Log.LogLevels.Debug, "ChangeOptions {0}", options);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnChangeOptions != null)
                    OnChangeOptions(player, options);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "ChangeOptions from unknown player");
            }
        }

        public virtual void KickPlayer(ITetriNETCallback callback, int playerId)
        {
            Log.WriteLine(Log.LogLevels.Debug, "KickPlayer {0}", playerId);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnKickPlayer != null)
                    OnKickPlayer(player, playerId);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "KickPlayer from unknown player");
            }
        }

        public virtual void BanPlayer(ITetriNETCallback callback, int playerId)
        {
            Log.WriteLine(Log.LogLevels.Debug, "BanPlayer {0}", playerId);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnBanPlayer != null)
                    OnBanPlayer(player, playerId);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "BanPlayer from unknown player");
            }
        }

        public virtual void ResetWinList(ITetriNETCallback callback)
        {
            Log.WriteLine(Log.LogLevels.Debug, "ResetWinList");

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnResetWinList != null)
                    OnResetWinList(player);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "ResetWinList from unknown player");
            }
        }

        public virtual void EarnAchievement(ITetriNETCallback callback, int achievementId, string achievementTitle)
        {
            Log.WriteLine(Log.LogLevels.Debug, "EarnAchievement {0} {1}", achievementId, achievementTitle);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                if (OnEarnAchievement != null)
                    OnEarnAchievement(player, achievementId, achievementTitle);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "EarnAchievement from unknown player");
            }
        }

        #endregion
    }
}
