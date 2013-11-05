using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Millenium : Achievement
    {
        private int _lineCount;

        public Millenium()
        {
            Id = 18;
            Points = 50;
            Title = "Millenium";
            Description = "Clear 1000 lines in one game";
        }

        public override void Reset()
        {
            _lineCount = 0;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IBoard board, List<Pieces> collapsedPieces)
        {
            _lineCount += lineCompleted;
            if (_lineCount >= 1000)
                Achieve();
        }
    }
}
