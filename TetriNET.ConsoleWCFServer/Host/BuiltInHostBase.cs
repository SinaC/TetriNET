using TetriNET.Server.HostBase;
using TetriNET.Server.Interfaces;

namespace TetriNET.ConsoleWCFServer.Host
{
    public sealed class BuiltInHostBase : HostBase
    {
        public BuiltInHostBase(IPlayerManager playerManager, ISpectatorManager spectatorManager, IBanManager banManager, IFactory factory)
            : base(playerManager, spectatorManager, banManager, factory)
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
