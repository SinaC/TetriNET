using System;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class LikeKing : Achievement
    {
        public LikeKing()
        {
            Id = 37;
            Points = 30;
            Title = "Like a King";
            Description = "Win 100 games";
            OnlyOnce = true;
        }

        public override string Progress
        {
            get { return String.Format("{0:#,0} / {1:#,0} ({2:0.0}%)", ExtraData, 100, ExtraData); }
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            ExtraData++;
            if (ExtraData >= 100)
                Achieve();
        }
    }
}
