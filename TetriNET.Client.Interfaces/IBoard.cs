using System;
using System.Collections.Generic;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Interfaces
{
    //  x-axis: 1(Left) -> Width(Right)
    //  y-axis: 1(Bottom) -> Height(Top)
    public interface IBoard
    {
        int Width { get; }
        int Height { get; }
        byte[] Cells { get; }

        IBoard Clone();
        bool CopyFrom(IBoard board);
        void Clear();
        void FillWithRandomCells(Func<Pieces> randomFunc);

        bool SetCells(byte[] cells);
        int TotalCells { get; }
        byte this[int x, int y] { get; }
        int GetCellIndex(int x, int y);
        int NonEmptyCellsCount { get; }

        int PieceSpawnX { get; }
        int PieceSpawnY { get; }

        bool CheckNoConflict(IPiece piece, bool checkTop = false);
        //bool CheckNoConflictWithBoard(IPiece piece, bool checkTop = false);
        int CollapseCompletedRows();
        int CollapseCompletedRows(out List<Specials> specials, out List<Pieces> pieces);
        void CommitPiece(IPiece piece);
        void DropAndCommit(IPiece piece);

        void Drop(IPiece piece);
        bool MoveLeft(IPiece piece);
        bool MoveRight(IPiece piece);
        bool MoveDown(IPiece piece);
        bool RotateClockwise(IPiece piece);
        bool RotateCounterClockwise(IPiece piece);

        #region Specials

        // TetriNET 1
        void AddLines(int count, Func<Pieces> randomFunc);
        void ClearLine();
        void NukeField();
        void RandomBlocksClear(int count);
        void SwitchFields(byte[] cells);
        void ClearSpecialBlocks(Func<Pieces> randomFunc);
        void BlockGravity();
        void BlockQuake();
        void BlockBomb();

        // TetriNET 2
        void ClearColumn();

        // Blocktrix
        void ZebraField();
        void LeftGravity();

        //
        void SpawnSpecialBlocks(int count, Func<Specials> randomFunc);
        void RemoveCellsHigherThan(int height);
        #endregion
    }
}
