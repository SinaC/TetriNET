using TetriNET.Common;

namespace TetriNET.Client.TenGen
{
    public class TetriminoS : Tetrimino
    {
        private static readonly byte[][] Rotations =
        {
            new byte[]
            {
                0, 5, 5,
                5, 5, 0,
                0, 0, 0,
            },
            new byte[]
            {
                5, 0, 0,
                5, 5, 0,
                0, 5, 0,
            }
        };
        public TetriminoS(int gridWidth, int gridHeight) 
            : base(3, 3, gridWidth, gridHeight)
        {
        }

        public override Tetriminos TetriminoValue
        {
            get { return Tetriminos.TetriminoS; }
        }


        protected override byte[] CurrentRotation(int rotation)
        {
            return Rotations[rotation % 2];
        }
    }
}
