namespace TetriNET.Client.Achievements.Achievements
{
    internal class Sniper : Achievement
    {
        public Sniper()
        {
            Id = 24;
            Points = 40;
            Title = "Sniper";
            Description = "Win a multiplayer game in less than 1 minute";
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            // play time < 60 sec and player count >= 3
            if (playTime < 60 && playerCount >= 3)
                Achieve();
        }
    }
}
