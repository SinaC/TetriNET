using TetriNET.Common;

namespace TetriNET.ConsoleWCFClient.TenGen
{
    public class TetriminoO : Tetrimino
    {
        private static readonly byte[] Rotations =
        {
            4, 4,
            4, 4,
        };

        public TetriminoO(int gridWidth, int gridHeight)
            : base(2, 2, gridWidth, gridHeight)
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
