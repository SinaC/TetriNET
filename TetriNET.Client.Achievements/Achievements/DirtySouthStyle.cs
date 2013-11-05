using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class DirtySouthStyle : Achievement
    {
        private int _count;

        public DirtySouthStyle()
        {
            Id = 7;
            Points = 60;
            Title = "Dirty South Style";
            Description = "Do 3 Tetrises in 3 drops";
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IBoard board, List<Pieces> collapsedPieces)
        {
            if (lineCompleted == 4)
            {
                _count++;
                if (_count == 3)
                    Achieve();
            }
            else
                _count = 0;
       }
    }
}
