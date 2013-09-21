using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;

namespace TetriNET.DefaultBoardAndPieces
{
    public abstract class Piece : IPiece
    {
        public int PosX { get; protected set; } // coordinates in board
        public int PosY { get; protected set; } // coordinates in board
        public int Orientation { get; protected set; } // 1 -> 4

        public int Index { get; protected set; }

        public Pieces Value { get; protected set; }
        
        public abstract int MaxOrientations { get; }
        public abstract int TotalCells { get; }
        public abstract void GetCellAbsolutePosition(int cellIndex, out int x, out int y); // cell: 1->#cells
        public abstract IPiece Clone();

        protected Piece()
        {
        }

        protected Piece(int spawnX, int spawnY, int spawnOrientation, int index)
        {
            PosX = spawnX;
            PosY = spawnY;
            Orientation = spawnOrientation;
            Index = index;
        }

        public void CopyFrom(IPiece piece)
        {
            // TODO: test if same type of cell
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

        public static IPiece CreatePiece(Pieces piece, int spawnX, int spawnY, int spawnOrientation, int index)
        {
            switch (piece)
            {
                case Pieces.TetriminoI:
                    return new TetriminoI(spawnX, spawnY, spawnOrientation, index);
                case Pieces.TetriminoJ:
                    return new TetriminoJ(spawnX, spawnY, spawnOrientation, index);
                case Pieces.TetriminoL:
                    return new TetriminoL(spawnX, spawnY, spawnOrientation, index);
                case Pieces.TetriminoO:
                    return new TetriminoO(spawnX, spawnY, spawnOrientation, index);
                case Pieces.TetriminoS:
                    return new TetriminoS(spawnX, spawnY, spawnOrientation, index);
                case Pieces.TetriminoT:
                    return new TetriminoT(spawnX, spawnY, spawnOrientation, index);
                case Pieces.TetriminoZ:
                    return new TetriminoZ(spawnX, spawnY, spawnOrientation, index);
                case Pieces.Invalid:
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Warning, "Create random cell because server didn't send next cell");
                    return new TetriminoZ(spawnX, spawnY, spawnOrientation, index); // TODO: sometimes server takes time to send next cell, it should send 2 or 3 next pieces to ensure this never happens
            }
            Logger.Log.WriteLine(Logger.Log.LogLevels.Warning, "Unknown piece {0}", piece);
            return new TetriminoZ(spawnX, spawnY, spawnOrientation, index);
        }
    }
}
