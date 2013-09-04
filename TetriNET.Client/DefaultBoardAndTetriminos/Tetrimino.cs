using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;

namespace TetriNET.Client.DefaultBoardAndTetriminos
{
    public abstract class Tetrimino : ITetrimino
    {
        public int PosX { get; protected set; } // coordinates in board
        public int PosY { get; protected set; } // coordinates in board
        public int Orientation { get; protected set; } // 1 -> 4

        public Tetriminos Value { get; protected set; }
        
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

        public void CopyFrom(ITetrimino tetrimino)
        {
            // TODO: test if same type of tetrimino
            PosX = tetrimino.PosX;
            PosY = tetrimino.PosY;
            Orientation = tetrimino.Orientation;
            Value = tetrimino.Value;
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
            int total = ((count%MaxOrientations) + MaxOrientations)%MaxOrientations; // 0 -> 3

            for (int step = 0; step < total; step++)
                RotateClockwise();
        }

        public void GetAbsoluteBoundingRectangle(out int minX, out int minY, out int maxX, out int maxY)
        {
            minX = 0;
            minY = 0;
            maxX = 0;
            maxY = 0;

            if (TotalCells < 1) return;

            int x;
            int y;

            // start bounding limits using first cell
            GetCellAbsolutePosition(1, out x, out y); // first cell
            minX = x;
            maxX = x;
            minY = y;
            maxY = y;

            // expand bounding limits with other cells
            for (int cellIndex = 2; cellIndex <= TotalCells; cellIndex++)
            {
                GetCellAbsolutePosition(cellIndex, out x, out y);
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }
    }
}
