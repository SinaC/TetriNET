using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Century : Achievement
    {
        private int _lineCount;

        public Century()
        {
            Id = 6;
            Points = 20;
            Title = "Century";
            Description = "Clear 100 lines in one game";
        }

        public override void Reset()
        {
            _lineCount = 0;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IBoard board, List<Pieces> collapsedPieces)
        {
            _lineCount += lineCompleted;
            if (_lineCount >= 100)
                Achieve();
        }
    }
}
