using System;
using TetriNET.Common.Contracts;

namespace TetriNET.Server.Player
{
    public delegate void ConnectionLostHandler(IPlayer player);

    public enum PlayerStates
    {
        Registered,
        Playing,
        GameLost,
    };

    public interface IPlayer : ITetriNETCallback
    {
        event ConnectionLostHandler OnConnectionLost;

        string Name { get; }
        int TetriminoIndex { get; set; }
        byte[] Grid { get; set; }
        //
        ITetriNETCallback Callback { get; } // Should never be used by anything else then IPlayerManager and IPlayer
        //
        PlayerStates State { get; set; }
        DateTime LossTime { get; set; }
        // Timeout management
        DateTime LastAction { get; }
        int TimeoutCount { get; }

        //
        void ResetTimeout();
        void SetTimeout();
    }
}
