using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class NuclearLaunchDetected : Achievement
    {
        public NuclearLaunchDetected()
        {
            Id = 21;
            Points = 30;
            Title = "Nuclear launch detected";
            Description = "Explode 3 (or more) Bombs in one attack";
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (special == Specials.BlockBomb)
            {
                int targetBomb = targetBoard.Cells.Count(x => CellHelper.GetSpecial(x) == Specials.BlockBomb);
                if (targetBomb >= 3)
                    Achieve();
            }
        }
    }
}
