using System;
using TetriNET.Common;

namespace TetriNET.Server
{
    public sealed class BuiltInHost : GenericHost
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

        public override void RemovePlayer(IPlayer player)
        {
            // NOP
        }

        public override void BanPlayer(IPlayer player)
        {
            // NOP
        }

        #endregion
    }
}
