﻿using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class NeedForSpeed : Achievement
    {
        private int _count;

        public NeedForSpeed()
        {
            Id = 19;
            Points = 50;
            Title = "Need for speed";
            Description = "Clear 50 lines on level 100";
            BronzeLevel = 1;
            SilverLevel = 3;
            GoldLevel = 5;
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IReadOnlyBoard board, IReadOnlyCollection<Pieces> collapsedPieces)
        {
            if (level >= 100)
            {
                _count += lineCompleted;
                if (_count >= 50)
                    Achieve();
            }
        }
    }
}
