﻿using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Pieces.Normal
{
    internal class TetriminoI : Piece
    {
        protected TetriminoI()
        {
        }

        public TetriminoI(int spawnX, int spawnY, int spawnOrientation, int index) : base(spawnX, spawnY, spawnOrientation, index)
        {
            Value = Common.DataContracts.Pieces.TetriminoI;
        }

        public override int MaxOrientations => 2;

        public override int TotalCells => 4;

        public override void GetCellAbsolutePosition(int cellIndex, out int x, out int y)
        {
            x = y = 0;
            // orientation 1,3: (-2,  0),  (-1,  0),  ( 0,  0),  ( 1,  0)
            // orientation 2,4: ( 0, -2),  ( 0, -1),  ( 0,  0),  ( 0,  1)
            if (Orientation == 1 || Orientation == 3)
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
                }
            }
            else // 2 or 4
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
                }
            }
            // Translate to board coordinates
            x += PosX;
            y += PosY;
        }

        public override IPiece Clone()
        {
            return new TetriminoI
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
