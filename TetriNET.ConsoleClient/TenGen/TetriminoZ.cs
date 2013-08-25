using TetriNET.Common;

namespace TetriNET.Client.TenGen
{
    public class TetriminoZ : Tetrimino
    {
        private static readonly byte[][] Rotations =
        {
            new byte[]
            {
                7, 7, 0,
                0, 7, 7,
                0, 0, 0,
            },
            new byte[]
            {
                0, 7, 0,
                7, 7, 0,
                7, 0, 0,
            }
        };

        public TetriminoZ(int gridWidth, int gridHeight)
            : base(3, 3, gridWidth, gridHeight)
        {
        }

        public override Tetriminos TetriminoValue
        {
            get { return Tetriminos.TetriminoZ; }
        }

        protected override byte[] CurrentRotation(int rotation)
        {
            return Rotations[rotation % 2];
        }
    }
}
