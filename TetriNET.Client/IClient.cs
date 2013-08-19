using System;
using TetriNET.Common.Contracts;

namespace TetriNET.Client
{
    public interface IClient
    {
        DateTime LastAction { get; set; }
        void OnDisconnectedFromServer(IWCFTetriNET proxy);
        void OnServerUnreachable(IWCFTetriNET proxy);
    }
}
