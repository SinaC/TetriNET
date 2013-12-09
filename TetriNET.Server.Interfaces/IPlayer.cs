using System;
using TetriNET.Common.Contracts;

namespace TetriNET.Server.Interfaces
{
    public enum PlayerStates
    {
        Registered,
        Playing,
        GameLost
    };

    public delegate void ConnectionLostHandler(IPlayer player);

    public interface IPlayer : ITetriNETCallback
    {
        event ConnectionLostHandler OnConnectionLost;

        string Name { get; }
        string Team { get; set; }
        int PieceIndex { get; set; }
        byte[] Grid { get; set; }

        //
        PlayerStates State { get; set; }
        DateTime LossTime { get; set; }

        //
        ITetriNETCallback Callback { get; } // should never be used by anything else then IPlayerManager/ISpectatorManager and IPlayer/ISpectator

        // Heartbeat management
        DateTime LastActionToClient { get; } // used to check if heartbeat is needed

        // Timeout management
        DateTime LastActionFromClient { get; }
        int TimeoutCount { get; }

        void ResetTimeout();
        void SetTimeout();

    }
}
