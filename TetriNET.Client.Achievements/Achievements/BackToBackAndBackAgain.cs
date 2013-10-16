using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class BackToBackAndBackAgain : Achievement
    {
        private int _count;

        public BackToBackAndBackAgain()
        {
            Title = "Back to Back and Back Again";
            Description = "Do 3 Tetrises without clearing any lines in between";
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            if (lineCompleted == 4)
            {
                _count++;
                if (_count == 3)
                    Achieve();
            }
            else if (lineCompleted > 0)
                _count = 0;
        }
    }
}
