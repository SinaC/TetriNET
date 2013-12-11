using System;
using TetriNET.Common.Contracts;

namespace TetriNET.Server.Interfaces
{
    public interface IEntity : ITetriNETCallback
    {
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
