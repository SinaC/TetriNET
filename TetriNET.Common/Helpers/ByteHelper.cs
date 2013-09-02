namespace TetriNET.Common.Helpers
{
    public static class ByteHelper
    {
        public static Tetriminos Tetrimino(byte cellValue)
        {
            return (Tetriminos)(cellValue & 0x0F);
        }

        public static Specials Special(byte cellValue)
        {
            return (Specials)((cellValue & 0xF0) >> 4); 
        }
    }
}
