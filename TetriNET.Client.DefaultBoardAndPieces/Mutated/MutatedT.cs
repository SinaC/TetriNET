using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Pieces.Mutated
{
    public class MutatedT : Piece
    {
        protected MutatedT()
        {
        }

        public MutatedT(int posX, int posY, int orientation, int index)
            : base(posX, posY, orientation, index)
        {
            Value = Common.DataContracts.Pieces.TetriminoT;
        }
        
        public override int MaxOrientations
        {
            get { return 1; }
        }

        public override int TotalCells
        {
            get { return 5; }
        }

        public override void GetCellAbsolutePosition(int cellIndex, out int x, out int y)
        {
            x = y = 0;
            // orientation 1, 2, 3, 4: ( 0, -1),  (-1,  0),  ( 0,  0),  ( 1,  0),  ( 0,  1)
            switch (cellIndex)
            {
                case 1:
                    x = 0;
                    y = -1;
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
                    x = 0;
                    y = 1;
                    break;
            }
            // Translate to board coordinates
            x += PosX;
            y += PosY;
        }

        public override IPiece Clone()
        {
            return new MutatedT
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
