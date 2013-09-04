using System;
using System.Collections.Generic;
using TetriNET.Common.GameDatas;

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
        void FillWithRandomCells(Func<Tetriminos> randomFunc);

        bool SetCells(byte[] cells);
        int TotalCells { get; }
        byte this[int x, int y] { get; }
        int GetCellIndex(int x, int y);
        int NonEmptyCellsCount { get; }

        int TetriminoSpawnX { get; }
        int TetriminoSpawnY { get; }

        bool CheckNoConflict(ITetrimino tetrimino);
        bool CheckNoConflictWithBoard(ITetrimino tetrimino);
        int CollapseCompletedRows(out List<Specials> specials);
        void GetAccessibleTranslationsForOrientation(ITetrimino tetrimino, out bool isMovePossible, out int minDeltaX, out int maxDeltaX);
        void CommitTetrimino(ITetrimino tetrimino);
        void DropAndCommit(ITetrimino tetrimino);

        void Drop(ITetrimino tetrimino);
        bool MoveLeft(ITetrimino tetrimino);
        bool MoveRight(ITetrimino tetrimino);
        bool MoveDown(ITetrimino tetrimino);
        bool RotateClockwise(ITetrimino tetrimino);
        bool RotateCounterClockwise(ITetrimino tetrimino);

        #region Specials

        // TetriNET 1
        void AddLines(int count, Func<Tetriminos> randomFunc);
        void ClearLine();
        void NukeField();
        void RandomBlocksClear(int count);
        void SwitchFields(byte[] cells);
        void ClearSpecialBlocks(Func<Tetriminos> randomFunc);
        void BlockGravity();
        void BlockQuake();
        void BlockBomb();

        // TetriNET 2
        void ClearColumn();

        // Blocktrix
        void ZebraField();

        //
        void SpawnSpecialBlocks(int count, Func<Specials> randomFunc);
        void RemoveCellsHigherThan(int height);
        #endregion
    }
}
