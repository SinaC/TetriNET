using TetriNET.Common.Interfaces;

namespace TetriNET.Strategy
{
    public interface IMoveStrategy
    {
        bool GetBestMove(IBoard board, IPiece current, IPiece next, out int bestRotationDelta, out int bestTranslationDelta, out bool rotationBeforeTranslation);
    }
}
