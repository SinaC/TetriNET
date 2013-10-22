using System.Collections.Generic;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Looooser : Achievement
    {
        private int _count;

        public Looooser()
        {
            Id = 13;
            Points = 10;
            Title = "Looooser";
            Description = "Be the first to lose, 5 times in a row in a multiplayer game (min. 4 players)";
            ResetOnGameStarted = false;
        }

        public override void Reset()
        {
            //if (IsAchieved && FirstTimeAchieved < new DateTime(2013, 10, 22, 13, 32, 00)) // reset holder of previous version
            //{
            //    AchieveCount = 0;
            //    IsAchieved = false;
            //}
            _count = 0;
            base.Reset();
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            _count = 0;
        }

        public override void OnGameLost(double playTime, int moveCount, int lineCount, int playerCount, int playerLeft, List<Specials> inventory)
        {
            if (playerLeft + 1 == playerCount && playerCount >= 4)
            {
                _count++;
                if (_count == 5)
                    Achieve();
            }
            else
                _count = 0;
        }
    }
}
