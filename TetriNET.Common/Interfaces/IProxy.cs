using System;
using System.Collections.Generic;
using TetriNET.Common.Contracts;

namespace TetriNET.Common.Interfaces
{
    public delegate void ProxyConnectionLostHandler();

    public interface IProxy : ITetriNET
    {
        DateTime LastActionToServer { get; } // used to check if heartbeat is needed

        event ProxyConnectionLostHandler OnConnectionLost;

        bool Disconnect();
    }
}
