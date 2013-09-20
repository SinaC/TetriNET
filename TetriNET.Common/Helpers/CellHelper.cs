using TetriNET.Common.DataContracts;

namespace TetriNET.Common.Helpers
{
    public static class CellHelper
    {
        public static byte EmptyCell = 0;

        public static bool IsSpecial(byte cellValue)
        {
            return cellValue > (byte)Pieces.MaxPieces;
        }

        public static byte SetSpecial(Specials special)
        {
            return (byte) special;
        }

        public static Specials GetSpecial(byte cellValue)
        {
            return IsSpecial(cellValue) ? (Specials) cellValue : Specials.Invalid;
        }

        public static byte SetColor(Pieces piece)
        {
            return (byte) piece;
        }

        public static Pieces GetColor(byte cellValue)
        {
            return (cellValue <= (byte)Pieces.MaxPieces) ? (Pieces)cellValue : Pieces.Invalid;
        }
    }
}
