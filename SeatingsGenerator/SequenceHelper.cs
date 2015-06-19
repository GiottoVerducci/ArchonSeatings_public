namespace ConsoleApplication5
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class SequenceHelper
    {
        public static Dictionary<int, int> Count = new Dictionary<int, int>();

        public static bool AreEquivalent<T>(IList<T> first, IList<T> second, Func<T, T, bool> areEqualDelegate)
        {
            var count = first.Count;
            if (count != second.Count)
            {
                return false;
            }

            //int c;
            //Count.TryGetValue(count, out c);
            //Count[count] = ++c;

            if (count == 3)
            {
                if (areEqualDelegate(first[0], second[0]))
                {
                    return areEqualDelegate(first[1], second[1]) && areEqualDelegate(first[2], second[2])
                        || areEqualDelegate(first[1], second[2]) && areEqualDelegate(first[2], second[1]);
                }
                if (areEqualDelegate(first[0], second[1]))
                {
                    return areEqualDelegate(first[1], second[0]) && areEqualDelegate(first[2], second[2])
                        || areEqualDelegate(first[1], second[2]) && areEqualDelegate(first[2], second[0]);
                }
                return areEqualDelegate(first[0], second[2])
                    && (areEqualDelegate(first[1], second[0]) && areEqualDelegate(first[2], second[1])
                        || areEqualDelegate(first[1], second[1]) && areEqualDelegate(first[2], second[0]));
            }
            if (count == 2)
            {
                return areEqualDelegate(first[0], second[0]) && areEqualDelegate(first[1], second[1])
                    || areEqualDelegate(first[0], second[1]) && areEqualDelegate(first[1], second[0]);
            }
            if (count == 1)
            {
                return areEqualDelegate(first[0], second[0]);
            }
            if (count == 0)
            {
                return true;
            }

            var y = second.ToList();
            foreach (var x in first)
            {
                var found = false;
                for (int i = 0; i < y.Count; ++i)
                {
                    if (areEqualDelegate(x, y[i]))
                    {
                        y.RemoveAt(i);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }
            return !y.Any();
        }

        public static T[] ParallelDistinct<T>(ICollection<T> sequence, IEqualityComparer<T> comparer)
            where T : class
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
                
            var splitCount = sequence.Count / options.MaxDegreeOfParallelism;

            var splits = new List<ILookup<int, T>>();
            var offset = 0;
            for(int i = 0; i < options.MaxDegreeOfParallelism - 1; ++i)
            {
                splits.Add(sequence.Skip(offset).Take(splitCount).ToArray().ToLookup(comparer.GetHashCode));
                offset += splitCount;
            }
            splits.Add(sequence.Skip(offset).Take(sequence.Count - offset).ToArray().ToLookup(comparer.GetHashCode));

            Dictionary<int, List<T>>[] distinct = splits
                .AsParallel()
                .Select(split => split
                    .Select(itemsWithSameHash => new KeyValuePair<int, List<T>>(itemsWithSameHash.Key, itemsWithSameHash.Distinct(comparer).ToList()))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                .ToArray();
            
            for(int i = 0; i < distinct.Length - 1; ++i)
            {
                Dictionary<int, List<T>> split = distinct[i];
                foreach (var kvp in split)
                {
                    for (int j = i + 1; j < distinct.Length; ++j)
                    {
                        Dictionary<int, List<T>> splitScanned = distinct[j];

                        List<T> list;
                        if (splitScanned.TryGetValue(kvp.Key, out list))
                        {
                            list.RemoveAll(item => kvp.Value.Contains(item, comparer));
                        }
                    }
                }
            }

            return distinct.SelectMany(dictionaries => dictionaries.Values).SelectMany(v => v).ToArray();
        }
    }
}