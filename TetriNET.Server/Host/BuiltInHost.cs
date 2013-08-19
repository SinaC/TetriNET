using System;
using TetriNET.Common.Contracts;
using TetriNET.Server.Ban;
using TetriNET.Server.Player;

namespace TetriNET.Server.Host
{
    public sealed class BuiltInHost : GenericHost
    {
        public BuiltInHost(IPlayerManager playerManager, IBanManager banManager, Func<string, ITetriNETCallback, IPlayer> createPlayerFunc)
            : base(playerManager, banManager, createPlayerFunc)
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
            // how can we know this player belong to this host
        }

        #endregion
    }
}
