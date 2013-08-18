using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TetriNET.Common.Helpers;

namespace TetriNET.Common.Randomizer
{
    public class RangeRandom
    {
        private static readonly ThreadSafeSingleton<Random> Randomizer = new ThreadSafeSingleton<Random>(() => new Random());

        /// <summary>
        /// Return a random index in <paramref name="ranges"/> depending on probabilities
        /// </summary>
        /// <param name="ranges">List of probabilities for each index</param>
        /// <returns></returns>
        private static int Random(List<int> ranges)
        {
            int sum = ranges.Aggregate((n, i) => n + i);
            int random = Randomizer.Instance.Next(sum);

            int range = 0;
            for (int i = 0; i < ranges.Count; i++)
            {
                range += ranges[i];
                if (random < range)
                    return i;
            }
            Debug.Assert(false, "RangeRandom");
            return 0;
        }
    }
}
