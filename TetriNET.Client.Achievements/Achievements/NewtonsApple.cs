using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class NewtonsApple : Achievement
    {
        private int _count;

        public NewtonsApple()
        {
            Id = 20;
            Points = 20;
            Title = "Newton's Apple";
            Description = "Use 5 Gravity in a game";
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (special == Specials.BlockGravity)
                _count++;
            if (_count == 3)
                Achieve();
        }
    }
}
