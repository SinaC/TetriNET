using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Pieces.Mutated
{
    public class MutatedO : Piece
    {
        protected MutatedO()
        {
        }

        public MutatedO(int posX, int posY, int orientation, int index)
            : base(posX, posY, orientation, index)
        {
            Value = Common.DataContracts.Pieces.TetriminoO;
        }

        public override int MaxOrientations
        {
            get { return 4; }
        }

        public override int TotalCells
        {
            get { return 5; } // 8
        }

        public override void GetCellAbsolutePosition(int cellIndex, out int x, out int y)
        {
            x = y = 0;
            // orientation 1 : (-1, -1),  ( 0, -1),  ( 0,  0),  (-1,  0), ( 1, -1)
            // orientation 2 : ( 1, -1),  ( 1,  0),  ( 0,  0),  ( 0, -1), ( 1,  1)
            // orientation 3 : ( 1,  1),  ( 0,  1),  ( 0,  0),  ( 1,  0), (-1,  1)
            // orientation 4 : (-1,  1),  (-1,  0),  ( 0,  0),  ( 0,  1), (-1, -1)
            switch (Orientation)
            {
                case 1:
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
                        case 5:
                            x = 1;
                            y = -1;
                            break;
                    }
                    break;
                case 2:
                    switch(cellIndex)
                    {
                        case 1:
                            x = 1;
                            y = -1;
                            break;
                        case 2:
                            x = 1;
                            y = 0;
                            break;
                        case 3:
                            x = 0;
                            y = 0;
                            break;
                        case 4:
                            x = 0;
                            y = -1;
                            break;
                        case 5:
                            x = 1;
                            y = 1;
                            break;
                    }
                    break;
                case 3:
                    switch (cellIndex)
                    {
                        case 1:
                            x = 1;
                            y = 1;
                            break;
                        case 2:
                            x = 0;
                            y = 1;
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
                    break;
                case 4:
                    switch (cellIndex)
                    {
                        case 1:
                            x = -1;
                            y = 1;
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
                            x = 0;
                            y = 1;
                            break;
                        case 5:
                            x = -1;
                            y = -1;
                            break;
                    }
                    break;
            }
            // Translate to board coordinates
            x += PosX;
            y += PosY;
        }

        public override IPiece Clone()
        {
            return new MutatedO
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
