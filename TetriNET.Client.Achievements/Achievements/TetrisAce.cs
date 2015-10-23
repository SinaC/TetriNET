﻿using System;
using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class TetrisAce : Achievement
    {
        public TetrisAce()
        {
            Id = 26;
            Points = 30;
            Title = "Tetris Ace";
            Description = "Perform 150 Tetrises";
            OnlyOnce = true;
            BronzeLevel = 1;
            SilverLevel = 3;
            GoldLevel = 5;
        }

        public override string Progress
        {
            get { return String.Format("{0} / {1} ({2:0.0}%)", ExtraData, 150, 100.0 * (ExtraData / 150.0)); }
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IReadOnlyBoard board, IReadOnlyCollection<Pieces> collapsedPieces)
        {
            if (lineCompleted == 4)
            {
                ExtraData++;
                if (ExtraData >= 150)
                    Achieve();
            }
        }
    }
}
