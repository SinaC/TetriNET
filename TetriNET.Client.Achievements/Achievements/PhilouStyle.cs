﻿using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class PhilouStyle : Achievement
    {
        private int _roundCount;

        public PhilouStyle()
        {
            Points = 50;
            Id = 31;
            Title = "Philou's Style";
            Description = "In a multiplayer game, let the first 3 pieces drop without any moves, then win the game";
            BronzeLevel = 1;
            SilverLevel = 3;
            GoldLevel = 5;
        }

        public override void Reset()
        {
            _roundCount = 0;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IReadOnlyBoard board, IReadOnlyCollection<Pieces> collapsedPieces)
        {
            _roundCount++;
            if (_roundCount <= 5 && moveCount > 0)
                IsFailed = true;
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            if (_roundCount >= 5)
                Achieve();
        }
    }
}
