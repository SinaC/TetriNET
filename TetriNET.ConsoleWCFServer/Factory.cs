using TetriNET.Common.BlockingActionQueue;
using TetriNET.Common.Contracts;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Randomizer;
using TetriNET.Server.BanManager;
using TetriNET.Server.Interfaces;
using TetriNET.Server.PieceProvider;
using TetriNET.Server.PlayerManager;
using TetriNET.Server.SpectatorManager;

namespace TetriNET.ConsoleWCFServer
{
    public class Factory : IFactory
    {
        public IActionQueue CreateActionQueue()
        {
            return new BlockingActionQueue();
        }

        public IBanManager CreateBanManager()
        {
            return new BanManager();
        }

        public IPlayerManager CreatePlayerManager(int maxPlayers)
        {
            return new PlayerManagerDictionaryBased(maxPlayers);
        }

        public ISpectatorManager CreateSpectatorManager(int maxSpectators)
        {
            return new SpectatorManagerDictionaryBased(maxSpectators);
        }

        public IPieceProvider CreatePieceProvider()
        {
            return new PieceBag(RangeRandom.Random, 4);
        }

        public IPlayer CreatePlayer(int id, string name, ITetriNETCallback callback)
        {
            return new Player(id, name, callback);
        }

        public ISpectator CreateSpectator(int id, string name, ITetriNETCallback callback)
        {
            return new Spectator(id, name, callback);
        }
    }
}
