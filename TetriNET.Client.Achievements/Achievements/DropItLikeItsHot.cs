using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class DropItLikeItsHot : Achievement
    {
        public DropItLikeItsHot()
        {
            Title = "Drop it like it's hot";
            Description = "Drop a total of 10,000 Tetriminos";
            OnlyOnce = true;
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            ExtraData++;
            if (ExtraData >= 10000)
                Achieve();
        }
    }
}
