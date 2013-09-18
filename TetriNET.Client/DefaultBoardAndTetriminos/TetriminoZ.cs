using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;

namespace TetriNET.Client.DefaultBoardAndTetriminos
{
    internal class TetriminoZ : Tetrimino
    {
        protected TetriminoZ()
        {
        }

        public TetriminoZ(int spawnX, int spawnY, int spawnOrientation, int index) : base(spawnX, spawnY, spawnOrientation, index)
        {
            Value = Tetriminos.TetriminoZ;
        }

        public override int MaxOrientations
        {
            get { return 2; }
        }

        public override int TotalCells
        {
            get { return 4; }
        }

        public override void GetCellAbsolutePosition(int cellIndex, out int x, out int y)
        {
            x = y = 0;
            // orientation 1,3: ( 0, -1),  ( 1, -1),  (-1,  0),  ( 0,  0)
            // orientation 2,4: ( 0, -1),  ( 0,  0),  ( 1,  0),  ( 1,  1)
            if (Orientation == 1 || Orientation == 3)
            {
                switch (cellIndex)
                {
                    case 1:
                        x = 0;
                        y = -1;
                        break;
                    case 2:
                        x = 1;
                        y = -1;
                        break;
                    case 3:
                        x = -1;
                        y = 0;
                        break;
                    case 4:
                        x = 0;
                        y = 0;
                        break;
                }
            }
            else // 2 or 4
            {
                switch (cellIndex)
                {
                    case 1:
                        x = 0;
                        y = -1;
                        break;
                    case 2:
                        x = 0;
                        y = 0;
                        break;
                    case 3:
                        x = 1;
                        y = 0;
                        break;
                    case 4:
                        x = 1;
                        y = 1;
                        break;
                }
            }
            // Translate to board coordinates
            x += PosX;
            y += PosY;
        }

        public override ITetrimino Clone()
        {
            return new TetriminoZ
            {
                PosX = PosX,
                PosY = PosY,
                Orientation = Orientation,
                Value = Value,
                Index = Index
            };
        }
    }
}
