using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Client.Board;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.Tests.Client
{
    [TestClass]
    public abstract class GenericBoardTest
    {
        protected abstract IBoard CreateBoard(int width, int height);

        [TestMethod]
        public void TestConstructor()
        {
            const int width = 11;
            const int height = 9;
            IBoard board = CreateBoard(width, height);

            Assert.IsNotNull(board.Cells);
            Assert.AreEqual(board.Width, width);
            Assert.AreEqual(board.Height, height);
            Assert.AreEqual(board.Cells.Length, width*height);
        }

        [TestMethod]
        public void TestClone()
        {
            const int width = 11;
            const int height = 9;
            IBoard board = CreateBoard(width, height);
            for(int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board.Cells[x + y*width] = (byte)(x ^ y);

            IBoard cloned = board.Clone();

            Assert.IsNotNull(cloned);
            Assert.AreEqual(cloned.Width, width);
            Assert.AreEqual(cloned.Height, height);
            Assert.AreEqual(cloned.Cells.Length, width * height);
            Assert.IsTrue(Enumerable.Range(0, width*height).All(i => cloned.Cells[i] == board.Cells[i]));
        }

        [TestMethod]
        public void TestCopyFromFailedIfSizeDifferent()
        {
            const int width = 11;
            const int height = 9;
            IBoard board1 = CreateBoard(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board1.Cells[x + y * width] = (byte)(x ^ y);
            IBoard board2 = CreateBoard(width*2, height*2);
            for(int y = 0; y < board2.Height; y++)
                for (int x = 0; x < board2.Width; x++)
                    board2.Cells[x + y*board2.Width] = (byte) (x & y);

            bool copied = board2.CopyFrom(board1);

            Assert.IsFalse(copied);
        }

        [TestMethod]
        public void TestCopyFromSucceed()
        {
            const int width = 11;
            const int height = 9;
            IBoard board1 = CreateBoard(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board1.Cells[x + y * width] = (byte)(x ^ y);
            IBoard board2 = CreateBoard(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board2.Cells[x + y * width] = (byte)(x & y);

            bool copied = board2.CopyFrom(board1);

            Assert.IsTrue(copied);
            Assert.IsTrue(Enumerable.Range(0, width*height).All(i => board2.Cells[i] == board1.Cells[i]));
        }

        [TestMethod]
        public void TestClear()
        {
            const int width = 11;
            const int height = 9;
            IBoard board = CreateBoard(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board.Cells[x + y * width] = (byte)(x ^ y);

            board.Clear();

            Assert.IsTrue(board.Cells.All(x => x == CellHelper.EmptyCell));
        }

        [TestMethod]
        public void TestFillWithRandomCells()
        {
            const int width = 11;
            const int height = 9;
            IBoard board = CreateBoard(width, height);

            board.FillWithRandomCells(() => Pieces.TetriminoO);

            Assert.IsTrue(board.Cells.All(x => CellHelper.GetColor(x) == Pieces.TetriminoO));
        }

        [TestMethod]
        public void TestSetCellsFailedIfDifferenceSize()
        {
            const int width = 11;
            const int height = 9;
            IBoard board1 = CreateBoard(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board1.Cells[x + y * width] = (byte)(x ^ y);
            IBoard board2 = CreateBoard(width * 2, height * 2);
            for (int y = 0; y < board2.Height; y++)
                for (int x = 0; x < board2.Width; x++)
                    board2.Cells[x + y * board2.Width] = (byte)(x & y);

            bool isSet = board2.SetCells(board1.Cells);

            Assert.IsFalse(isSet);
        }

        [TestMethod]
        public void TestSetCellsSucceed()
        {
            const int width = 11;
            const int height = 9;
            IBoard board1 = CreateBoard(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board1.Cells[x + y * width] = (byte)(x ^ y);
            IBoard board2 = CreateBoard(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board2.Cells[x + y * width] = (byte)(x & y);

            bool isSet = board2.SetCells(board1.Cells);

            Assert.IsTrue(isSet);
            Assert.IsTrue(Enumerable.Range(0, width * height).All(i => board2.Cells[i] == board1.Cells[i]));
        }

        [TestMethod]
        public void TestTotalCells()
        {
            const int width = 11;
            const int height = 9;
            IBoard board = CreateBoard(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board.Cells[x + y * width] = (byte)(x ^ y);
            
            int count = board.TotalCells;

            Assert.AreEqual(count, width*height);
        }

        [TestMethod]
        public void TestIndexerGetFailedIfWrongIndices()
        {
            const int width = 11;
            const int height = 9;
            IBoard board = CreateBoard(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board.Cells[x + y * width] = CellHelper.SetColor(Pieces.TetriminoL); // different from EmptyCell

            byte value = board[width + 1, height + 1]; // coordinates are [1, width] and [1, height]

            Assert.AreEqual(value, CellHelper.EmptyCell);
        }

        [TestMethod]
        public void TestIndexerGetFailedIfWrongIndices2()
        {
            const int width = 11;
            const int height = 9;
            IBoard board = CreateBoard(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board.Cells[x + y * width] = CellHelper.SetColor(Pieces.TetriminoL); // different from EmptyCell

            byte value = board[0, 0]; // coordinates are [1, width] and [1, height]

            Assert.AreEqual(value, CellHelper.EmptyCell);
        }

        [TestMethod]
        public void TestGetCellIndex()
        {
            const int width = 11;
            const int height = 9;
            IBoard board = CreateBoard(width, height);

            int x = 10;
            int y = 5;
            int cellIndex = board.GetCellIndex(x, y); // coordinates are [1, width] and [1, height]

            Assert.AreEqual(cellIndex, (x-1)+(y-1)*width);
        }

        [TestMethod]
        public void TestNonEmptyCellsCount()
        {
            const int width = 11;
            const int height = 9;
            IBoard board = CreateBoard(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board.Cells[x + y*width] = CellHelper.SetColor(Pieces.TetriminoL); // different from EmptyCell
            board.Cells[(width-1) + (height-1)*width] = CellHelper.EmptyCell;
            board.Cells[0] = CellHelper.EmptyCell;

            int count = board.NonEmptyCellsCount;

            Assert.AreEqual(count, width*height-2);
        }

        // No tests on PieceSpawnX and PieceSpawnY, those are IBoard implementation specific

        // TODO: 
        //bool CheckNoConflict(IPiece piece, bool checkTop = false);
        //int CollapseCompletedRows();
        //int CollapseCompletedRows(out List<Specials> specials, out List<Pieces> pieces);
        //void CommitPiece(IPiece piece);
        //void DropAndCommit(IPiece piece);

        //void Drop(IPiece piece);
        //bool MoveLeft(IPiece piece);
        //bool MoveRight(IPiece piece);
        //bool MoveDown(IPiece piece);
        //bool RotateClockwise(IPiece piece);
        //bool RotateCounterClockwise(IPiece piece);

        //void AddLines(int count, Func<Pieces> randomFunc);
        //void ClearLine();
        //void NukeField();
        //void RandomBlocksClear(int count);
        //void SwitchFields(byte[] cells);
        //void ClearSpecialBlocks(Func<Pieces> randomFunc);
        //void BlockGravity();
        //void BlockQuake();
        //void BlockBomb();

        //void ClearColumn();

        //void ZebraField();
        //void LeftGravity();

        //void SpawnSpecialBlocks(int count, Func<Specials> randomFunc);
        //void RemoveCellsHigherThan(int height);
    }

    [TestClass]
    public class BoardTest : GenericBoardTest
    {
        protected override IBoard CreateBoard(int width, int height)
        {
            return new Board(width, height);
        }
    }

    [TestClass]
    public class BoardWithWallKickTest : GenericBoardTest
    {
        protected override IBoard CreateBoard(int width, int height)
        {
            return new BoardWithWallKick(width, height);
        }
    }
}
