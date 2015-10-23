using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class SomeLikeItHot : TetriminoCountBase
    {
        public SomeLikeItHot()
        {
            Id = 25;
            Points = 50;
            Title = "Some like it hot";
            Description = "Drop a total of 100,000 Tetriminos";
            OnlyOnce = true;
            BronzeLevel = 1;
            SilverLevel = 2;
            GoldLevel = 10;
        }

        public override int CountToAchieve
        {
            get { return 100000; }
        }
    }
}
