﻿using System;
using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Eradicator: Achievement
    {
        public Eradicator()
        {
            Id = 11;
            Points = 50;
            Title = "Eradicator";
            Description = "Clear a total of 100,000 lines";
            OnlyOnce = true;
        }

        public override string Progress
        {
            get { return String.Format("{0:#,0} / {1:#,0} ({2:0.0}%)", ExtraData, 100000, 100.0 * (ExtraData / 100000.0)); }
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, IBoard board)
        {
            ExtraData += lineCompleted;
            if (ExtraData >= 100000)
                Achieve();
        }
    }
}
