using TetriNET.Server.HostBase;
using TetriNET.Server.Interfaces;

namespace TetriNET.Tests.Server.Mocking
{
    public class HostBaseMock : HostBase
    {
        public HostBaseMock(IPlayerManager playerManager, ISpectatorManager spectatorManager, IBanManager banManager, IFactory factory)
            : base(playerManager, spectatorManager, banManager, factory)
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
