﻿using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class FearMyBrain : Achievement
    {
        private int _count;

        public FearMyBrain()
        {
            Points = 50;
            Title = "Fear my brain !";
            Description = "Clear 10 lines in 10 drops";
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            if (lineCompleted == 0)
                _count = 0;
            else
                _count += lineCompleted;
            if (_count >= 10)
                Achieve();
        }
    }
}
