using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.DefaultBoardAndPieces;

namespace TetriNET.WPF_WCF_Client.Models.MutatedPieces
{
    public class MutatedS : Piece
    {
        protected MutatedS()
        {
        }

        public MutatedS(int posX, int posY, int orientation, int index)
            : base(posX, posY, orientation, index)
        {
            Value = Pieces.MutatedS;
        }

        public override int MaxOrientations
        {
            get { return 2; }
        }

        public override int TotalCells
        {
            get { return 5; }
        }

        public override void GetCellAbsolutePosition(int cellIndex, out int x, out int y)
        {
            x = y = 0;
            // orientation 1,3: (-1, -1),  ( 0, -1),  ( 0,  0),  ( 0,  1),  ( 1,  1)
            // orientation 2,4: ( 1, -1),  ( 0,  0),  ( 1,  0),  ( -1, 0),  (-1,  1)
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
                        x = 0;
                        y = 1;
                        break;
                    case 5:
                        x = 1;
                        y = 1;
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
                        x = -1;
                        y = 0;
                        break;
                    case 5:
                        x = -1;
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
            return new MutatedS
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
