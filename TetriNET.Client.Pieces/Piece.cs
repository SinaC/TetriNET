using TetriNET.Client.Interfaces;
using TetriNET.Common.Logger;

namespace TetriNET.Client.Pieces
{
    public abstract class Piece : IPiece
    {
        public int PosX { get; protected set; } // coordinates in board
        public int PosY { get; protected set; } // coordinates in board
        public int Orientation { get; protected set; } // 1 -> 4

        public int Index { get; protected set; }

        public Common.DataContracts.Pieces Value { get; protected set; }
        
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

        public void Move(int x, int y)
        {
            PosX = x;
            PosY = y;
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

        public static IPiece CreatePiece(Common.DataContracts.Pieces piece, int spawnX, int spawnY, int spawnOrientation, int index)
        {
            switch (piece)
            {
                case Common.DataContracts.Pieces.TetriminoI:
                    return new SRS.TetriminoI(spawnX, spawnY, spawnOrientation, index);
                case Common.DataContracts.Pieces.TetriminoJ:
                    return new SRS.TetriminoJ(spawnX, spawnY, spawnOrientation, index);
                case Common.DataContracts.Pieces.TetriminoL:
                    return new SRS.TetriminoL(spawnX, spawnY, spawnOrientation, index);
                case Common.DataContracts.Pieces.TetriminoO:
                    return new SRS.TetriminoO(spawnX, spawnY, spawnOrientation, index);
                case Common.DataContracts.Pieces.TetriminoS:
                    return new SRS.TetriminoS(spawnX, spawnY, spawnOrientation, index);
                case Common.DataContracts.Pieces.TetriminoT:
                    return new SRS.TetriminoT(spawnX, spawnY, spawnOrientation, index);
                case Common.DataContracts.Pieces.TetriminoZ:
                    return new SRS.TetriminoZ(spawnX, spawnY, spawnOrientation, index);
                case Common.DataContracts.Pieces.Invalid:
                    Log.WriteLine(Log.LogLevels.Warning, "Create random cell because server didn't send next cell");
                    return new SRS.TetriminoZ(spawnX, spawnY, spawnOrientation, index); // TODO: sometimes server takes time to send next cell, it should send 2 or 3 next pieces to ensure this never happens
            }
            Log.WriteLine(Log.LogLevels.Warning, "Unknown piece {0}", piece);
            return new SRS.TetriminoZ(spawnX, spawnY, spawnOrientation, index);
        }

        public static IPiece CreatePiece(Common.DataContracts.Pieces piece, int spawnX, int spawnY, int spawnOrientation, int index, bool isMutationActive)
        {
            if (isMutationActive)
            {
                switch (piece)
                {
                    case Common.DataContracts.Pieces.TetriminoI:
                        return new Mutated.MutatedI(spawnX, spawnY, spawnOrientation, index);
                    case Common.DataContracts.Pieces.TetriminoJ:
                        return new Mutated.MutatedJ(spawnX, spawnY, spawnOrientation, index);
                    case Common.DataContracts.Pieces.TetriminoL:
                        return new Mutated.MutatedL(spawnX, spawnY, spawnOrientation, index);
                    case Common.DataContracts.Pieces.TetriminoO:
                        return new Mutated.MutatedO(spawnX, spawnY, spawnOrientation, index);
                    case Common.DataContracts.Pieces.TetriminoS:
                        return new Mutated.MutatedS(spawnX, spawnY, spawnOrientation, index);
                    case Common.DataContracts.Pieces.TetriminoT:
                        return new Mutated.MutatedT(spawnX, spawnY, spawnOrientation, index);
                    case Common.DataContracts.Pieces.TetriminoZ:
                        return new Mutated.MutatedZ(spawnX, spawnY, spawnOrientation, index);
                    //
                    case Common.DataContracts.Pieces.Invalid:
                        Log.WriteLine(Log.LogLevels.Warning, "Create random cell because server didn't send next cell");
                        return CreatePiece(Common.DataContracts.Pieces.TetriminoZ, spawnX, spawnY, spawnOrientation, index); // TODO: sometimes server takes time to send next cell, it should send 2 or 3 next pieces to ensure this never happens
                }
            }
            else
                return CreatePiece(piece, spawnX, spawnY, spawnOrientation, index);
            Log.WriteLine(Log.LogLevels.Warning, "Unknow piece {0}", piece);
            return CreatePiece(Common.DataContracts.Pieces.TetriminoZ, spawnX, spawnY, spawnOrientation, index);
        }
    }
}
