﻿using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Century : Achievement
    {
        private int _lineCount;

        public Century()
        {
            Id = 6;
            Points = 20;
            Title = "Century";
            Description = "Clear 100 lines in one game";
        }

        public override void Reset()
        {
            _lineCount = 0;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            _lineCount += lineCompleted;
            if (_lineCount >= 100)
                Achieve();
        }
    }
}
