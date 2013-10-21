namespace TetriNET.Client.Achievements.Achievements
{
    internal class HHStyle : Achievement
    {
        private int _count;

        public HHStyle()
        {
            Id = 13;
            Points = 10;
            Title = "HH’s style";
            Description = "Be the first to lose, 5 times in a row in a multiplayer game (min. 4 players)";
            ResetOnGameStarted = false;
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            _count = 0;
        }

        public override void OnGameLost(double playTime, int moveCount, int lineCount, int playerCount, int playerLeft)
        {
            if (playerLeft + 1 == playerCount && playerCount >= 4)
            {
                _count++;
                if (_count == 5)
                    Achieve();
            }
        }
    }
}
