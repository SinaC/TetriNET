using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;

namespace TetriNET.Client.DefaultBoardAndTetriminos
{
    public abstract class Tetrimino : ITetrimino
    {
        public int PosX { get; protected set; } // coordinates in board
        public int PosY { get; protected set; } // coordinates in board
        public int Orientation { get; protected set; } // 1 -> 4

        public int Index { get; protected set; }

        public Tetriminos Value { get; protected set; }
        
        public abstract int MaxOrientations { get; }
        public abstract int TotalCells { get; }
        public abstract void GetCellAbsolutePosition(int cellIndex, out int x, out int y); // cell: 1->#cells
        public abstract ITetrimino Clone();

        protected Tetrimino()
        {
        }

        protected Tetrimino(int spawnX, int spawnY, int spawnOrientation, int index)
        {
            PosX = spawnX;
            PosY = spawnY;
            Orientation = spawnOrientation;
            Index = index;
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

        public static ITetrimino CreateTetrimino(Tetriminos tetrimino, int spawnX, int spawnY, int spawnOrientation, int index)
        {
            switch (tetrimino)
            {
                case Tetriminos.TetriminoI:
                    return new TetriminoI(spawnX, spawnY, spawnOrientation, index);
                case Tetriminos.TetriminoJ:
                    return new TetriminoJ(spawnX, spawnY, spawnOrientation, index);
                case Tetriminos.TetriminoL:
                    return new TetriminoL(spawnX, spawnY, spawnOrientation, index);
                case Tetriminos.TetriminoO:
                    return new TetriminoO(spawnX, spawnY, spawnOrientation, index);
                case Tetriminos.TetriminoS:
                    return new TetriminoS(spawnX, spawnY, spawnOrientation, index);
                case Tetriminos.TetriminoT:
                    return new TetriminoT(spawnX, spawnY, spawnOrientation, index);
                case Tetriminos.TetriminoZ:
                    return new TetriminoZ(spawnX, spawnY, spawnOrientation, index);
            }
            Logger.Log.WriteLine(Logger.Log.LogLevels.Warning, "Create random Tetrimino because server didn't send next tetrimino");
            return new TetriminoZ(spawnX, spawnY, spawnOrientation, index); // TODO: sometimes server takes time to send next tetrimino, it should send 2 or 3 next tetriminoes to ensure this never happens
        }
    }
}
