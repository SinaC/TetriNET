using System;
using TetriNET.Common.Contracts;
using TetriNET.Common.Interfaces;
using TetriNET.Server;

namespace TetriNET.ConsoleWCFServer.Host
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

        #endregion
    }
}
