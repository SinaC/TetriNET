using TetriNET.Client.Achievements.Achievements.Base;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Cnofisuon : SpecialCountBase
    {
        public Cnofisuon()
        {
            Points = 20;
            Title = "Cnofisuon";
            Description = "Use 5 Confusions in a game";
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override Specials Special
        {
            get { return Specials.Confusion; }
        }
        public override int CountToAchieve
        {
            get { return 5; }
        }
    }
}
