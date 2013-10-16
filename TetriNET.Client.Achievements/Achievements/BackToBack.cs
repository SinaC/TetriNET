using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class BackToBack : Achievement
    {
        private bool _active;

        public BackToBack()
        {
            Title = "Back to Back";
            Description = "Do 2 Tetrises without clearing any lines in between";
        }

        public override void Reset()
        {
            _active = false;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            if (lineCompleted == 4 && _active)
                Achieve();
            else if (lineCompleted == 4)
                _active = true;
            else if (lineCompleted > 0)
                _active = false;
        }
    }
}
