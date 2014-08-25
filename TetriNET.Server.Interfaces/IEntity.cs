using System;
using TetriNET.Common.Contracts;

namespace TetriNET.Server.Interfaces
{
    public delegate void ConnectionLostEventHandler(IEntity entity);

    public interface IEntity : ITetriNETCallback
    {
        event ConnectionLostEventHandler ConnectionLost;

        int Id { get; }
        string Name { get; }

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
