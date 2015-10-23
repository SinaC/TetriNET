using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Eliminator : LineClearedBase
    {
        public Eliminator()
        {
            Id = 10;
            Points = 30;
            Title = "Eliminator";
            Description = "Clear a total of 10,000 lines";
            OnlyOnce = true;
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override int CountToAchieve
        {
            get { return 10000; }
        }
    }
}
