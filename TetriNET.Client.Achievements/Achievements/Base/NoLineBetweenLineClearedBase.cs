using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements.Base
{
    internal abstract class NoLineBetweenLineClearedBase : Achievement
    {
        private int _count;

        protected abstract int LineCount { get; } // 1, 2, 3 or 4(for Tetris)
        protected abstract int CountToAchieve { get; } // number of consecutive LineCount lines to achieve

        protected NoLineBetweenLineClearedBase()
        {
            _count = 0;
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IReadOnlyBoard board, IReadOnlyCollection<Pieces> collapsedPieces)
        {
            if (lineCompleted == LineCount)
            {
                _count++;
                if (_count == CountToAchieve)
                    Achieve();
            }
            else if (lineCompleted > 0)
                _count = 0;
        }
    }
}
