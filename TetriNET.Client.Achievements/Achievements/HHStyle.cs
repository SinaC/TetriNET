using System.Collections.Generic;
using System.Linq;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class HHStyle : Achievement
    {
        public HHStyle()
        {
            Id = 32;
            Points = 10;
            Title = "HH's style";
            Description = "Lose a multiplayer game while having a Nuke, Gravity or Switch in inventory";
        }

        public override void OnGameLost(double playTime, int moveCount, int lineCount, int playerCount, int playerLeft, List<Specials> inventory)
        {
            if (inventory != null && inventory.Any(x => x == Specials.SwitchFields || x == Specials.BlockGravity || x == Specials.NukeField))
                Achieve();
        }
    }
}
