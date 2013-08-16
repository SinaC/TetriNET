using System.Collections.Generic;
using System.Windows.Media;

namespace Tetris.Model.Blocks
{
    public class BlockT : Block
    {
        public BlockT(List<Part> grid) : base(grid)
        {
            this.Color = Colors.LimeGreen;
            Parts = new List<Part> { new Part(this, 1, 0), new Part(this, 0, 1), new Part(this, 1, 1), new Part(this, 2, 1) };
        }
    }
}
