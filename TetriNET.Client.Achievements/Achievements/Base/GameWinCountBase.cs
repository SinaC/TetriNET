using System;

namespace TetriNET.Client.Achievements.Achievements.Base
{
    internal abstract class GameWinCountBase : Achievement
    {
        public abstract int CountToAchieve { get; }

        public override string Progress
        {
            get { return String.Format("{0:#,0} / {1:#,0} ({2:0.0}%)", ExtraData, CountToAchieve, 100.0 * (ExtraData / (double)CountToAchieve)); }
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            ExtraData++;
            if (ExtraData >= CountToAchieve)
                Achieve();
        }
    }
}
