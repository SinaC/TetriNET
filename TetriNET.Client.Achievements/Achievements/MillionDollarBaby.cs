using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

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
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IReadOnlyBoard board, IReadOnlyCollection<Pieces> collapsedPieces)
        {
            if (score >= 1000000)
                Achieve();
        }
    }
}
