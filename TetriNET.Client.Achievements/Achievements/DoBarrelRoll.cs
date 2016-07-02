using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class DoBarrelRoll : AchievementBase
    {
        private bool _gravity;
        private bool _leftGravity;

        public DoBarrelRoll()
        {
            Id = 8;
            Points = 15;
            Title = "Do a barrel roll";
            Description = "Use a Gravity and a Left Gravity in a game";
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override void Reset()
        {
            _gravity = false;
            _leftGravity = false;
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IReadOnlyBoard playerBoard, int targetId, string targetTeam, IReadOnlyBoard targetBoard, Specials special)
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
