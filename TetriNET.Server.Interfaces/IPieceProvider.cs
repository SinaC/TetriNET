using System;
using System.Collections.Generic;
using TetriNET.Common.DataContracts;

namespace TetriNET.Server.Interfaces
{
    public interface IPieceProvider
    {
        void Reset();
        Func<IEnumerable<PieceOccurancy>> Occurancies { get; set; }
        Pieces this[int index] { get; }
    }
}
