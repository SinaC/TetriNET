namespace TetriNET.Client
{
    public interface ITetrimino
    {
        Common.Tetriminos TetriminoValue { get; }

        byte[] Parts { get; }
        
        int PosX { get; }
        int PosY { get; }

        int GridWidth { get; }
        int GridHeight { get; }
        
        int Width { get; }
        int Height { get; }

        bool CheckConflict(byte[] grid);

        bool MoveLeft(byte[] grid);
        bool MoveRight(byte[] grid);
        bool MoveDown(byte[] grid);
        bool MoveUp(byte[] grid);
        bool RotateClockwise(byte[] grid);
        bool RotateCounterClockwise(byte[] grid);

        void Dump();
    }
}
