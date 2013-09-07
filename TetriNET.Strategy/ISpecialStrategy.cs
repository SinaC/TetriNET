using System.Collections.Generic;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;

namespace TetriNET.Strategy
{
    public class SpecialAdvices
    {
        public enum SpecialAdviceActions
        {
            Discard,
            UseSelf,
            UseOpponent,
            Wait
        }

        public SpecialAdviceActions SpecialAdviceAction { get; set; }
        public int OpponentId { get; set; }
    }

    public interface ISpecialStrategy
    {
        bool GetSpecialAdvice(IBoard board, ITetrimino current, ITetrimino next, List<Specials> inventory, int inventoryMaxSize, List<IOpponent> opponents, out List<SpecialAdvices> advices);
    }
}
