using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Interfaces
{
    public interface IPiece
    {
        int PosX { get; } // coordinates in board
        int PosY { get; } // coordinates in board
        int Orientation { get; } // 1 -> 4
        int Index { get; } // index in sequence

        Pieces Value { get; }

        int MaxOrientations { get; }
        int TotalCells { get; }
        void GetCellAbsolutePosition(int cellIndex, out int x, out int y); // piece: 1->#cells

        IPiece Clone();
        void CopyFrom(IPiece piece);

        void Move(int x, int y);
        void Translate(int dx, int dy);
        void RotateClockwise();
        void RotateCounterClockwise();
        void Rotate(int count);

        void GetAbsoluteBoundingRectangle(out int minX, out int minY, out int maxX, out int maxY);
    }
}
