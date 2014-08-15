using System;
using TetriNET.Common.Contracts;

namespace TetriNET.Client.Interfaces
{
    public delegate void ProxyConnectionLostEventHandler();

    public interface IProxy : ITetriNET
    {
        DateTime LastActionToServer { get; } // used to check if heartbeat is needed

        event ProxyConnectionLostEventHandler ConnectionLost;

        bool Disconnect();
    }
}
