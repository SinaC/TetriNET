using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class DoBarrelRoll : Achievement
    {
        private bool _gravity;
        private bool _leftGravity;

        public DoBarrelRoll()
        {
            Id = 8;
            Points = 15;
            Title = "Do a barrel roll";
            Description = "Use a Gravity and a Left Gravity in a game";
        }

        public override void Reset()
        {
            _gravity = false;
            _leftGravity = false;
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (special == Specials.BlockGravity)
                _gravity = true;
            if (special == Specials.LeftGravity)
                _leftGravity = true;
            if (_gravity && _leftGravity)
                Achieve();
        }
    }
}
