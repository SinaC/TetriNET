using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;

namespace TetriNET.Server.Interfaces
{
    public delegate void HostRegisterPlayerHandler(IPlayer player, int playerId);
    public delegate void HostUnregisterPlayerHandler(IPlayer player);
    public delegate void HostPlayerTeamHandler(IPlayer player, string team);
    public delegate void HostPublishMessageHandler(IPlayer player, string msg);
    public delegate void HostPlacePieceHandler(IPlayer player, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);
    public delegate void HostUseSpecialHandler(IPlayer player, IPlayer target, Specials special);
    public delegate void HostSendLinesHandler(IPlayer player, int count);
    public delegate void HostModifyGridHandler(IPlayer player, byte[] grid);
    public delegate void HostStartGameHandler(IPlayer player);
    public delegate void HostStopGameHandler(IPlayer player);
    public delegate void HostPauseGameHandler(IPlayer player);
    public delegate void HostResumeGameHandler(IPlayer player);
    public delegate void HostGameLostHandler(IPlayer player);
    public delegate void HostChangeOptionsHandler(IPlayer player, GameOptions options);
    public delegate void HostKickPlayerHandler(IPlayer player, int playerId);
    public delegate void HostBanPlayerHandler(IPlayer player, int playerId);
    public delegate void HostResetWinListHandler(IPlayer player);
    public delegate void HostFinishContinuousSpecialHandler(IPlayer player, Specials special);
    public delegate void HostEarnAchievementHandler(IPlayer player, int achievementId, string achievementTitle);

    public delegate void PlayerLeftHandler(IPlayer player, LeaveReasons reason);

    public interface IHost : ITetriNET
    {
        event HostRegisterPlayerHandler OnPlayerRegistered;
        event HostUnregisterPlayerHandler OnPlayerUnregistered;
        event HostPlayerTeamHandler OnPlayerTeamChanged;
        event HostPublishMessageHandler OnMessagePublished;
        event HostPlacePieceHandler OnPiecePlaced;
        event HostUseSpecialHandler OnUseSpecial;
        event HostSendLinesHandler OnSendLines;
        event HostModifyGridHandler OnGridModified;
        event HostStartGameHandler OnStartGame;
        event HostStopGameHandler OnStopGame;
        event HostPauseGameHandler OnPauseGame;
        event HostResumeGameHandler OnResumeGame;
        event HostGameLostHandler OnGameLost;
        event HostChangeOptionsHandler OnChangeOptions;
        event HostKickPlayerHandler OnKickPlayer;
        event HostBanPlayerHandler OnBanPlayer;
        event HostResetWinListHandler OnResetWinList;
        event HostFinishContinuousSpecialHandler OnFinishContinuousSpecial;
        event HostEarnAchievementHandler OnEarnAchievement;

        event PlayerLeftHandler OnPlayerLeft;

        IBanManager BanManager { get; }
        IPlayerManager PlayerManager { get; }

        void Start();
        void Stop();
        void RemovePlayer(IPlayer player); // Should be overridden to handle local table storing reference to player
    }
}
