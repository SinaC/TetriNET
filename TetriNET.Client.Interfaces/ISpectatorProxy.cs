using System;
using TetriNET.Common.Contracts;

namespace TetriNET.Client.Interfaces
{
    public delegate void ProxySpectatorConnectionLostEventHandler();

    public interface ISpectatorProxy : ITetriNETSpectator
    {
        DateTime LastActionToServer { get; } // used to check if heartbeat is needed

        event ProxySpectatorConnectionLostEventHandler ConnectionLost;

        bool Disconnect();
    }
}
