using TetriNET.Server.GenericHost;
using TetriNET.Server.Interfaces;

namespace TetriNET.Tests.Server.Mocking
{
    public class HostMock : GenericHost
    {
        public HostMock(IPlayerManager playerManager, ISpectatorManager spectatorManager, IBanManager banManager, IFactory factory)
            : base(playerManager, spectatorManager, banManager, factory, 1, 1)
        {
        }

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
    }
}
