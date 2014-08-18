using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;

namespace TetriNET.Server.Interfaces
{
    public delegate void HostRegisterPlayerEventHandler(IPlayer player, int playerId);
    public delegate void HostUnregisterPlayerEventHandler(IPlayer player);
    public delegate void HostPlayerTeamEventHandler(IPlayer player, string team);
    public delegate void HostPublishMessageEventHandler(IPlayer player, string msg);
    public delegate void HostPlacePieceEventHandler(IPlayer player, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);
    public delegate void HostUseSpecialEventHandler(IPlayer player, IPlayer target, Specials special);
    //public delegate void HostSendLinesEventHandler(IPlayer player, int count);
    public delegate void HostClearLinesEventHandler(IPlayer player, int count);
    public delegate void HostModifyGridEventHandler(IPlayer player, byte[] grid);
    public delegate void HostStartGameEventHandler(IPlayer player);
    public delegate void HostStopGameEventHandler(IPlayer player);
    public delegate void HostPauseGameEventHandler(IPlayer player);
    public delegate void HostResumeGameEventHandler(IPlayer player);
    public delegate void HostGameLostEventHandler(IPlayer player);
    public delegate void HostChangeOptionsEventHandler(IPlayer player, GameOptions options);
    public delegate void HostKickPlayerEventHandler(IPlayer player, int playerId);
    public delegate void HostBanPlayerEventHandler(IPlayer player, int playerId);
    public delegate void HostResetWinListEventHandler(IPlayer player);
    public delegate void HostFinishContinuousSpecialEventHandler(IPlayer player, Specials special);
    public delegate void HostEarnAchievementEventHandler(IPlayer player, int achievementId, string achievementTitle);

    public delegate void HostRegisterSpectatorEventHandler(ISpectator spectator, int spectatorId);
    public delegate void HostUnregisterSpectatorEventHandler(ISpectator spectator);
    public delegate void HostPublishSpectatorMessageEventHandler(ISpectator spectator, string msg);

    public delegate void PlayerLeftEventHandler(IPlayer player, LeaveReasons reason);
    public delegate void SpectatorLeftEventHandler(ISpectator spectator, LeaveReasons reason);

    public interface IHost : ITetriNET, ITetriNETSpectator
    {
        event HostRegisterPlayerEventHandler HostPlayerRegistered;
        event HostUnregisterPlayerEventHandler HostPlayerUnregistered;
        event HostPlayerTeamEventHandler HostPlayerTeamChanged;
        event HostPublishMessageEventHandler HostMessagePublished;
        event HostPlacePieceEventHandler HostPiecePlaced;
        event HostUseSpecialEventHandler HostUseSpecial;
        //event HostSendLinesEventHandler HostSendLines;
        event HostClearLinesEventHandler HostClearLines;
        event HostModifyGridEventHandler HostGridModified;
        event HostStartGameEventHandler HostStartGame;
        event HostStopGameEventHandler HostStopGame;
        event HostPauseGameEventHandler HostPauseGame;
        event HostResumeGameEventHandler HostResumeGame;
        event HostGameLostEventHandler HostGameLost;
        event HostChangeOptionsEventHandler HostChangeOptions;
        event HostKickPlayerEventHandler HostKickPlayer;
        event HostBanPlayerEventHandler HostBanPlayer;
        event HostResetWinListEventHandler HostResetWinList;
        event HostFinishContinuousSpecialEventHandler HostFinishContinuousSpecial;
        event HostEarnAchievementEventHandler HostEarnAchievement;

        event HostRegisterSpectatorEventHandler HostSpectatorRegistered;
        event HostUnregisterSpectatorEventHandler HostSpectatorUnregistered;
        event HostPublishSpectatorMessageEventHandler HostSpectatorMessagePublished;

        event PlayerLeftEventHandler HostPlayerLeft;
        event SpectatorLeftEventHandler HostSpectatorLeft;

        IBanManager BanManager { get; }
        IPlayerManager PlayerManager { get; }
        ISpectatorManager SpectatorManager { get; }

        void Start();
        void Stop();
        void RemovePlayer(IPlayer player); // Should be overridden to handle local table storing reference to player
        void RemoveSpectator(ISpectator spectator); // Should be overridden to handle local table storing reference to spectator
    }
}
