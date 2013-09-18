using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TetriNET.Common.Helpers;

namespace TetriNET.Common.Randomizer
{
    public interface IOccurancy<out T>
    {
        T Value { get; }
        int Occurancy { get; }
    }

    public class RangeRandom
    {
        private static readonly ThreadSafeSingleton<Random> Randomizer = new ThreadSafeSingleton<Random>(() => new Random());

        /// <summary>
        /// Return a random index in <paramref name="ranges"/> depending on probabilities
        /// </summary>
        /// <param name="ranges">List of probabilities for each index</param>
        /// <returns></returns>
        public static int Random(List<int> ranges)
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
        
        /// <summary>
        /// Return a random value of type T depending on occurancies found in <paramref name="occurancies"/>
        /// </summary>
        /// <typeparam name="T">value return type</typeparam>
        /// <param name="occurancies">List of occurancies for each T value</param>
        /// <returns></returns>
        public static T Random<T>(IEnumerable<IOccurancy<T>> occurancies)
        {
            var list = occurancies as IList<IOccurancy<T>> ?? occurancies.ToList();

            int sum = list.Aggregate(0, (n, i) => n + i.Occurancy);
            int random = Randomizer.Instance.Next(sum);

            int range = 0;
            foreach (IOccurancy<T> occurancy in list)
            {
                range += occurancy.Occurancy;
                if (random < range)
                    return occurancy.Value;
            }
            Debug.Assert(false, "RangeRandom");
            return default(T);
        }

        /// <summary>
        /// Return sum of occurancy found in <paramref name="occurancies"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="occurancies">List of occurancies for each T value</param>
        /// <returns></returns>
        public static int SumOccurancies<T>(IEnumerable<IOccurancy<T>> occurancies)
        {
            return occurancies.Aggregate(0, (n, i) => n + i.Occurancy);
        }
    }
}
