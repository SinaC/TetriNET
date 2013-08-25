using TetriNET.Common;

namespace TetriNET.Client.TenGen
{
    public class TetriminoO : Tetrimino
    {
        private static readonly byte[] Rotations =
        {
            4, 4, 0, 0,
            4, 4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0
        };

        public TetriminoO(int gridWidth, int gridHeight)
            : base(4, 4, gridWidth, gridHeight)
        {
        }

        public override Tetriminos TetriminoValue
        {
            get { return Tetriminos.TetriminoO; }
        }


        protected override byte[] CurrentRotation(int rotation)
        {
            return Rotations;
        }
    }
}
