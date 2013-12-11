using System.Collections.Generic;
using TetriNET.Common.Contracts;

namespace TetriNET.Server.Interfaces
{
    public interface ISpectatorManager
    {
        int Add(ISpectator spectator);
        bool Remove(ISpectator spectator);
        void Clear();

        int MaxSpectators { get; }
        int SpectatorCount { get; }
        object LockObject { get; }

        IEnumerable<ISpectator> Spectators { get; }

        int GetId(ISpectator spectator);

        ISpectator this[string name] { get; }
        ISpectator this[int index] { get; }
        ISpectator this[ITetriNETCallback callback] { get; } // Callback property from ISpectator should only be used here
    }
}
