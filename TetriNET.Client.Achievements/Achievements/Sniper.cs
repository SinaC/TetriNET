namespace TetriNET.Client.Achievements.Achievements
{
    public class Sniper : Achievement
    {
        public Sniper()
        {
            Title = "Sniper";
            Description = "Win a game in less than 1 minute (min 3 players)";
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            // play time < 60 sec and player count >= 3
            if (playTime < 60 && playerCount >= 3)
                Achieve();
        }
    }
}
