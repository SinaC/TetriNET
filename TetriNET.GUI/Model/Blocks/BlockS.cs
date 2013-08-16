using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Tetris.Model.Blocks
{
    public class BlockS : Block
    {
        public BlockS(List<Part> grid) : base(grid)
        {
            this.Color = Colors.Gray;
            Parts = new List<Part> { new Part(this, 1, 1), new Part(this, 2, 1), new Part(this, 0, 2), new Part(this, 1, 2) };
        }

        /// <summary>
        /// Rotates the Block if its possible. Otherwise it returns false.
        /// </summary>
        /// <returns>True/False whether or not the Block was rotated.</returns>
        public override bool Rotate()
        {
            #region Determine which state the block currently is in and rotate either clock- or counterclockwise
                if (Parts.Any(p => p.PosXInBlock == 1 && p.PosYInBlock == 0))
                    return base.RotateClockwise();
                else
                    return base.Rotate();
            #endregion
        }
    }
}
