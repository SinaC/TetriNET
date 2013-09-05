using System.Windows.Media;
using TetriNET.Common.GameDatas;

namespace TetriNET.WPF_WCF_Client.Controls
{
    public static class Mapper
    {
        // TetriNET color scheme
        static private readonly SolidColorBrush TetriminoIColor = new SolidColorBrush(Colors.Blue);
        static private readonly SolidColorBrush TetriminoJColor = new SolidColorBrush(Colors.Green);
        static private readonly SolidColorBrush TetriminoLColor = new SolidColorBrush(Colors.Magenta);
        static private readonly SolidColorBrush TetriminoOColor = new SolidColorBrush(Colors.Yellow);
        static private readonly SolidColorBrush TetriminoSColor = new SolidColorBrush(Colors.Blue);
        static private readonly SolidColorBrush TetriminoTColor = new SolidColorBrush(Colors.Yellow);
        static private readonly SolidColorBrush TetriminoZColor = new SolidColorBrush(Colors.Red);

        public static SolidColorBrush MapTetriminoToColor(Tetriminos tetrimino)
        {
            switch (tetrimino)
            {
                case Tetriminos.TetriminoI:
                    return TetriminoIColor;
                case Tetriminos.TetriminoJ:
                    return TetriminoJColor;
                case Tetriminos.TetriminoL:
                    return TetriminoLColor;
                case Tetriminos.TetriminoO:
                    return TetriminoOColor;
                case Tetriminos.TetriminoS:
                    return TetriminoSColor;
                case Tetriminos.TetriminoT:
                    return TetriminoTColor;
                case Tetriminos.TetriminoZ:
                    return TetriminoZColor;
            }
            return new SolidColorBrush(Colors.Pink);
        }

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
                case Specials.ZebraField:
                    return 'Z';
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
                case Specials.ZebraField:
                    return "Zebra Field";
            }
            return special.ToString();
        }
    }
}
