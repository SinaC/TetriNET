using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class JustInTime : Achievement
    {
        public JustInTime()
        {
            Id = 15;
            Points = 30;
            Title = "Just in time...";
            Description = "Nuke 1 line before death";
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (playerId == targetId && special == Specials.NukeField)
            {
                int spawnX = playerBoard.PieceSpawnX;
                int topY = playerBoard.Height - 2;
                if (playerBoard[spawnX, topY] != CellHelper.EmptyCell)
                    Achieve();
            }
        }
    }
}
