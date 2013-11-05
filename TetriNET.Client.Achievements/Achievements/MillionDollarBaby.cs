using System.Collections.Generic;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class MillionDollarBaby : Achievement
    {
        public MillionDollarBaby()
        {
            Id = 36;
            Points = 50;
            Title = "Million Dollar Baby";
            Description = "Get 1,000,000 points in one game";
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, Interfaces.IBoard board, List<Common.DataContracts.Pieces> collapsedPieces)
        {
            if (score >= 1000000)
                Achieve();
        }
    }
}
