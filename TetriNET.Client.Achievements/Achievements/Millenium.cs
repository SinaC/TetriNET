using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Millenium : LineClearedByGameBase
    {
        public Millenium()
        {
            Id = 18;
            Points = 50;
            Title = "Millenium";
            Description = "Clear 1000 lines in one game";
            BronzeLevel = 1;
            SilverLevel = 3;
            GoldLevel = 5;
        }

        public override int CountToAchieve => 1000;
    }
}
