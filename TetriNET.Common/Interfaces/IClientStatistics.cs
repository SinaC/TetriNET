using System.Collections.Generic;
using TetriNET.Common.GameDatas;

namespace TetriNET.Common.Interfaces
{
    public interface IClientStatistics
    {
        Dictionary<Tetriminos, int> TetriminoCount { get; }
        Dictionary<Specials, int> SpecialCount { get; }
        Dictionary<Specials, int> SpecialUsed { get; }
        Dictionary<Specials, int> SpecialDiscarded { get; }

        int EndOfTetriminoQueueReached { get; }
        int NextTetriminoNotYetReceived { get; }
    }
}
