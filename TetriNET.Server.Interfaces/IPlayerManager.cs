using System.Collections.Generic;
using TetriNET.Common.Contracts;

namespace TetriNET.Server.Interfaces
{
    public interface IPlayerManager
    {
        int Add(IPlayer player);
        bool Remove(IPlayer player);
        void Clear();

        int MaxPlayers { get; }
        int PlayerCount { get; }
        object LockObject { get; }

        IEnumerable<IPlayer> Players { get; }

        int GetId(IPlayer player);

        IPlayer ServerMaster { get; }

        IPlayer this[string name] { get; }
        IPlayer this[int index] { get; }
        IPlayer this[ITetriNETCallback callback] { get; } // Callback property from IPlayer should only be used here
    }
}
