using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.Client.Achievements.Achievements
{
    public class JustInTime : Achievement
    {
        public JustInTime()
        {
            Title = "Just in time...";
            Description = "'Nuke field' 1 line before death";
        }

        public override void OnUseSpecial(int playerId, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (playerId == targetId && special == Specials.NukeField)
            {
                int spawnX = targetBoard.PieceSpawnX;
                int topY = targetBoard.Height - 2;
                if (targetBoard[spawnX, topY] != CellHelper.EmptyCell)
                    Achieve();
            }
        }
    }
}
