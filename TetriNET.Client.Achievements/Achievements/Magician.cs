using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    public class Magician : Achievement
    {
        private int _count;

        public Magician()
        {
            Title = "Magician";
            Description = "Use 3 'switch fields' in a game";
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (playerId == sourceId)
            {
                if (special == Specials.SwitchFields)
                    _count++;
                if (_count == 3)
                    Achieve();
            }
        }
    }
}
