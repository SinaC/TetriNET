namespace TetriNET.Client.Achievements.Achievements
{
    public class RunBabyRun : Achievement
    {
        public RunBabyRun()
        {
            Title = "Run baby run";
            Description = "> 300 mov/min and win the game(at least 3 players game)";
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            // 300 moves/min -> 5 moves/sec
            double speed = moveCount/playTime;
            if (playerCount >= 3 && speed > 5)
                Achieve();
        }
    }
}
