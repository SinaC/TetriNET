namespace TetriNET.Common.Interfaces
{
    //  x-axis: 1 -> Width
    //  y-axis: 1(Bottom) -> Height(Top)
    public interface IBoard
    {
        int Width { get; }
        int Height { get; }
        byte[] Cells { get; }

        IBoard Clone();
        bool CopyFrom(IBoard board);
        void Clear();

        bool SetCells(byte[] cells);
        int TotalCells { get; }
        byte this[int x, int y] { get; }
        int GetCellIndex(int x, int y);

        int TetriminoSpawnX { get; }
        int TetriminoSpawnY { get; }

        bool CheckNoConflict(ITetrimino piece);
        int CollapseCompletedRows();
        void GetAccessibleTranslationsForOrientation(ITetrimino piece, out bool isMovePossible, out int minDeltaX, out int maxDeltaX);
        void CommitPiece(ITetrimino piece);
        void DropAndCommit(ITetrimino piece);

        void Drop(ITetrimino piece);
        bool MoveLeft(ITetrimino piece);
        bool MoveRight(ITetrimino piece);
        bool MoveDown(ITetrimino piece);
        bool RotateClockwise(ITetrimino piece);
        bool RotateCounterClockwise(ITetrimino piece);

        #region Specials
        void AddLines(int count);
        void ClearLine();
        void NukeField();
        void RandomBlocksClear(int count);
        void SwitchFields(byte[] cells);
        void ClearSpecialBlocks();
        void BlockGravity();
        void BlockQuake();
        void BlockBomb();
        #endregion
    }

}
