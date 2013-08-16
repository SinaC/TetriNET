using System.Collections.Generic;
using System.Windows.Media;

namespace Tetris.Model.Blocks
{
    public class BlockL : Block
    {
        public BlockL(List<Part> grid) : base(grid)
        {
            Color = Colors.Green;
            Parts = new List<Part> { new Part(this, 0, 1), new Part(this, 1, 1), new Part(this, 2, 1), new Part(this, 0, 2) };
        }
    }
}
