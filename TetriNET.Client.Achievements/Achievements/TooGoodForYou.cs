using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    public class TooGoodForYou : Achievement
    {
        private int _specialCount;

        public TooGoodForYou()
        {
            Title = "Too good for you";
            Description = "Win a game without using any specials";
        }

        public override void Reset()
        {
            _specialCount = 0;
            base.Reset();
        }

        public override void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (playerId == sourceId)
                _specialCount++;
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            if (_specialCount == 0)
                Achieve();
        }
    }
}
