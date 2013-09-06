using TetriNET.Common.Interfaces;

namespace TetriNET.Strategy
{
    public abstract class MoveStrategyBase
    {
        public abstract string StrategyName { get; }
        public abstract bool GetBestMove(IBoard board, ITetrimino current, ITetrimino next, out int bestRotationDelta, out int bestTranslationDelta);
    }
}
