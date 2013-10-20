using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Millenium : Achievement
    {
        private int _lineCount;

        public Millenium()
        {
            Id = 18;
            Points = 50;
            Title = "Millenium";
            Description = "Clear 1000 lines in one game";
        }

        public override void Reset()
        {
            _lineCount = 0;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            _lineCount += lineCompleted;
            if (_lineCount >= 1000)
                Achieve();
        }
    }
}
