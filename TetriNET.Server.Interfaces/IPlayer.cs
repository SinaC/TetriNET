using System;

namespace TetriNET.Server.Interfaces
{
    public enum PlayerStates
    {
        Registered,
        Playing,
        GameLost
    };

    public delegate void ConnectionLostEventHandler(IPlayer player);

    public interface IPlayer : IEntity
    {
        event ConnectionLostEventHandler ConnectionLost;

        string Name { get; }
        string Team { get; set; }
        int PieceIndex { get; set; }
        byte[] Grid { get; set; }

        //
        PlayerStates State { get; set; }
        DateTime LossTime { get; set; }
    }
}
