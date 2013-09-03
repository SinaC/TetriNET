namespace TetriNET.Common.Helpers
{
    public static class CellHelper
    {
        public static Tetriminos Tetrimino(byte cellValue)
        {
            return (Tetriminos)(cellValue & 0x0F);
        }

        public static Specials Special(byte cellValue)
        {
            return (Specials)((cellValue & 0xF0) >> 4); 
        }

        public static byte SetTetrimino(Tetriminos tetrimino)
        {
            return (byte)((int)tetrimino & 0x0F);
        }

        public static byte SetSpecial(byte cellValue, Specials special)
        {
            return (byte)((cellValue & 0x0F) | ((int) special << 4));
        }

        public static byte ClearSpecial(byte cellValue)
        {
            return (byte)(cellValue & 0x0F);
        }
    }
}
