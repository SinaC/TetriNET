using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.DefaultBoardAndPieces;

namespace TetriNET.WPF_WCF_Client.Models.MutatedPieces
{
    public static class MutatedPieceGenerator
    {
        public static IPiece Generate(Pieces piece, int spawnX, int spawnY, int spawnOrientation, int index)
        {
            switch (piece)
            {
                case Pieces.TetriminoI:
                case Pieces.TetriminoJ:
                case Pieces.TetriminoL:
                case Pieces.TetriminoO:
                case Pieces.TetriminoS:
                case Pieces.TetriminoT:
                case Pieces.TetriminoZ:
                    return Piece.CreatePiece(piece, spawnX, spawnY, spawnOrientation, index);
                //case Pieces.TetriminoI:
                case Pieces.MutatedI:
                    return new MutatedI(spawnX, spawnY, spawnOrientation, index);
                //case Pieces.TetriminoJ:
                case Pieces.MutatedJ:
                    return new MutatedJ(spawnX, spawnY, spawnOrientation, index);
                //case Pieces.TetriminoL:
                case Pieces.MutatedL:
                    return new MutatedL(spawnX, spawnY, spawnOrientation, index);
                //case Pieces.TetriminoO:
                case Pieces.MutatedO:
                    return new MutatedO(spawnX, spawnY, spawnOrientation, index);
                //case Pieces.TetriminoS:
                case Pieces.MutatedS:
                    return new MutatedS(spawnX, spawnY, spawnOrientation, index);
                //case Pieces.TetriminoT:
                case Pieces.MutatedT:
                    return new MutatedT(spawnX, spawnY, spawnOrientation, index);
                //case Pieces.TetriminoZ:
                case Pieces.MutatedZ:
                    return new MutatedZ(spawnX, spawnY, spawnOrientation, index);
                //
                case Pieces.Invalid:
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Warning, "Create random cell because server didn't send next cell");
                    return Piece.CreatePiece(Pieces.TetriminoZ, spawnX, spawnY, spawnOrientation, index);// TODO: sometimes server takes time to send next cell, it should send 2 or 3 next pieces to ensure this never happens
            }
            Logger.Log.WriteLine(Logger.Log.LogLevels.Warning, "Unknow piece {0}", piece);
            return Piece.CreatePiece(Pieces.TetriminoZ, spawnX, spawnY, spawnOrientation, index);
        }
    }
}
