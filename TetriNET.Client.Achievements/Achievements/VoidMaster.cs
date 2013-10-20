using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Helpers;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class VoidMaster : Achievement
    {
        public VoidMaster()
        {
            Id = 29;
            Points = 50;
            Title = "Void Master";
            Description = "Clear whole board by clearing lines";
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            if (board.Cells.All(x => x == CellHelper.EmptyCell))
                Achieve();
        }
    }
}
