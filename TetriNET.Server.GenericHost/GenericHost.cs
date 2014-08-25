using System;
using System.Linq;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.GenericHost
{
    public abstract class GenericHost : IHost
    {
        protected readonly Func<int, string, ITetriNETCallback, IPlayer> CreatePlayerFunc;
        protected readonly Func<int, string, ITetriNETCallback, ISpectator> CreateSpectatorFunc;

        protected GenericHost(IPlayerManager playerManager, ISpectatorManager spectatorManager, IBanManager banManager, Func<int, string, ITetriNETCallback, IPlayer> createPlayerFunc, Func<int, string, ITetriNETCallback, ISpectator> createSpectatorFunc)
        {
            if (playerManager == null)
                throw new ArgumentNullException("playerManager");
            if (banManager == null)
                throw new ArgumentNullException("banManager");
            if (createPlayerFunc == null)
                throw new ArgumentNullException("createPlayerFunc");

            PlayerManager = playerManager;
            SpectatorManager = spectatorManager;
            BanManager = banManager;
            CreatePlayerFunc = createPlayerFunc;
            CreateSpectatorFunc = createSpectatorFunc;
        }

        protected virtual void PlayerOnConnectionLost(IPlayer player)
        {
            HostPlayerLeft.Do(x => x(player, LeaveReasons.ConnectionLost));
        }

        protected virtual void SpectatorOnConnectionLost(ISpectator spectator)
        {
            HostSpectatorLeft.Do(x => x(spectator, LeaveReasons.ConnectionLost));
        }

        #region IHost

        public event HostRegisterPlayerEventHandler HostPlayerRegistered;
        public event HostUnregisterPlayerEventHandler HostPlayerUnregistered;
        public event HostPlayerTeamEventHandler HostPlayerTeamChanged;
        public event HostPublishMessageEventHandler HostMessagePublished;
        public event HostPlacePieceEventHandler HostPiecePlaced;
        public event HostUseSpecialEventHandler HostUseSpecial;
        public event HostClearLinesEventHandler HostClearLines;
        public event HostModifyGridEventHandler HostGridModified;
        public event HostStartGameEventHandler HostStartGame;
        public event HostStopGameEventHandler HostStopGame;
        public event HostPauseGameEventHandler HostPauseGame;
        public event HostResumeGameEventHandler HostResumeGame;
        public event HostGameLostEventHandler HostGameLost;
        public event HostChangeOptionsEventHandler HostChangeOptions;
        public event HostKickPlayerEventHandler HostKickPlayer;
        public event HostBanPlayerEventHandler HostBanPlayer;
        public event HostResetWinListEventHandler HostResetWinList;
        public event HostFinishContinuousSpecialEventHandler HostFinishContinuousSpecial;
        public event HostEarnAchievementEventHandler HostEarnAchievement;
        
        public event HostRegisterSpectatorEventHandler HostSpectatorRegistered;
        public event HostUnregisterSpectatorEventHandler HostSpectatorUnregistered;
        public event HostPublishSpectatorMessageEventHandler HostSpectatorMessagePublished;

        public event PlayerLeftEventHandler HostPlayerLeft;
        public event SpectatorLeftEventHandler HostSpectatorLeft;

        public IBanManager BanManager { get; private set; }
        public IPlayerManager PlayerManager { get; private set; }
        public ISpectatorManager SpectatorManager { get; private set; }

        public abstract void Start();
        public abstract void Stop();
        public abstract void RemovePlayer(IPlayer player);
        public abstract void RemoveSpectator(ISpectator spectator);

        #endregion

        #region ITetriNET

        public virtual void RegisterPlayer(ITetriNETCallback callback, string playerName, string team)
        {
            Log.WriteLine(Log.LogLevels.Debug, "RegisterPlayer {0} {1}", playerName, team);

            // TODO: check ban list

            RegistrationResults result = RegistrationResults.RegistrationSuccessful;
            IPlayer player = null;
            int id = -1;
            bool added = false;
            lock (PlayerManager.LockObject)
            {
                if (String.IsNullOrEmpty(playerName) || playerName.Length > 20)
                {
                    result = RegistrationResults.RegistrationFailedInvalidName;
                    Log.WriteLine(Log.LogLevels.Warning, "Cannot register {0} because name is invalid", playerName);
                }
                else if (PlayerManager[playerName] != null || SpectatorManager[playerName] != null)
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
                    id = PlayerManager.FirstAvailableId;
                    if (id != -1)
                    {
                        player = CreatePlayerFunc(id, playerName, callback);
                        //
                        player.ConnectionLost += PlayerOnConnectionLost;
                        player.Team = team;
                        //
                        added = PlayerManager.Add(player);
                    }
                }
            }
            if (added && id != -1 && result == RegistrationResults.RegistrationSuccessful)
            {
                //
                player.ResetTimeout(); // player alive
                //
                HostPlayerRegistered.Do(x => x(player, id));
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
                HostPlayerUnregistered.Do(x => x(player));
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
            Log.WriteLine(Log.LogLevels.Debug, "PlayerTeam {0}", team);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                HostPlayerTeamChanged.Do(x => x(player, team));
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
                HostMessagePublished.Do(x => x(player, msg));
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
                HostPiecePlaced.Do(x => x(player, pieceIndex, highestIndex, piece, orientation, posX, posY, grid));
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
                    HostUseSpecial.Do(x => x(player, target, special));
                }
                else
                    Log.WriteLine(Log.LogLevels.Warning, "UseSpecial to unknown player {0} from {1}", targetId, player.Name);
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "UseSpecial from unknown player");
            }
        }

        public virtual void ClearLines(ITetriNETCallback callback, int count)
        {
            Log.WriteLine(Log.LogLevels.Debug, "ClearLines {0}", count);

            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                //
                player.ResetTimeout(); // player alive
                //
                HostClearLines.Do(x => x(player, count));
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "ClearLines from unknown player");
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
                HostGridModified.Do(x => x(player, grid));
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
                HostStartGame.Do(x => x(player));
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
                HostStopGame.Do(x => x(player));
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
                HostPauseGame.Do(x => x(player));
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
                HostResumeGame.Do(x => x(player));
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
                HostGameLost.Do(x => x(player));
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
                HostFinishContinuousSpecial.Do(x => x(player, special));
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
                HostChangeOptions.Do(x => x(player, options));
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
                HostKickPlayer.Do(x => x(player, playerId));
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
                HostBanPlayer.Do(x => x(player, playerId));
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
                HostResetWinList.Do(x => x(player));
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
                HostEarnAchievement.Do(x => x(player, achievementId, achievementTitle));
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "EarnAchievement from unknown player");
            }
        }

        #endregion

        #region ITetriNETSpectator

        public virtual void RegisterSpectator(ITetriNETCallback callback, string spectatorName)
        {
            Log.WriteLine(Log.LogLevels.Debug, "RegisterSpectator {0}", spectatorName);

            // TODO: check ban list

            RegistrationResults result = RegistrationResults.RegistrationSuccessful;
            ISpectator spectator = null;
            int id = -1;
            bool added = false;
            lock (SpectatorManager.LockObject)
            {
                if (String.IsNullOrEmpty(spectatorName) || spectatorName.Length > 20)
                {
                    result = RegistrationResults.RegistrationFailedInvalidName;
                    Log.WriteLine(Log.LogLevels.Warning, "Cannot register {0} because name is invalid", spectatorName);
                }
                else if (SpectatorManager[spectatorName] != null || PlayerManager[spectatorName] != null)
                {
                    result = RegistrationResults.RegistrationFailedPlayerAlreadyExists;
                    Log.WriteLine(Log.LogLevels.Warning, "Cannot register {0} because it already exists", spectatorName);
                }
                else if (SpectatorManager.SpectatorCount >= SpectatorManager.MaxSpectators)
                {
                    result = RegistrationResults.RegistrationFailedTooManyPlayers;
                    Log.WriteLine(Log.LogLevels.Warning, "Cannot register {0} because too many spectators are already connected", spectatorName);
                }
                else
                {
                    id = SpectatorManager.FirstAvailableId;
                    if (id != -1)
                    {
                        spectator = CreateSpectatorFunc(id, spectatorName, callback);
                        //
                        spectator.ConnectionLost += SpectatorOnConnectionLost;
                        //
                        added = SpectatorManager.Add(spectator);
                    }
                }
            }
            if (added && id != -1 && result == RegistrationResults.RegistrationSuccessful)
            {
                //
                spectator.ResetTimeout(); // spectator alive
                //
                HostSpectatorRegistered.Do(x => x(spectator, id));
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Info, "Register failed for spectator {0}", spectatorName);
                //
                callback.OnSpectatorRegistered(result, -1, false, null);
            }
        }

        public virtual void UnregisterSpectator(ITetriNETCallback callback)
        {
            Log.WriteLine(Log.LogLevels.Debug, "UnregisterPlayer");

            ISpectator spectator = SpectatorManager[callback];
            if (spectator != null)
            {
                //
                spectator.ResetTimeout(); // player alive
                //
                HostSpectatorUnregistered.Do(x => x(spectator));
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "UnregisterSpectator from unknown spectator");
            }
        }

        public virtual void HeartbeatSpectator(ITetriNETCallback callback)
        {
            //Log.WriteLine(Log.LogLevels.Debug, "Heartbeat");

            ISpectator spectator = SpectatorManager[callback];
            if (spectator != null)
            {
                //Log.WriteLine("Heartbeat from {0}", player.Name);
                spectator.ResetTimeout(); // player alive
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "HeartbeatSpectator from unknown spectator");
            }
        }

        public virtual void PublishSpectatorMessage(ITetriNETCallback callback, string msg)
        {
            Log.WriteLine(Log.LogLevels.Debug, "PublishMessage {0}", msg);

            ISpectator spectator = SpectatorManager[callback];
            if (spectator != null)
            {
                //
                spectator.ResetTimeout(); // player alive
                //
                HostSpectatorMessagePublished.Do(x => x(spectator, msg));
            }
            else
            {
                Log.WriteLine(Log.LogLevels.Warning, "OnSpectatorMessagePublished from unknown spectator");
            }
        }

        #endregion
    }
}
