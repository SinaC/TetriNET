using System.Collections.Generic;
using System.Windows.Media;

namespace Tetris.Model.Blocks
{
    public class BlockJ : Block
    {
        public BlockJ(List<Part> grid) : base(grid)
        {
            Color = Colors.Red;
            Parts = new List<Part> { new Part(this, 0, 1), new Part(this, 1, 1), new Part(this, 2, 1), new Part(this, 2, 2) };
        }
    }
}
