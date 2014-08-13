namespace TetriNET.Client.Achievements.Achievements
{
    internal class HitchhikersGuide : Achievement
    {
        public HitchhikersGuide()
        {
            Id = 14;
            Points = 30;
            Title = "Hitchhiker's Guide";
            Description = "Win with exactly 42 lines";
            BronzeLevel = 1;
            SilverLevel = 3;
            GoldLevel = 5;
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            if (lineCount == 42)
                Achieve();
        }
    }
}
