using System;
using TetriNET.Common.Interfaces;

namespace TetriNET.Common.Randomizer
{
    public class Randomizer : IRandomizer
    {
        private readonly Random _random = new Random();

        #region Singleton

        private static readonly Lazy<Randomizer> Lazy = new Lazy<Randomizer>(() => new Randomizer());

        public static Randomizer Instance => Lazy.Value;

        private Randomizer()
        {
        }

        #endregion

        public int Next()
        {
            return _random.Next();
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }
}
