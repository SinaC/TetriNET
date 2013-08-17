using TetriNET.Common;

namespace TetriNET.Server
{
    public delegate void RegisterPlayerHandler(IPlayer player, int playerId);
    public delegate void UnregisterPlayerHandler(IPlayer player, int playerId);
    public delegate void PublishMessageHandler(IPlayer player, string msg);
    public delegate void PlaceTetriminoHandler(IPlayer player, int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid);
    public delegate void SendAttackHandler(IPlayer player, IPlayer target, Attacks attack);
    public delegate void SendLinesHandler(IPlayer player, int count);
    public delegate void ModifyGridHandler(IPlayer player, PlayerGrid grid);
    public delegate void StartGameHandler(IPlayer player);
    public delegate void StopGameHandler(IPlayer player);
    public delegate void PauseGameHandler(IPlayer player);
    public delegate void ResumeGameHandler(IPlayer player);
    public delegate void GameLostHandler(IPlayer player);
    public delegate void ChangeOptionsHandler(IPlayer player, GameOptions options);
    public delegate void KickPlayerHandler(IPlayer player, int playerId);
    public delegate void BanPlayerHandler(IPlayer player, int playerId);

    public interface IHost : ITetriNET
    {
        event RegisterPlayerHandler OnPlayerRegistered;
        event UnregisterPlayerHandler OnPlayerUnregistered;
        event PublishMessageHandler OnMessagePublished;
        event PlaceTetriminoHandler OnTetriminoPlaced;
        event SendAttackHandler OnSendAttack;
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

        // TODO: add reference to BanList

        void Start();
        void Stop();
    }
}
