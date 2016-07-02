using System.Collections.Generic;
using System.Linq;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.Server.BanManager;
using TetriNET.Server.Interfaces;
using TetriNET.Server.PieceProvider;
using TetriNET.Server.PlayerManager;
using TetriNET.Server.SpectatorManager;

namespace TetriNET.Tests.Server.Mocking
{
    public class FactoryMock : IFactory
    {
        public IActionQueue CreateActionQueue()
        {
            return new ActionQueueMock();
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

        // Always get first available
        private Pieces PseudoRandom(IEnumerable<IOccurancy<Pieces>> occurancies, IEnumerable<Pieces> history)
        {
            var available = (occurancies as IList<IOccurancy<Pieces>> ?? occurancies.ToList()).Where(x => !history.Contains(x.Value)).ToList();
            if (available.Any())
            {
                Pieces piece = available[0].Value;
                return piece;
            }
            return Pieces.Invalid;
        }

        public IPieceProvider CreatePieceProvider()
        {
            return new PieceBag(PseudoRandom, 4);
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
