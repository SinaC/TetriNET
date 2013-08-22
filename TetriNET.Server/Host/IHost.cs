using TetriNET.Common;
using TetriNET.Common.Contracts;
using TetriNET.Server.Ban;
using TetriNET.Server.Player;

namespace TetriNET.Server.Host
{
    public delegate void RegisterPlayerHandler(IPlayer player, int playerId);
    public delegate void UnregisterPlayerHandler(IPlayer player);
    public delegate void PublishMessageHandler(IPlayer player, string msg);
    public delegate void PlaceTetriminoHandler(IPlayer player, int index, Tetriminos tetrimino, Orientations orientation, Position position, byte[] grid);
    public delegate void UseSpecialHandler(IPlayer player, IPlayer target, Specials special);
    public delegate void SendLinesHandler(IPlayer player, int count);
    public delegate void ModifyGridHandler(IPlayer player, byte[] grid);
    public delegate void StartGameHandler(IPlayer player);
    public delegate void StopGameHandler(IPlayer player);
    public delegate void PauseGameHandler(IPlayer player);
    public delegate void ResumeGameHandler(IPlayer player);
    public delegate void GameLostHandler(IPlayer player);
    public delegate void ChangeOptionsHandler(IPlayer player, GameOptions options);
    public delegate void KickPlayerHandler(IPlayer player, int playerId);
    public delegate void BanPlayerHandler(IPlayer player, int playerId);
    public delegate void ResetWinListHandler(IPlayer player);

    public delegate void PlayerLeftHandler(IPlayer player, LeaveReasons reason);

    public interface IHost : ITetriNET
    {
        event RegisterPlayerHandler OnPlayerRegistered;
        event UnregisterPlayerHandler OnPlayerUnregistered;
        event PublishMessageHandler OnMessagePublished;
        event PlaceTetriminoHandler OnTetriminoPlaced;
        event UseSpecialHandler OnUseSpecial;
        event SendLinesHandler OnSendLines;
        event ModifyGridHandler OnGridModified;
        event StartGameHandler OnStartGame;
        event StopGameHandler OnStopGame;
        event PauseGameHandler OnPauseGame;
        event ResumeGameHandler OnResumeGame;
        event GameLostHandler OnGameLost;
        event ChangeOptionsHandler OnChangeOptions;
        event KickPlayerHandler OnKickPlayer;
        event BanPlayerHandler OnBanPlayer;
        event ResetWinListHandler OnResetWinList;

        event PlayerLeftHandler OnPlayerLeft;

        IBanManager BanManager { get; }

        void Start();
        void Stop();
        void RemovePlayer(IPlayer player); // Should be overridden to handle local table storing reference to player
    }
}
