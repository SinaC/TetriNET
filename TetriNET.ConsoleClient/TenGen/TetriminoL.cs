using TetriNET.Common;

namespace TetriNET.Client.TenGen
{
    public class TetriminoL : Tetrimino
    {
        private static readonly byte[][] Rotations = new byte[][]
        {
            new byte[]
            {
                0, 0, 3,
                3, 3, 3,
                0, 0, 0
            },
            new byte[]
            {
                3, 0, 0,
                3, 0, 0,
                3, 3, 0
            },
            new byte[]
            {
                3, 3, 3,
                3, 0, 0,
                0, 0, 0
            }, 
            new byte[]
            {
                3, 3, 0,
                0, 3, 0,
                0, 3, 0
            } 
        };

        public TetriminoL(int gridWidth, int gridHeight)
            : base(3, 3, gridWidth, gridHeight)
        {
        }

        public override Tetriminos TetriminoValue
        {
            get { return Tetriminos.TetriminoL; }
        }


        protected override byte[] CurrentRotation(int rotation)
        {
            return Rotations[rotation];
        }
    }
}
