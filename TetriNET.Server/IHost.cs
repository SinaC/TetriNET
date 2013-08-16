using TetriNET.Common;

namespace TetriNET.Server
{
    public delegate void RegisterPlayerHandler(IPlayer player, int id);
    public delegate void UnregisterPlayerHandler(IPlayer player);
    public delegate void PublishMessageHandler(IPlayer player, string msg);
    public delegate void PlaceTetriminoHandler(IPlayer player, int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid);
    public delegate void SendAttackHandler(IPlayer player, IPlayer target, Attacks attack);

    public interface IHost : ITetriNET
    {
        event RegisterPlayerHandler OnPlayerRegistered;
        event UnregisterPlayerHandler OnPlayerUnregistered;
        event PublishMessageHandler OnMessagePublished;
        event PlaceTetriminoHandler OnTetriminoPlaced;
        event SendAttackHandler OnAttackSent;

        void Start();
        void Stop();
    }
}
