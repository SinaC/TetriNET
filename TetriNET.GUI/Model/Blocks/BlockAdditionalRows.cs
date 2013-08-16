using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Tetris.Model.Blocks
{
    public class BlockAdditionalRows : Block
    {
        private static readonly Color[] AvailableColors =
            {
                Colors.Brown, // I
                Colors.Red, // J
                Colors.Green, // L
                Colors.HotPink, // O
                Colors.Gray, // S
                Colors.LimeGreen, // T
                Colors.Blue, // Z
            };

        private static Random _random;

        public BlockAdditionalRows(List<Part> grid, int rows = 1)
            : base(grid)
        {
            Color = Colors.CornflowerBlue; // just in case

            _random = _random ?? new Random();

            // Create block of 'rows' row with a random hole on each row
            Parts = new List<Part>();
            for (int i = 0; i < rows; i++)
            {
                int hole = _random.Next(10);
                Parts.AddRange(Enumerable.Range(0, 10)
                                   .Where(x => x != hole)
                                   .Select(
                                       x => new Part(this, x, i)
                                           {
                                               Color = AvailableColors[_random.Next(AvailableColors.Length)]
                                           }
                                   ));
            }
        }
    }
}
