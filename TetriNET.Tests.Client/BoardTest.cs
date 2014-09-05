using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Client.Board;
using TetriNET.Client.Interfaces;

namespace TetriNET.Tests.Client
{
    [TestClass]
    public abstract class IBoardTest
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

        // TODO: test every IBoard methods
    }

    [TestClass]
    public class BoardTest : IBoardTest
    {
        protected override IBoard CreateBoard(int width, int height)
        {
            return new Board(width, height);
        }
    }

    [TestClass]
    public class BoardWithWallKickTest : IBoardTest
    {
        protected override IBoard CreateBoard(int width, int height)
        {
            return new BoardWithWallKick(width, height);
        }
    }
}
