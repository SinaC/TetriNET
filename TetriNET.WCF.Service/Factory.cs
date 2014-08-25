using TetriNET.Common.Contracts;
using TetriNET.Common.Randomizer;
using TetriNET.Server.BanManager;
using TetriNET.Server.Interfaces;
using TetriNET.Server.PieceProvider;
using TetriNET.Server.PlayerManager;
using TetriNET.Server.SpectatorManager;

namespace TetriNET.WCF.Service
{
    public class Factory : IFactory
    {
        public IBanManager CreateBanManager()
        {
            return new BanManager();
        }

        public IPlayerManager CreatePlayerManager(int maxPlayers)
        {
            return new PlayerManager(maxPlayers);
        }

        public ISpectatorManager CreateSpectatorManager(int maxSpectators)
        {
            return new SpectatorManager(maxSpectators);
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
