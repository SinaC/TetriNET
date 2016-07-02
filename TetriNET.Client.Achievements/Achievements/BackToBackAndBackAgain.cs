using TetriNET.Client.Achievements.Achievements.Base;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class BackToBackAndBackAgain : NoLineBetweenLineClearedBase
    {
        public BackToBackAndBackAgain()
        {
            Id = 3;
            Points = 40;
            Title = "Back to Back and Back Again";
            Description = "Do 3 Tetrises without clearing any lines in between";
            BronzeLevel = 1;
            SilverLevel = 3;
            GoldLevel = 5;
        }

        protected override int LineCount => 4;

        protected override int CountToAchieve => 3;
    }
}
