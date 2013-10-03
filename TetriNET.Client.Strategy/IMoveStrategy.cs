using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Strategy
{
    public interface IMoveStrategy
    {
        bool GetBestMove(IBoard board, IPiece current, IPiece next, out int bestRotationDelta, out int bestTranslationDelta, out bool rotationBeforeTranslation);
    }
}
