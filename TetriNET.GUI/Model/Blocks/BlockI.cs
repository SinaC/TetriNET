using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Tetris.Model.Blocks
{
    public class BlockI : Block
    {
        public BlockI(List<Part> grid)
            : base(grid)
        {
            Color = Colors.Brown;
            Parts = new List<Part>
                {
                    new Part(this, 0, 1),
                    new Part(this, 1, 1),
                    new Part(this, 2, 1),
                    new Part(this, 3, 1)
                };
        }

        /// <summary>
        /// Rotates the Block if its possible. Otherwise it returns false.
        /// </summary>
        /// <returns>True/False whether or not the Block was rotated.</returns>
        public override bool Rotate()
        {
            //The BlockI has two fixed states instead of just rotating counterclockwise

            #region Change based on whats the current state

            //Part at 1,0 means vertical
            if (Parts.Any(p => p.PosXInBlock == 1 && p.PosYInBlock == 0))
            {
                #region Confirm that there arent any conflicts

                for (int i = 0; i < 4; i++)
                {
                    if (!Parts[i].CheckConflict(PosX + i, PosY + 1))
                        return false;
                }

                #endregion

                #region Rearrange all parts

                for (int i = 0; i < 4; i++)
                {
                    Parts[i].RearrangePart(i, 1);
                }

                #endregion

            }
                //Part at 0,1 means horizontal
            else
            {
                #region Confirm that there arent any conflicts

                for (int i = 0; i < 4; i++)
                {
                    if (!Parts[i].CheckConflict(PosX + 1, PosY + i))
                        return false;
                }

                #endregion

                #region Rearrange all parts

                for (int i = 0; i < 4; i++)
                {
                    Parts[i].RearrangePart(1, i);
                }

                #endregion
            }

            #endregion

            return true;
        }
    }
}
