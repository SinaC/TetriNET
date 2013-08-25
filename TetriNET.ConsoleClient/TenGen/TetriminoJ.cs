using TetriNET.Common;

namespace TetriNET.Client.TenGen
{
    public class TetriminoJ : Tetrimino
    {
        private static readonly byte[][] Rotations =
        {
            new byte[]
            {
                2, 0, 0,
                2, 2, 2,
                0, 0, 0
            },
            new byte[]
            {
                2, 2, 0,
                2, 0, 0,
                2, 0, 0
            },
            new byte[]
            {
                2, 2, 2,
                0, 0, 2,
                0, 0, 0
            }, 
            new byte[]
            {
                0, 2, 0,
                0, 2, 0,
                2, 2, 0
            } 
        };

        public TetriminoJ(int gridWidth, int gridHeight)
            : base(3, 3, gridWidth, gridHeight)
        {
        }

        public override Tetriminos TetriminoValue
        {
            get { return Tetriminos.TetriminoJ; }
        }


        protected override byte[] CurrentRotation(int rotation)
        {
            return Rotations[rotation];
        }
    }
}
