using System;
using System.Collections.Generic;
using TetriNET.Common;

namespace TetriNET.Server
{
    public interface IPlayerManager
    {
        IPlayer Add(string name, ITetriNETCallback callback);
        bool Remove(IPlayer player);

        int PlayerCount { get; }

        IEnumerable<IPlayer> Players { get; }

        IPlayer this[ITetriNETCallback callback] { get; }
        IPlayer this[string name] { get; }
        IPlayer this[int index] { get; }
    }
}