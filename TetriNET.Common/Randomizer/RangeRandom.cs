using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TetriNET.Common.Interfaces;

namespace TetriNET.Common.Randomizer
{
    public static class RangeRandom
    {
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
        
        public static T Random<T>(IEnumerable<IOccurancy<T>> occurancies, IEnumerable<T> history)
        {
            var list = (occurancies as IList<IOccurancy<T>> ?? occurancies.ToList()).Where(x => !history.Contains(x.Value));

            return Random(list);
        }

        public static int SumOccurancies<T>(IEnumerable<IOccurancy<T>> occurancies)
        {
            return occurancies.Aggregate(0, (n, i) => n + i.Occurancy);
        }
    }
}
