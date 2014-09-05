using TetriNET.Common.Contracts;
using TetriNET.Common.Interfaces;

namespace TetriNET.Server.Interfaces
{
    // Abstract factory
    //http://stackoverflow.com/questions/1943576/is-there-a-pattern-for-initializing-objects-created-via-a-di-container/1945023#1945023
    public interface IFactory
    {
        IActionQueue CreateActionQueue();
        IBanManager CreateBanManager();
        IPlayerManager CreatePlayerManager(int maxPlayers);
        ISpectatorManager CreateSpectatorManager(int maxSpectators);
        IPieceProvider CreatePieceProvider();

        IPlayer CreatePlayer(int id, string name, ITetriNETCallback callback);
        ISpectator CreateSpectator(int id, string name, ITetriNETCallback callback);
    }
}
