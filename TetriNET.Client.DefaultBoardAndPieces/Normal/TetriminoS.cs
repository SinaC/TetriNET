using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Pieces.Normal
{
    internal class TetriminoS : Piece
    {
        protected TetriminoS()
        {
        }

        public TetriminoS(int spawnX, int spawnY, int spawnOrientation, int index) : base(spawnX, spawnY, spawnOrientation, index)
        {
            Value = Common.DataContracts.Pieces.TetriminoS;
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
            // orientation 1,3: (-1, -1),  ( 0, -1),  ( 0,  0),  ( 1,  0)
            // orientation 2,4: ( 1, -1),  ( 0,  0),  ( 1,  0),  ( 0,  1)
            if (Orientation == 1 || Orientation == 3)
            {
                switch (cellIndex)
                {
                    case 1:
                        x = -1;
                        y = -1;
                        break;
                    case 2:
                        x = 0;
                        y = -1;
                        break;
                    case 3:
                        x = 0;
                        y = 0;
                        break;
                    case 4:
                        x = 1;
                        y = 0;
                        break;
                }
            }
            else // 2 or 4
            {
                switch (cellIndex)
                {
                    case 1:
                        x = 1;
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
                        x = 0;
                        y = 1;
                        break;
                }
            }
            // Translate to board coordinates
            x += PosX;
            y += PosY;
        }

        public override IPiece Clone()
        {
            return new TetriminoS
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
