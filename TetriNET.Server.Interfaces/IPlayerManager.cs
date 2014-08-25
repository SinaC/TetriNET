using System.Collections.Generic;
using TetriNET.Common.Contracts;

namespace TetriNET.Server.Interfaces
{
    public interface IPlayerManager
    {
        bool Add(IPlayer player);
        bool Remove(IPlayer player);
        void Clear();

        int MaxPlayers { get; }
        int PlayerCount { get; }
        object LockObject { get; }

        int FirstAvailableId { get; }

        List<IPlayer> Players { get; }

        IPlayer ServerMaster { get; }

        IPlayer this[string name] { get; }
        IPlayer this[int id] { get; }
        IPlayer this[ITetriNETCallback callback] { get; } // Callback property from IPlayer should only be used here
    }
}
