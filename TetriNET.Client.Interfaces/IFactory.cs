using TetriNET.Common.Contracts;

namespace TetriNET.Client.Interfaces
{
    // Abstract factory
    // http://stackoverflow.com/questions/1943576/is-there-a-pattern-for-initializing-objects-created-via-a-di-container/1945023#1945023
    public interface IFactory
    {
        IProxy CreatePlayerProxy(ITetriNETCallback callback, string address);
        ISpectatorProxy CreateSpectatorProxy(ITetriNETCallback callback, string address);
        IAchievementManager CreateAchievementManager();

        IPiece CreatePiece(Common.DataContracts.Pieces piece, int spawnX, int spawnY, int spawnOrientation, int index, bool isMutationActive);
        IBoard CreateBoard(int width, int height);
    }
}
