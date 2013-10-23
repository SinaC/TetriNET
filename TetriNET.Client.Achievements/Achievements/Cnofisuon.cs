using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Cnofisuon : Achievement
    {
        private int _count;

        public Cnofisuon()
        {
            Points = 20;
            Title = "Cnofisuon";
            Description = "Use 5 Confusions in a game";
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (special == Specials.Confusion)
                _count++;
            if (_count == 5)
                Achieve();
        }
    }
}
