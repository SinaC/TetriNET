using System;
using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Eliminator : Achievement
    {
        public Eliminator()
        {
            Id = 10;
            Points = 30;
            Title = "Eliminator";
            Description = "Clear a total of 10,000 lines";
            OnlyOnce = true;
        }

        public override string Progress
        {
            get { return String.Format("{0:#,0} / {1:#,0} ({2:0.0}%)", ExtraData, 10000, 100.0 * (ExtraData / 10000.0)); }
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IBoard board, List<Pieces> collapsedPieces)
        {
            ExtraData += lineCompleted;
            if (ExtraData >= 10000)
                Achieve();
        }
    }
}
