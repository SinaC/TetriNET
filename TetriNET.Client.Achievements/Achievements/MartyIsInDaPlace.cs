using TetriNET.Client.Achievements.Achievements.Base;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class MartyIsInDaPlace : SpecialCountBase
    {
        public MartyIsInDaPlace()
        {
            Id = 17;
            Points = 20;
            Title = "Marty is in da place";
            Description = "Use 5 Zebra in a game";
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override Specials Special => Specials.ZebraField;

        public override int CountToAchieve => 5;
    }
}
