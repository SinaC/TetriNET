using System;
using TetriNET.Common;

namespace TetriNET.Server
{
    public delegate void PlayerDisconnectedHandler(IPlayer player);

    public interface IPlayer : ITetriNETCallback
    {
        event PlayerDisconnectedHandler OnDisconnected;

        string Name { get; }
        int TetriminoIndex { get; set; }
        DateTime LastAction { get; set; }
        ITetriNETCallback Callback { get; } // Should never be used by anything else then IPlayerManager and IPlayer
    }
}
