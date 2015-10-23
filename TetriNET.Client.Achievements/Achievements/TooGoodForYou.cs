using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class TooGoodForYou : Achievement
    {
        private int _specialCount;

        public TooGoodForYou()
        {
            Id = 28;
            Points = 40;
            Title = "Too good for you";
            Description = "Win a game without using any specials";
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override void Reset()
        {
            _specialCount = 0;
            base.Reset();
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IReadOnlyBoard playerBoard, int targetId, string targetTeam, IReadOnlyBoard targetBoard, Specials special)
        {
            _specialCount++;
            IsFailed = true;
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            if (_specialCount == 0)
                Achieve();
        }
    }
}
