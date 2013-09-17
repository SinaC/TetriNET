using TetriNET.Common.GameDatas;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    public static class Mapper
    {
        public static char MapSpecialToChar(Specials special)
        {
            switch (special)
            {
                case Specials.AddLines:
                    return 'A';
                case Specials.ClearLines:
                    return 'C';
                case Specials.NukeField:
                    return 'N';
                case Specials.RandomBlocksClear:
                    return 'R';
                case Specials.SwitchFields:
                    return 'S';
                case Specials.ClearSpecialBlocks:
                    return 'B';
                case Specials.BlockGravity:
                    return 'G';
                case Specials.BlockQuake:
                    return 'Q';
                case Specials.BlockBomb:
                    return 'O';
                case Specials.ClearColumn:
                    return 'V';
                case Specials.Darkness:
                    return 'D';
                case Specials.Confusion:
                    return 'F';
                //case Specials.ZebraField: // will be available when Left Gravity is implemented
                //    return 'Z';
            }
            return '?';
        }

        public static string MapSpecialToString(Specials special)
        {
            switch (special)
            {
                case Specials.AddLines:
                    return "Add Line";
                case Specials.ClearLines:
                    return "Clear Line";
                case Specials.NukeField:
                    return "Nuke Field";
                case Specials.RandomBlocksClear:
                    return "Random Blocks Clear";
                case Specials.SwitchFields:
                    return "Switch Fields";
                case Specials.ClearSpecialBlocks:
                    return "Clear Special Blocks";
                case Specials.BlockGravity:
                    return "Block Gravity";
                case Specials.BlockQuake:
                    return "Block Quake";
                case Specials.BlockBomb:
                    return "Block Bomb";
                case Specials.ClearColumn:
                    return "Clear Column";
                case Specials.Darkness:
                    return "Darkness";
                case Specials.Confusion:
                    return "Confusion";
                //case Specials.ZebraField: // will be available when Left Gravity is implemented
                //    return "Zebra Field";
            }
            return special.ToString();
        }
    }
}
