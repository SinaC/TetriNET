using System.Collections.Generic;
using TetriNET.Common.DataContracts;

namespace TetriNET.Common.Interfaces
{
    public interface IClientStatistics
    {
        Dictionary<Pieces, int> PieceCount { get; }
        Dictionary<Specials, int> SpecialCount { get; }
        Dictionary<Specials, int> SpecialUsed { get; }
        Dictionary<Specials, int> SpecialDiscarded { get; }

        int EndOfPieceQueueReached { get; }
        int NextPieceNotYetReceived { get; }
    }
}
