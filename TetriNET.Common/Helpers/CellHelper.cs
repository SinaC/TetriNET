using TetriNET.Common.GameDatas;

namespace TetriNET.Common.Helpers
{
    public static class CellHelper
    {
        public static byte EmptyCell = 0;

        public static bool IsSpecial(byte cellValue)
        {
            return cellValue > (byte)Tetriminos.TetriminoLast;
        }

        public static byte SetSpecial(Specials special)
        {
            return (byte) special;
        }

        public static Specials GetSpecial(byte cellValue)
        {
            return IsSpecial(cellValue) ? (Specials) cellValue : Specials.Invalid;
        }

        public static byte SetColor(Tetriminos tetrimino)
        {
            return (byte) tetrimino;
        }

        public static Tetriminos GetColor(byte cellValue)
        {
            return (cellValue <= (byte)Tetriminos.TetriminoLast) ? (Tetriminos)cellValue : Tetriminos.Invalid;
        }
    }
}
