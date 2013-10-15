namespace TetriNET.Client.Achievements.Achievements
{
    public class HitchhikersGuide : Achievement
    {
        public HitchhikersGuide()
        {
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
