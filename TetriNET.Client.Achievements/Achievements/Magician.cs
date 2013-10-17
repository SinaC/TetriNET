using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Magician : Achievement
    {
        private int _count;

        public Magician()
        {
            Points = 20;
            Title = "Magician";
            Description = "Use 3 Switch in a game";
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (special == Specials.SwitchFields)
                _count++;
            if (_count == 3)
                Achieve();
        }
    }
}
