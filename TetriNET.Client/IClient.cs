using System;
using TetriNET.Common;

namespace TetriNET.Client
{
    public interface IClient
    {
        DateTime LastAction { get; set; }
        void OnDisconnectedFromServer(ITetriNET proxy);
        void OnServerUnreachable(ITetriNET proxy);
    }
}
