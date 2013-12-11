using System;
using TetriNET.Common.Contracts;
using TetriNET.Server.GenericHost;
using TetriNET.Server.Interfaces;

namespace TetriNET.ConsoleWCFServer.Host
{
    public sealed class BuiltInHost : GenericHost
    {
        public BuiltInHost(IPlayerManager playerManager, ISpectatorManager spectatorManager, IBanManager banManager, Func<string, ITetriNETCallback, IPlayer> createPlayerFunc, Func<string, ITetriNETCallback, ISpectator> createSpectatorFunc)
            : base(playerManager, spectatorManager, banManager, createPlayerFunc, createSpectatorFunc)
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

        public override void RemoveSpectator(ISpectator spectator)
        {
            // NOP
        }

        #endregion
    }
}
