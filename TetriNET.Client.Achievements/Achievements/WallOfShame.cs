using System.Collections.Generic;
using System.Linq;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class WallOfShame : Achievement
    {
        public WallOfShame()
        {
            Id = 39;
            Points = 40;
            Title = "Wall of Shame";
            Description = "Lose a multiplayer game while having a Nuke, a Gravity and a Switch in inventory";
            BronzeLevel = 1;
            SilverLevel = 3;
            GoldLevel = 5;
        }

        public override void OnGameLost(double playTime, int moveCount, int lineCount, int playerCount, int playerLeft, IReadOnlyCollection<Specials> inventory)
        {
            if (inventory != null && inventory.Any(x => x == Specials.NukeField) && inventory.Any(x => x == Specials.SwitchFields) && inventory.Any(x => x == Specials.BlockGravity))
                Achieve();
        }
    }
}
