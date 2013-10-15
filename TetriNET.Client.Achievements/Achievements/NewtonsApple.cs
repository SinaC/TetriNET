using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    public class NewtonsApple : Achievement
    {
        private int _count;

        public NewtonsApple()
        {
            Title = "Newton's Apple";
            Description = "Use 3 gravity in a game";
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
                if (special == Specials.BlockGravity)
                    _count++;
                if (_count == 3)
                    Achieve();
            }
        }
    }
}
