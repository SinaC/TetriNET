using TetriNET.Common.Interfaces;

namespace TetriNET.Strategy
{
    public interface IMoveStrategy
    {
        bool GetBestMove(IBoard board, ITetrimino current, ITetrimino next, out int bestRotationDelta, out int bestTranslationDelta, out bool rotationBeforeTranslation);
    }
}
