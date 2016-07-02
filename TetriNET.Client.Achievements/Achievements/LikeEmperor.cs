using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class LikeEmperor : GameWinCountBase
    {
        public LikeEmperor()
        {
            Id = 38;
            Points = 60;
            Title = "Like an Emperor";
            Description = "Win 1000 games";
            OnlyOnce = true;
            BronzeLevel = 1;
            SilverLevel = 2;
            GoldLevel = 3;
        }

        public override int CountToAchieve => 1000;
    }
}
