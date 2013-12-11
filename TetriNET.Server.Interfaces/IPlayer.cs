using System;

namespace TetriNET.Server.Interfaces
{
    public enum PlayerStates
    {
        Registered,
        Playing,
        GameLost
    };

    public delegate void ConnectionLostHandler(IPlayer player);

    public interface IPlayer : IEntity
    {
        event ConnectionLostHandler OnConnectionLost;

        string Name { get; }
        string Team { get; set; }
        int PieceIndex { get; set; }
        byte[] Grid { get; set; }

        //
        PlayerStates State { get; set; }
        DateTime LossTime { get; set; }
    }
}
