using System.Linq;
using System.Windows.Media;

namespace Tetris.Model
{
    public class Part
    {
        #region Fields

        private readonly Block _parentBlock;
        private Color? _color;

        #endregion

        #region Properties
        public int PosXInBlock { get; private set; }
        public int PosYInBlock { get; private set; }

        public Block ParentBlock
        {
            get { return _parentBlock; }
        }
       

        //The public properties for PosX/PosY will return the absolute position in the Grid (based on the values of the parent block).
        public int PosX
        {
            get { return ParentBlock.PosX + PosXInBlock; }
        }

        public int PosY
        {
            get { return ParentBlock.PosY + PosYInBlock; }
        }

        public Color Color
        {
            get { return _color ?? _parentBlock.Color; }
            set { _color = value; }
        }

        #endregion

        #region Contructors

        public Part(Block parentBlock, int posx, int posy)
        {
            _parentBlock = parentBlock;
            PosXInBlock = posx;
            PosYInBlock = posy;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determine if the part can move to the new position x/y
        /// </summary>
        /// <param name="x">The new x value.</param>
        /// <param name="y">The new y value.</param>
        /// <returns>Returns if the move will be allowed.</returns>
        public bool CheckConflict(int x, int y)
        {
            /*  No move allowed if:
             *  PosX < 0
             *  PosX > 9
             *  PosY > 17
             *  Or if any other part already is in the new location
             */

            #region Check for the new position to be in the borders of the Grid

            if (x < 0 || x > 9 || y > 17)
                return false;

            #endregion

            #region Get a part which is currently in the new location and not a part of the same block

            //Note: Due to the fact the Grid is now a List instead of an Array you have to check every Part instead of a single Array Index here...
            if (!ParentBlock.Grid.Any(p => p.PosX == x && p.PosY == y && p.ParentBlock != ParentBlock))
                return true;

            #endregion

            return false;
        }

        /// <summary>
        /// Rearrange the part in its block.
        /// </summary>
        /// <param name="x">The parts new x value inside its blocks subgrid (4x4).</param>
        /// <param name="y">The parts new x value inside its blocks subgrid (4x4).</param>
        public void RearrangePart(int x, int y)
        {
            #region Check for the new values to be valid (0-3)

            if (!((x >= 0 && x < 4) && (y >= 0 && y < 4)))
                return;

            #endregion

            #region Set the new part position

            PosXInBlock = x;
            PosYInBlock = y;

            #endregion
        }

        #endregion
    }
}