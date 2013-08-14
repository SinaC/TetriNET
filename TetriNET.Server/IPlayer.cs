using System;
using TetriNET.Common;

namespace TetriNET.Server
{
    public delegate void ConnectionLostHandler(IPlayer player);

    public interface IPlayer : ITetriNETCallback
    {
        event ConnectionLostHandler OnConnectionLost;

        string Name { get; }
        int TetriminoIndex { get; set; }
        DateTime LastAction { get; set; }
        ITetriNETCallback Callback { get; } // Should never be used by anything else then IPlayerManager and IPlayer
    }
}
