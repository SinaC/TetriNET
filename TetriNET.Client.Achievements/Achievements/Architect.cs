using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Architect : Achievement
    {
        private bool _active;

        public Architect()
        {
            Id = 1;
            Points = 40;
            Title = "Architect";
            Description = "Do 2 Tetrises in 2 drops";
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
            else
                _active = false;
       }
    }
}
