using TetriNET.Common.Interfaces;

namespace TetriNET.ConsoleWCFClient
{
    public abstract class Tetrimino : ITetrimino
    {
        public int PosX { get; protected set; } // coordinates in board
        public int PosY { get; protected set; } // coordinates in board
        public int Orientation { get; protected set; } // 1 -> 4

        public byte Value { get; protected set; } // bit 0->3: color     bit 4->7: special
        
        public byte Color {
            get { return (byte)(Value & 0x0F); }
        }

        public byte Special
        {
            get { return (byte)((Value & 0xF0) >> 4); }
        }

        public abstract int MaxOrientations { get; }
        public abstract int TotalCells { get; }
        public abstract void GetCellAbsolutePosition(int cellIndex, out int x, out int y); // cell: 1->#cells
        public abstract ITetrimino Clone();

        protected Tetrimino()
        {
        }

        protected Tetrimino(int spawnX, int spawnY, int spawnOrientation)
        {
            PosX = spawnX;
            PosY = spawnY;
            Orientation = spawnOrientation;
        }

        public void CopyFrom(ITetrimino piece)
        {
            // TODO: test if same type of piece
            PosX = piece.PosX;
            PosY = piece.PosY;
            Orientation = piece.Orientation;
            Value = piece.Value;
        }

        public void Translate(int dx, int dy)
        {
            PosX += dx;
            PosY += dy;
        }

        public void RotateClockwise()
        {
            int newOrientation = Orientation + 1;
            // 1->4
            Orientation = 1 + (((newOrientation - 1)%MaxOrientations) + MaxOrientations)%MaxOrientations;
        }

        public void RotateCounterClockwise()
        {
            int newOrientation = Orientation - 1;
            // 1->4
            Orientation = 1 + (((newOrientation - 1)%MaxOrientations) + MaxOrientations)%MaxOrientations;
        }

        public void Rotate(int count)
        {
            int total = ((count/MaxOrientations) + MaxOrientations)%MaxOrientations; // 0 -> 3

            for (int step = 0; step < total; step++)
                RotateClockwise();
        }
    }
}
