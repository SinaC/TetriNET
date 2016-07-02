using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Century : LineClearedByGameBase
    {
        public Century()
        {
            Id = 6;
            Points = 20;
            Title = "Century";
            Description = "Clear 100 lines in one game";
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override int CountToAchieve => 100;
    }
}
