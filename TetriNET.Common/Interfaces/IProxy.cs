using System;
using TetriNET.Common.Contracts;

namespace TetriNET.Common.Interfaces
{
    public delegate void ProxyConnectionLostHandler();

    public interface IProxy : ITetriNET
    {
        event ProxyConnectionLostHandler OnConnectionLost;

        DateTime LastActionToServer { get; } // used to check if heartbeat is needed
    }
}
