﻿using TetriNET.Common.Interfaces;

namespace TetriNET.Client.DefaultBoardAndTetriminos
{
    public class TetriminoO : Tetrimino
    {
        protected TetriminoO()
        {
        }

        public TetriminoO(int spawnX, int spawnY, int spawnOrientation) : base(spawnX, spawnY, spawnOrientation)
        {
            Value = 4;
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

        public override ITetrimino Clone()
        {
            return new TetriminoO
            {
                PosX = PosX,
                PosY = PosY,
                Orientation = Orientation,
                Value = Value
            };
        }
    }
}