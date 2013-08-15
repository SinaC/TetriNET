using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class BuiltInHost : GenericHost
    {
        public BuiltInHost(IPlayerManager playerManager, Func<string, ITetriNETCallback, IPlayer> createPlayerFunc) : base(playerManager, createPlayerFunc)
        {
        }

        #region IHost
        public override void Start()
        {
            // NOP
        }

        public override void Stop()
        {
            // NOP
        }
        #endregion
    }
}
