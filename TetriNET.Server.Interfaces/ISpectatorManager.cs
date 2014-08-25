using System.Collections.Generic;
using TetriNET.Common.Contracts;

namespace TetriNET.Server.Interfaces
{
    public interface ISpectatorManager
    {
        bool Add(ISpectator spectator);
        bool Remove(ISpectator spectator);
        void Clear();

        int MaxSpectators { get; }
        int SpectatorCount { get; }
        object LockObject { get; }

        int FirstAvailableId { get; }

        IEnumerable<ISpectator> Spectators { get; }

        ISpectator this[string name] { get; }
        ISpectator this[int id] { get; }
        ISpectator this[ITetriNETCallback callback] { get; } // Callback property from ISpectator should only be used here
    }
}
