﻿using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Pieces.SRS
{
    internal class TetriminoL : Piece
    {
        protected TetriminoL()
        {
        }

        public TetriminoL(int spawnX, int spawnY, int spawnOrientation, int index)
            : base(spawnX, spawnY, spawnOrientation, index)
        {
            Value = Common.DataContracts.Pieces.TetriminoL;
        }

        public override int MaxOrientations => 4;

        public override int TotalCells => 4;

        public override void GetCellAbsolutePosition(int cellIndex, out int x, out int y)
        {
            x = y = 0;
            // orientation 1: ( 1, -1),  (-1,  0),  ( 0,  0),  ( 1,  0)
            // orientation 2: ( 0, -1),  ( 0,  0),  ( 0,  1),  ( 1,  1)
            // orientation 3: (-1,  0),  ( 0,  0),  ( 1,  0),  (-1,  1)
            // orientation 4: (-1, -1),  ( 0, -1),  ( 0,  0),  ( 0,  1)
            switch (Orientation)
            {
                case 1:
                    switch (cellIndex)
                    {
                        case 1:
                            x = 1;
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
                    }
                    break;
                case 2:
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
                            x = 1;
                            y = 1;
                            break;
                    }
                    break;
                case 3:
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
                    }
                    break;
            }
            // Translate to board coordinates
            x += PosX;
            y += PosY;
        }

        public override IPiece Clone()
        {
            return new TetriminoL
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
