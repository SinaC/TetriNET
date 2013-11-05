using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class BackToBack : Achievement
    {
        private bool _active;

        public BackToBack()
        {
            Id = 2;
            Points = 20;
            Title = "Back to Back";
            Description = "Do 2 Tetrises without clearing any lines in between";
        }

        public override void Reset()
        {
            _active = false;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IBoard board, List<Pieces> collapsedPieces)
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
