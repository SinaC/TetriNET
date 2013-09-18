using TetriNET.Common.DataContracts;

namespace TetriNET.Common.Interfaces
{
    public interface ITetrimino
    {
        int PosX { get; } // coordinates in board
        int PosY { get; } // coordinates in board
        int Orientation { get; } // 1 -> 4
        int Index { get; } // index in sequence

        Tetriminos Value { get; }

        int MaxOrientations { get; }
        int TotalCells { get; }
        void GetCellAbsolutePosition(int cellIndex, out int x, out int y); // cell: 1->#cells

        ITetrimino Clone();
        void CopyFrom(ITetrimino tetrimino);

        void Translate(int dx, int dy);
        void RotateClockwise();
        void RotateCounterClockwise();
        void Rotate(int count);

        void GetAbsoluteBoundingRectangle(out int minX, out int minY, out int maxX, out int maxY);
    }
}
