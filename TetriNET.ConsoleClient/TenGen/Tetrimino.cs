using System;
using System.Linq;

namespace TetriNET.Client.TenGen
{
    public abstract class Tetrimino : ITetrimino
    {
        public abstract Common.Tetriminos TetriminoValue { get; }

        public byte[] Parts {
            get
            {
                return CurrentRotation(_rotation);
            }
        }

        public int PosX { get; private set; }
        public int PosY { get; private set; }

        private int _rotation;

        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        protected Tetrimino(int width, int height, int gridWidth, int gridHeight)
        {
            Width = width;
            Height = height;
            GridWidth = gridWidth;
            GridHeight = gridHeight;

            PosX = 1; // TODO: center
            PosY = 1;

            _rotation = 0; // TODO: random
        }

        public bool CheckConflict(byte[] grid)
        {
            return CheckConflict(Parts, grid);
        }

        public bool MoveLeft(byte[] grid)
        {
            // Simulate move and check if any conflict occurs
            for (int i = 0; i < Width*Height; i++)
            {
                if (Parts[i] > 0)
                {
                    // Get part coordinates
                    int partX = i%Width;
                    int partY = i/Width;

                    // Transport coordinates into grid coordinates and perform move
                    int gridX = PosX + partX - 1;
                    int gridY = PosY + partY;
                    int linearGridCoordinate = gridY*GridWidth + gridX;

                    // Check conflict
                    if (gridX < 0 || grid[linearGridCoordinate] > 0)
                    {
                        Console.WriteLine("Conflict at {0},{1} part {2},{3}", gridX, gridY, partX, partY);
                        return false; // Conflict
                    }
                }
            }
            // Move didn't raise any conflict, perform move
            PosX--;
            return true;
        }

        public bool MoveRight(byte[] grid)
        {
            // Simulate move and check if any conflict occurs
            for (int i = 0; i < Width*Height; i++)
            {
                if (Parts[i] > 0)
                {
                    // Get part coordinates
                    int partX = i%Width;
                    int partY = i/Width;

                    // Transport coordinates into grid coordinates and perform move
                    int gridX = PosX + partX + 1;
                    int gridY = PosY + partY;
                    int linearGridCoordinate = gridY*GridWidth + gridX;

                    // Check conflict
                    if (gridX >= GridWidth || grid[linearGridCoordinate] > 0)
                    {
                        Console.WriteLine("Conflict at {0},{1} part {2},{3}", gridX, gridY, partX, partY);
                        return false; // Conflict
                    }
                }
            }
            // Move didn't raise any conflict, perform move
            PosX++;
            return true;
        }

        public bool MoveDown(byte[] grid)
        {
            // Simulate move and check if any conflict occurs
            for (int i = 0; i < Width*Height; i++)
            {
                if (Parts[i] > 0)
                {
                    // Get part coordinates
                    int partX = i%Width;
                    int partY = i/Width;

                    // Transport coordinates into grid coordinates and perform move
                    int gridX = PosX + partX;
                    int gridY = PosY + partY + 1;
                    int linearGridCoordinate = gridY*GridWidth + gridX;

                    // Check conflict
                    if (gridY >= GridHeight || grid[linearGridCoordinate] > 0)
                    {
                        Console.WriteLine("Conflict at {0},{1} part {2},{3}", gridX, gridY, partX, partY);
                        return false; // Conflict
                    }
                }
            }
            // Move didn't raise any conflict, perform move
            PosY++;
            return true;
        }

        public bool MoveUp(byte[] grid)
        {
            // Simulate move and check if any conflict occurs
            for (int i = 0; i < Width*Height; i++)
            {
                if (Parts[i] > 0)
                {
                    // Get part coordinates
                    int partX = i%Width;
                    int partY = i/Width;

                    // Transport coordinates into grid coordinates and perform move
                    int gridX = PosX + partX;
                    int gridY = PosY + partY - 1;
                    int linearGridCoordinate = gridY*GridWidth + gridX;

                    // Check conflict
                    if (gridY < 0 || grid[linearGridCoordinate] > 0)
                    {
                        Console.WriteLine("Conflict at {0},{1} part {2},{3}", gridX, gridY, partX, partY);
                        return false; // Conflict
                    }
                }
            }
            // Move didn't raise any conflict, perform move
            PosY--;
            return true;
        }

        protected abstract byte[] CurrentRotation(int rotation);

        public virtual bool RotateClockwise(byte[] grid)
        {
            int newRotation = _rotation == 3 ? 0 : _rotation+1;
            byte[] parts = CurrentRotation(newRotation);
            if (CheckConflict(parts, grid))
                return false;
            _rotation = newRotation;
            return true;
        }

        public virtual bool RotateCounterClockwise(byte[] grid)
        {
            int newRotation = _rotation == 0 ? 3 : _rotation-1;
            byte[] parts = CurrentRotation(newRotation);
            if (CheckConflict(parts, grid))
                return false;
            _rotation = newRotation;
            return true;
        }

        private bool CheckConflict(byte[] parts, byte[] grid)
        {
            for (int i = 0; i < Width * Height; i++)
                if (parts[i] > 0)
                {
                    // Get part coordinates
                    int partX = i % Width;
                    int partY = i / Width;

                    // Transport coordinates into grid coordinates
                    int gridX = PosX + partX;
                    int gridY = PosY + partY;
                    int linearGridCoordinate = gridY * GridWidth + gridX;

                    // Check conflict
                    if (gridX < 0 || gridX >= GridWidth || gridY < 0 || gridY >= GridHeight || grid[linearGridCoordinate] > 0)
                        return true;
                }
            return false;
        }

        public void Dump()
        {
            string coordinates = Parts.Select((x, i) => new
            {
                value = x,
                index = i
            }).Where(x => x.value > 0).Aggregate(String.Empty, (n, i) => n + "{" + ((i.index%Width) + PosX) + "," + ((i.index/Width) + PosY) + "}");
            Console.WriteLine("Global coordinates:{0}", coordinates);
            for (int i = 0; i < Width*Height; i++)
            {
                Console.Write(Parts[i]);
                if ((i + 1) % Width == 0)
                    Console.WriteLine();
            }
        }
    }
}