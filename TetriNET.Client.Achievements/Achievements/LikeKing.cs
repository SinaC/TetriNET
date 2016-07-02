using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class LikeKing : GameWinCountBase
    {
        public LikeKing()
        {
            Id = 37;
            Points = 30;
            Title = "Like a King";
            Description = "Win 100 games";
            OnlyOnce = true;
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override int CountToAchieve => 100;
    }
}
