using System;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class LikeEmperor : Achievement
    {
        public LikeEmperor()
        {
            Id = 38;
            Points = 60;
            Title = "Like an Emperor";
            Description = "Win 1000 games";
            OnlyOnce = true;
        }

        public override string Progress
        {
            get { return String.Format("{0:#,0} / {1:#,0} ({2:0.0}%)", ExtraData, 1000, 100.0 * (ExtraData / 1000.0)); }
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            ExtraData++;
            if (ExtraData >= 1000)
                Achieve();
        }
    }
}
