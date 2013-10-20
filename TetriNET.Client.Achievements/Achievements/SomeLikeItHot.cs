using System;
using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class SomeLikeItHot : Achievement
    {
        public SomeLikeItHot()
        {
            Id = 25;
            Points = 50;
            Title = "Some like it hot";
            Description = "Drop a total of 100,000 Tetriminos";
            OnlyOnce = true;
        }

        public override string Progress
        {
            get { return String.Format("{0:#,0} / {1:#,0} ({2:0.0}%)", ExtraData, 100000, 100.0 * (ExtraData / 100000.0)); }
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            ExtraData++;
            if (ExtraData >= 100000)
                Achieve();
        }
    }
}
