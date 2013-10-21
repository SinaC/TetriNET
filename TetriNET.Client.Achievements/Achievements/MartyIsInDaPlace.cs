using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class MartyIsInDaPlace : Achievement
    {
        private int _count;

        public MartyIsInDaPlace()
        {
            Id = 17;
            Points = 20;
            Title = "Marty is in da place";
            Description = "Use 5 Zebra in a game";
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (special == Specials.ZebraField)
                _count++;
            if (_count == 5)
                Achieve();
        }
    }
}
