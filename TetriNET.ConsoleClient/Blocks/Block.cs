using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetriNET.Client.Blocks
{
    public abstract class Block
    {
        public byte[] Parts = new byte[16]; // 4x4 grid

        public int PosX { get; set; }
        public int PosY { get; set; }

        public bool RotateClockwise(byte[] grid, int width, int height)
        {
            return Rotate(90, grid, width, height);
        }

        public bool RotateCounterClockwise(byte[] grid, int width, int height)
        {
            return Rotate(-90, grid, width, height);
        }

        protected bool Rotate(int degrees, byte[] grid, int width, int height)
        {
            byte[] newBlockParts = new byte[16];

            // Rotation center
            const int zx = 1;
            const int zy = 1;

            // Degrees to radians
            double radians = (Math.PI * degrees / 180);

            // Rotate each part
            for(int i = 0; i < 16; i++)
                if (Parts[i] > 0)
                {
                    // Get part coordinates
                    int partX = i%4;
                    int partY = i/4;

                    // Reduce the coordinates to a 0,0 center
                    int x = partX - zx;
                    int y = partY - zy;

                    // Compute rotation + transpose in part coordinates
                    int newX = zx + (int)(Math.Cos(radians)*x - Math.Sin(radians)*y);
                    int newY = zy + (int)(Math.Sin(radians)*x + Math.Cos(radians)*y);

                    // Transpose new part coordinates to grid coordinates and check conflict
                    int gridX = newX + PosX;
                    int gridY = newY + PosY;
                    int linearGridCoordinate = gridY * width + gridX;
                    if (grid[linearGridCoordinate] > 0)
                    {
                        Console.WriteLine("Conflict at {0},{1} part {2},{3}", gridX, gridY, partX, partY);
                        return false; // Conflict
                    }

                    // Save part in its new coordinates
                    int linearPartCoordinate = newY*4 + newX;
                    newBlockParts[linearPartCoordinate] = Parts[i];
                }
            // Rotation doesn't raise any conflict, perform it
            Parts = newBlockParts;
            return true;
        }

        public void Dump()
        {
            string coordinates = Parts.Select((x, i) => new
                {
                    value = x,
                    index = i
                }).Where(x => x.value > 0).Aggregate(String.Empty, (n, i) => n + "{" + ((i.index%4) + PosX) + "," + ((i.index/4) + PosY) + "}");
            Console.WriteLine("Global coordinates:{0}", coordinates);
            for (int i = 0; i < 16; i++)
            {
                Console.Write(Parts[i]);
                if ((i + 1)%4 == 0)
                    Console.WriteLine();
            }
        }
    }
}
