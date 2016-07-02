using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements.Base
{
    internal abstract class LineClearedByGameBase : AchievementBase
    {
        private int _lineCount;

        public abstract int CountToAchieve { get; }

        public override void Reset()
        {
            _lineCount = 0;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IReadOnlyBoard board, IReadOnlyCollection<Pieces> collapsedPieces)
        {
            _lineCount += lineCompleted;
            if (_lineCount >= CountToAchieve)
                Achieve();
        }
    }
}
