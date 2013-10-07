using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Pieces.Normal
{
    internal class TetriminoO : Piece
    {
        protected TetriminoO()
        {
        }

        public TetriminoO(int spawnX, int spawnY, int spawnOrientation, int index) : base(spawnX, spawnY, spawnOrientation, index)
        {
            Value = Common.DataContracts.Pieces.TetriminoO;
        }

        public override int MaxOrientations
        {
            get { return 1; }
        }

        public override int TotalCells
        {
            get { return 4; }
        }

        public override void GetCellAbsolutePosition(int cellIndex, out int x, out int y)
        {
            x = y = 0;
            // orientation 1,2,3,4 : (-1, -1),  ( 0, -1),  ( 0,  0),  (-1,  0)
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
                    x = -1;
                    y = 0;
                    break;
            }
            // Translate to board coordinates
            x += PosX;
            y += PosY;
        }

        public override IPiece Clone()
        {
            return new TetriminoO
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
