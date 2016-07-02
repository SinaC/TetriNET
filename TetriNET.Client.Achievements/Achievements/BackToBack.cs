using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class BackToBack : NoLineBetweenLineClearedBase
    {
        public BackToBack()
        {
            Id = 2;
            Points = 20;
            Title = "Back to Back";
            Description = "Do 2 Tetrises without clearing any lines in between";
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        protected override int LineCount => 4;

        protected override int CountToAchieve => 2;
    }
}
