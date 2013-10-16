﻿using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class FearMyBrain : Achievement
    {
        private int _count;

        public FearMyBrain()
        {
            Title = "Fear my brain !";
            Description = "Clear 10 lines non-stop";
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
