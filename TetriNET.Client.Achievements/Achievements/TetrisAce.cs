using System;
using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class TetrisAce : Achievement
    {
        public TetrisAce()
        {
            Points = 30;
            Title = "Tetris Ace";
            Description = "Perform 150 Tetrises";
            OnlyOnce = true;
        }

        public override string Progress
        {
            get { return String.Format("{0} / {1} ({2:0.0}%)", ExtraData, 150, 100.0 * (ExtraData / 150.0)); }
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            if (lineCompleted == 4)
                ExtraData++;
            if (ExtraData >= 150)
                Achieve();
        }
    }
}
