using System.Collections.Generic;
using System.Linq;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class RainbowWarrior : Achievement
    {
        public RainbowWarrior()
        {
            Id = 35;
            Points = 15;
            Title = "Rainbow Warrior";
            Description = "Clear a line with 7 different piece's colors";
            BronzeLevel = 1;
            SilverLevel = 50;
            GoldLevel = 100;
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, Interfaces.IBoard board, List<Common.DataContracts.Pieces> collapsedPieces)
        {
            if (collapsedPieces != null && collapsedPieces.Distinct().Count() == 7)
                Achieve();
        }
    }
}
