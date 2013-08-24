using System;
using TetriNET.Common.Contracts;

namespace TetriNET.Client.Proxy
{
    public delegate void ConnectionLostHandler();

    public interface IProxy : ITetriNET
    {
        event ConnectionLostHandler OnConnectionLost;

        DateTime LastServerAction { get; }
        int TimeoutCount { get; }

        void ResetTimeout();
        void SetTimeout();
    }
}
