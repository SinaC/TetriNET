using TetriNET.Common;

namespace TetriNET.ConsoleWCFClient.TenGen
{
    public class TetriminoI : Tetrimino
    {
        private static readonly byte[][] Rotations =
        {
            new byte[]
            {
                1, 1, 1, 1,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            },
            new byte[]
            {
                0, 0, 1, 0,
                0, 0, 1, 0,
                0, 0, 1, 0,
                0, 0, 1, 0
            }
        };

        public TetriminoI(int gridWidth, int gridHeight) 
            : base(4, 4, gridWidth, gridHeight)
        {
        }

        public override Tetriminos TetriminoValue
        {
            get { return Tetriminos.TetriminoI; }
        }

        protected override byte[] CurrentRotation(int rotation)
        {
            return Rotations[rotation%2];
        }
    }
}
