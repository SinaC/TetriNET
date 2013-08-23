using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetriNET.Client.Blocks
{
    public class BlockT : Block
    {
        public BlockT()
        {
            Parts = new byte[]
                {
                    0, 1, 0, 0,
                    1, 1, 1, 0,
                    0, 0, 0, 0,
                    0, 0, 0, 0,
                };
        }
    }
}
