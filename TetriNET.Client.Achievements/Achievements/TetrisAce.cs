using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class TetrisAce : Achievement
    {
        public TetrisAce()
        {
            Title = "Tetris Ace";
            Description = "Perform 150 Tetrises";
            OnlyOnce = true;
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            if (lineCompleted == 4)
                ExtraData++;
            if (ExtraData >= 150)
                Achieve();
        }
    }
}
