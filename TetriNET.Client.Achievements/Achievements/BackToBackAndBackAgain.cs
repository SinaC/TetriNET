using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class BackToBackAndBackAgain : Achievement
    {
        private int _count;

        public BackToBackAndBackAgain()
        {
            Id = 3;
            Points = 40;
            Title = "Back to Back and Back Again";
            Description = "Do 3 Tetrises without clearing any lines in between";
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IBoard board, List<Pieces> collapsedPieces)
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
