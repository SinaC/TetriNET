namespace TetriNET.Common.Interfaces
{
    //public interface ITetrimino
    //{
    //    Tetriminos TetriminoValue { get; }

    //    byte[] Parts { get; }
        
    //    int PosX { get; }
    //    int PosY { get; }

    //    int GridWidth { get; }
    //    int GridHeight { get; }
        
    //    int Width { get; }
    //    int Height { get; }

    //    int LinearPosInGrid(int linearPosInPart);

    //    bool CheckConflict(byte[] grid);

    //    bool MoveLeft(byte[] grid);
    //    bool MoveRight(byte[] grid);
    //    bool MoveDown(byte[] grid);
    //    bool MoveUp(byte[] grid);
    //    bool RotateClockwise(byte[] grid);
    //    bool RotateCounterClockwise(byte[] grid);

    //    void Dump();
    //}

    public interface ITetrimino
    {
        int PosX { get; } // coordinates in board
        int PosY { get; } // coordinates in board
        int Orientation { get; } // 1 -> 4

        byte Value { get; } // bit 0->3: color     bit 4->7: special      use ByteHelper to get Tetrimino(color) and Special

        int MaxOrientations { get; }
        int TotalCells { get; }
        void GetCellAbsolutePosition(int cellIndex, out int x, out int y); // cell: 1->#cells

        ITetrimino Clone();
        void CopyFrom(ITetrimino piece);

        void Translate(int dx, int dy);
        void RotateClockwise();
        void RotateCounterClockwise();
        void Rotate(int count);
    }
}
