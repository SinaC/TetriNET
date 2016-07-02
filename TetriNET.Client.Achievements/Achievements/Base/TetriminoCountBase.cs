using System;
using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements.Base
{
    internal abstract class TetriminoCountBase : AchievementBase
    {
        public abstract int CountToAchieve { get; }

        public override string Progress => String.Format("{0:#,0} / {1:#,0} ({2:0.0}%)", ExtraData, CountToAchieve, 100.0 * (ExtraData / (double)CountToAchieve));

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IReadOnlyBoard board, IReadOnlyCollection<Pieces> collapsedPieces)
        {
            ExtraData++;
            if (ExtraData >= CountToAchieve)
                Achieve();
        }
    }
}
