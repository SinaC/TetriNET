using TetriNET.Client.Achievements;
using TetriNET.Client.Board;
using TetriNET.Client.Interfaces;
using TetriNET.Client.Pieces;
using TetriNET.Client.WCFProxy;
using TetriNET.Common.Contracts;

namespace TetriNET.ConsoleWCFClient
{
    public class Factory : IFactory
    {
        public IProxy CreatePlayerProxy(ITetriNETCallback callback, string address)
        {
            return new WCFProxy(callback, address);
        }

        public ISpectatorProxy CreateSpectatorProxy(ITetriNETCallback callback, string address)
        {
            return new WCFSpectatorProxy(callback, address);
        }

        public IAchievementManager CreateAchievementManager()
        {
            return new AchievementManager();
        }

        public IPiece CreatePiece(Common.DataContracts.Pieces piece, int spawnX, int spawnY, int spawnOrientation, int index, bool isMutationActive)
        {
            return Piece.CreatePiece(piece, spawnX, spawnY, spawnOrientation, index, isMutationActive);
        }

        public IBoard CreateBoard(int width, int height)
        {
            return new BoardWithWallKick(width, height);
        }
    }
}
