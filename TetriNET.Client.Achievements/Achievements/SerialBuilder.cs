namespace TetriNET.Client.Achievements.Achievements
{
    public class SerialBuilder : Achievement
    {
        public SerialBuilder()
        {
            Title = "Serial builder";
            Description = "> 15 lines/min and at least 15 lines";
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            Check(playTime, lineCount);
        }

        public override void OnGameLost(double playTime, int moveCount, int lineCount, int playerCount)
        {
            Check(playTime, lineCount);
        }

        private void Check(double playTime, int lineCount)
        {
            double speed = lineCount/(playTime/60);
            if (speed > 15 && lineCount > 15)
                Achieve();
        }
    }
}
