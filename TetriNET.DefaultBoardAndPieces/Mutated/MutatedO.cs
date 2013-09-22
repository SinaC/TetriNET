using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;

namespace TetriNET.DefaultBoardAndPieces.Mutated
{
    public class MutatedO : Piece
    {
        protected MutatedO()
        {
        }

        public MutatedO(int posX, int posY, int orientation, int index)
            : base(posX, posY, orientation, index)
        {
            Value = Pieces.TetriminoO;
        }

        public override int MaxOrientations
        {
            get { return 1; }
        }

        public override int TotalCells
        {
            get { return 5; } // 8
        }

        public override void GetCellAbsolutePosition(int cellIndex, out int x, out int y)
        {
            x = y = 0;
            // orientation 1,2,3,4 : (-1, -1),  ( 0, -1),  ( 0,  0),  (-1,  0),  (-2, -1)//,  ( 0, -2),  ( 1,  0),  (-1,  1)
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
                    x = -2;
                    y = -1;
                    break;
                //case 6:
                //    x = 0;
                //    y = -2;
                //    break;
                //case 7:
                //    x = 1;
                //    y = 0;
                //    break;
                //case 8:
                //    x = -1;
                //    y = 1;
                //    break;
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
