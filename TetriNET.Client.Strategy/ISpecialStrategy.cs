﻿using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Strategy
{
    public class SpecialAdvice
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
        bool GetSpecialAdvices(IBoard board, IPiece current, IPiece next, IEnumerable<Specials> inventory, int inventoryMaxSize, IEnumerable<IOpponent> opponents, out List<SpecialAdvice> advices);
    }
}
