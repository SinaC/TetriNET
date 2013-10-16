using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class NeedForSpeed : Achievement
    {
        private int _count;

        public NeedForSpeed()
        {
            Title = "Need for speed";
            Description = "Clear 50 lines on level 100";
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            if (level >= 100)
                _count += lineCompleted;
            if (_count >= 50)
                Achieve();
        }
    }
}
