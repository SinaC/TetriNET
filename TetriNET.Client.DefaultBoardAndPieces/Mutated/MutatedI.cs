using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Pieces.Mutated
{
    public class MutatedI : Piece
    {
        protected MutatedI()
        {
        }

        public MutatedI(int posX, int posY, int orientation, int index)
            : base(posX, posY, orientation, index)
        {
            Value = Common.DataContracts.Pieces.TetriminoI;
        }

        public override int MaxOrientations
        {
            get { return 4; }
        }

        public override int TotalCells
        {
            get { return 5; }
        }

        public override void GetCellAbsolutePosition(int cellIndex, out int x, out int y)
        {
            x = y = 0;
            // orientation 1: (-2,  0),  (-1,  0),  ( 0,  0),  ( 1,  0),  (-1,  1)
            // orientation 2: ( 0, -2),  ( 0, -1),  ( 0,  0),  ( 0,  1),  (-1, -1)
            // orientation 3: (-1,  0),  ( 0,  0),  ( 1,  0),  ( 2,  0),  ( 1, -1)
            // orientation 4: ( 0, -1),  ( 0,  0),  ( 0,  1),  ( 0,  2),  ( 1,  1)
            if (Orientation == 1)
            {
                switch (cellIndex)
                {
                    case 1:
                        x = -2;
                        y = 0;
                        break;
                    case 2:
                        x = -1;
                        y = 0;
                        break;
                    case 3:
                        x = 0;
                        y = 0;
                        break;
                    case 4:
                        x = 1;
                        y = 0;
                        break;
                    case 5:
                        x = -1;
                        y = 1;
                        break;
                }
            }
            else if (Orientation == 2) // 2
            {
                switch (cellIndex)
                {
                    case 1:
                        x = 0;
                        y = -2;
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
                        x = 0;
                        y = 1;
                        break;
                    case 5:
                        x = -1;
                        y = -1;
                        break;
                }
            }
            else if (Orientation == 3) // 3
            {
                switch (cellIndex)
                {
                    case 1:
                        x = -1;
                        y = 0;
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
                        x = 2;
                        y = 0;
                        break;
                    case 5:
                        x = 1;
                        y = -1;
                        break;
                }
            }
            else // 4
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
                        x = 0;
                        y = 1;
                        break;
                    case 4:
                        x = 0;
                        y = 2;
                        break;
                    case 5:
                        x = 1;
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
            return new MutatedI
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
