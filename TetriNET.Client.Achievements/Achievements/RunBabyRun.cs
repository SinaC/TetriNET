namespace TetriNET.Client.Achievements.Achievements
{
    internal class RunBabyRun : Achievement
    {
        public RunBabyRun()
        {
            Id = 22;
            Points = 20;
            Title = "Run baby run";
            Description = "Win a multiplayer game with more than 300 moves/min";
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
