using System.Collections.Generic;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;

namespace TetriNET.Strategy
{
    public class SpecialAdvices
    {
        public enum SpecialAdviceActions
        {
            Drop,
            Use,
            Wait
        }

        public SpecialAdviceActions SpecialAdviceAction { get; set; }
        public int UseTarget { get; set; }
    }

    public class OpponentData // Almost same as PlayerData in Client
    {
        public int PlayerId { get; private set; }
        public IBoard Board { get; private set; }

        public OpponentData(int playerId, IBoard board)
        {
            PlayerId = playerId;
            Board = board;
        }
    }

    public abstract class SpecialStrategyBase
    {
        public abstract string StrategyName { get; }
        public abstract bool GetSpecialAdvice(IBoard board, ITetrimino current, ITetrimino next, List<Specials> specials, out List<SpecialAdvices> advices);
    }
}
