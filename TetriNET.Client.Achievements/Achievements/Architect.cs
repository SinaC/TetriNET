using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Architect : ConsecutiveLineClearedBase
    {
        public Architect()
        {
            Id = 1;
            Points = 40;
            Title = "Architect";
            Description = "Do 2 Tetrises in 2 drops";
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        protected override int LineCount => 4;

        protected override int CountToAchieve => 2;
    }
}
