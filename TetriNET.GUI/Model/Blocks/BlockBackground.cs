using System.Collections.Generic;
using System.Linq;

namespace Tetris.Model.Blocks
{
    // TODO: attribute to avoid being taken as random block
    public class BlockBackground : Block
    {
        public BlockBackground(List<Part> grid) : base(grid)
        {
            // In this block, relative and absolute position are the same
            PosX = 0;
            PosY = 18;
        }

        public override bool Rotate()
        {
            return false; // rotating background is impossible
        }

        public void Merge(Block block)
        {
            // Add each part
            foreach (Part part in block.Parts.Where(part => !Parts.Any(p => p.PosX == part.PosX && p.PosY == part.PosY)))
            {
                // TODO: rearrange part in block or create a new part
                Parts.Add(part);
            }
        }

        // TODO: 
        //  add every attacks
        //  add merge(Block) called when a block cannot be moved further down
        //      remove block from grid and add it to this
    }
}
