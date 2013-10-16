using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Helpers;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class VoidMaster : Achievement
    {
        public VoidMaster()
        {
            Title = "Void Master";
            Description = "Clear whole board";
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            if (board.Cells.All(x => x == CellHelper.EmptyCell))
                Achieve();
        }
    }
}
