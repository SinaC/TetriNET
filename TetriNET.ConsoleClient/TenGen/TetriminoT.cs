using TetriNET.Common;

namespace TetriNET.Client.TenGen
{
    public class TetriminoT : Tetrimino
    {
        private static readonly byte[][] Rotations =
        {
            new byte[]
            {
                0, 6, 0,
                6, 6, 6,
                0, 0, 0
            },
            new byte[]
            {
                6, 0, 0,
                6, 6, 0,
                6, 0, 0
            },
            new byte[]
            {
                6, 6, 6,
                0, 6, 0,
                0, 0, 0
            },
            new byte[]
            {
                0, 6, 0,
                6, 6, 0,
                0, 6, 0
            }
        };

        public TetriminoT(int width, int height)
            : base(3, 3, width, height)
        {
        }

        public override Tetriminos TetriminoValue
        {
            get { return Tetriminos.TetriminoT; }
        }


        protected override byte[] CurrentRotation(int rotation)
        {
            return Rotations[rotation];
        }
    }
}