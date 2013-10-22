using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.Client
{
    internal sealed class Statistics : IClientStatistics
    {
        public Dictionary<Pieces, int> PieceCount { get; set; }
        public Dictionary<Specials, int> SpecialCount { get; set; }
        public Dictionary<Specials, int> SpecialUsed { get; set; }
        public Dictionary<Specials, int> SpecialDiscarded { get; set; }

        public int MoveCount { get; set; }
        public int SingleCount { get; set; }
        public int DoubleCount { get; set; }
        public int TripleCount { get; set; }
        public int TetrisCount { get; set; }
        public int EndOfPieceQueueReached { get; set; }
        public int NextPieceNotYetReceived { get; set; }

        public int GameWon { get; set; }
        public int GameLost { get; set; }

        public Statistics()
        {
            PieceCount = new Dictionary<Pieces, int>();
            SpecialCount = new Dictionary<Specials, int>();
            SpecialUsed = new Dictionary<Specials, int>();
            SpecialDiscarded = new Dictionary<Specials, int>();

            foreach (Pieces piece in EnumHelper.GetPieces(available => available))
                PieceCount.Add(piece, 0);
            foreach (Specials special in EnumHelper.GetSpecials(available => available))
            {
                SpecialCount.Add(special, 0);
                SpecialUsed.Add(special, 0);
                SpecialDiscarded.Add(special, 0);
            }
        }

        public void Reset()
        {
            foreach (Pieces piece in EnumHelper.GetPieces(available => available))
                PieceCount[piece] = 0;
            foreach (Specials special in EnumHelper.GetSpecials(available => available))
            {
                SpecialCount[special] = 0;
                SpecialUsed[special] = 0;
                SpecialDiscarded[special] = 0;
            }
            MoveCount = 0;
            SingleCount = 0;
            DoubleCount = 0;
            TripleCount = 0;
            TetrisCount = 0;
            EndOfPieceQueueReached = 0;
            NextPieceNotYetReceived = 0;
            // No reset for GameWon and GameLost
        }
    }
}
