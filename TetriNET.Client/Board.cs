namespace TetriNET.Client
{
    public class Board
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] Cells { get; private set; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new byte[Width*Height];
        }

        public Board Clone()
        {
            Board board = new Board(Width, Height);
            Cells.CopyTo(board.Cells, 0);
            return board;
        }

        public bool SetCells(byte[] cells)
        {
            if (cells.Length != Width * Height)
                return false;
            cells.CopyTo(Cells, 0);
            return true;
        }
    }
}
