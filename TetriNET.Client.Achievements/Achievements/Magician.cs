using TetriNET.Client.Achievements.Achievements.Base;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Magician : SpecialCountBase
    {
        public Magician()
        {
            Id = 16;
            Points = 20;
            Title = "Magician";
            Description = "Use 3 Switch in a game";
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override Specials Special
        {
            get { return Specials.SwitchFields; }
        }

        public override int CountToAchieve
        {
            get { return 3; }
        }
    }
}
