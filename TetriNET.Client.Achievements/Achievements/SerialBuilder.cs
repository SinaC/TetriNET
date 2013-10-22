using System.Collections.Generic;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class SerialBuilder : Achievement
    {
        public SerialBuilder()
        {
            Id = 23;
            Points = 30;
            Title = "Serial builder";
            Description = "Finish a game with more than 15 lines cleared/min";
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            Check(playTime, lineCount);
        }

        public override void OnGameLost(double playTime, int moveCount, int lineCount, int playerCount, int playerLeft, List<Specials> inventory)
        {
            Check(playTime, lineCount);
        }

        private void Check(double playTime, int lineCount)
        {
            double speed = lineCount/(playTime/60);
            if (speed > 15 && lineCount > 15)
                Achieve();
        }
    }
}
