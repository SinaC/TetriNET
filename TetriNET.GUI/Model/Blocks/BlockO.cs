using System.Collections.Generic;
using System.Windows.Media;

namespace Tetris.Model.Blocks
{
    public class BlockO : Block
    {
        public BlockO(List<Part> grid) : base(grid)
        {
            Color = Colors.HotPink;
            Parts = new List<Part> { new Part(this, 1, 1), new Part(this, 2, 1), new Part(this, 1, 2), new Part(this, 2, 2) };
        }

        /// <summary>
        /// The BlockO cannot rotate at all.
        /// </summary>
        /// <returns></returns>
        public override bool Rotate()
        {
            return true;
        }
    }
}
