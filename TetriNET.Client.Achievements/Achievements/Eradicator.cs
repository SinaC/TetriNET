using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Eradicator: LineClearedBase
    {
        public Eradicator()
        {
            Id = 11;
            Points = 50;
            Title = "Eradicator";
            Description = "Clear a total of 100,000 lines";
            OnlyOnce = true;
            BronzeLevel = 1;
            SilverLevel = 3;
            GoldLevel = 5;
        }

        public override int CountToAchieve => 100000;
    }
}
