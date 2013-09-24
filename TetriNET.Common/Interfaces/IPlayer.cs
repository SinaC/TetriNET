using System;
using TetriNET.Common.Contracts;

namespace TetriNET.Common.Interfaces
{
    public delegate void PlayerConnectionLostHandler(IPlayer player);

    public enum PlayerStates
    {
        Registered,
        Playing,
        GameLost,
    };

    public interface IPlayer : ITetriNETCallback
    {
        event PlayerConnectionLostHandler OnConnectionLost;

        string Name { get; }
        int PieceIndex { get; set; }
        byte[] Grid { get; set; }
        //
        ITetriNETCallback Callback { get; } // should never be used by anything else then IPlayerManager and IPlayer
        //
        PlayerStates State { get; set; }
        DateTime LossTime { get; set; }

        // Heartbeat management
        DateTime LastActionToClient { get; } // used to check if heartbeat is needed

        // Timeout management
        DateTime LastActionFromClient { get; }
        int TimeoutCount { get; }

        void ResetTimeout();
        void SetTimeout();
    }
}
