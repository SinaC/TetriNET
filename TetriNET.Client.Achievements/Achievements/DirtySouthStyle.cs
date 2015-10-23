using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class DirtySouthStyle : ConsecutiveLineClearedBase
    {
        public DirtySouthStyle()
        {
            Id = 7;
            Points = 60;
            Title = "Dirty South Style";
            Description = "Do 3 Tetrises in 3 drops";
            BronzeLevel = 1;
            SilverLevel = 3;
            GoldLevel = 5;
        }

        protected override int LineCount
        {
            get { return 4; }
        }

        protected override int CountToAchieve
        {
            get { return 3; }
        }
    }
}
