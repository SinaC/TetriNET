namespace TetriNET.Client.Achievements.Achievements
{
    internal class HitchhikersGuide : Achievement
    {
        public HitchhikersGuide()
        {
            Points = 15;
            Title = "Hitchhiker's Guide";
            Description = "Win with exactly 42 lines";
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            if (lineCount == 42)
                Achieve();
        }
    }
}
