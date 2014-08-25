using System;

namespace TetriNET.Server.Interfaces
{
    public enum PlayerStates
    {
        Registered,
        Playing,
        GameLost
    };

    public interface IPlayer : IEntity
    {
        string Team { get; set; }
        int PieceIndex { get; set; }
        byte[] Grid { get; set; }

        //
        PlayerStates State { get; set; }
        DateTime LossTime { get; set; }
    }
}
