using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class DropItLikeItsHot : TetriminoCountBase
    {
        public DropItLikeItsHot()
        {
            Id = 9;
            Points = 30;
            Title = "Drop it like it's hot";
            Description = "Drop a total of 10,000 Tetriminos";
            OnlyOnce = true;
            BronzeLevel = 1;
            SilverLevel = 3;
            GoldLevel = 5;
        }

        public override int CountToAchieve
        {
            get { return 10000; }
        }
    }
}
