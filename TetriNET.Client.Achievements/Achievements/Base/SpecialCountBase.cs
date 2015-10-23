using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements.Base
{
    internal abstract class SpecialCountBase : Achievement
    {
        private int _count;

        public abstract Specials Special { get; }
        public abstract int CountToAchieve { get; }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IReadOnlyBoard playerBoard, int targetId, string targetTeam, IReadOnlyBoard targetBoard, Specials special)
        {
            if (special == Special)
            {
                _count++;
                if (_count == CountToAchieve)
                    Achieve();
            }
        }
    }
}
