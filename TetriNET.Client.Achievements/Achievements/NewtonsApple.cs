using TetriNET.Client.Achievements.Achievements.Base;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class NewtonsApple : SpecialCountBase
    {
        public NewtonsApple()
        {
            Id = 20;
            Points = 20;
            Title = "Newton's Apple";
            Description = "Use 5 Gravity in a game";
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override Specials Special => Specials.BlockGravity;

        public override int CountToAchieve => 5;
    }
}
