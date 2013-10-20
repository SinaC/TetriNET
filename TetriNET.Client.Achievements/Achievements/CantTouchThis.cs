namespace TetriNET.Client.Achievements.Achievements
{
    internal class CantTouchThis : Achievement
    {
        public CantTouchThis()
        {
            Id = 5;
            Points = 100;
            Title = "Can't Touch This";
            Description = "Earn all achievements[NOT YET AVAILABLE]";
            OnlyOnce = true;
        }

        // TODO: new trigger, OnAchievementEarned
    }
}
