﻿using TetriNET.Common;

namespace TetriNET.Server
{
    public delegate void RegisterPlayerHandler(IPlayer player, int id);
    public delegate void UnregisterPlayerHandler(IPlayer player);
    public delegate void PublishMessageHandler(IPlayer player, string msg);
    public delegate void PlaceTetriminoHandler(IPlayer player, Tetriminos tetrimino, Orientations orientation, Position position);
    public delegate void SendAttackHandler(IPlayer player, IPlayer target, Attacks attack);

    public interface IHost : ITetriNET
    {
        event RegisterPlayerHandler OnPlayerRegistered;
        event UnregisterPlayerHandler OnPlayerUnregistered;
        event PublishMessageHandler OnMessagePublished;
        event PlaceTetriminoHandler OnTetriminoPlaced;
        event SendAttackHandler OnAttackSent;

        IPlayerManager PlayerManager { get; }

        void Start(string port);
        void Stop();
    }
}