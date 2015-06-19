namespace ConsoleApplication5
{
    using System.Collections.Generic;
    using System.Linq;

    public static class PermutationsHelper
    {
        public static IEnumerable<int[]> GetPermutations(int[] values)
        {
            if (values.Length == 1)
            {
                yield return values;
            }
            else
            {
                foreach (var value in values)
                {
                    foreach (var permutations in GetPermutations(values.Where(v => v != value).ToArray()))
                    {
                        yield return new[] { value }.Concat(permutations).ToArray();
                    }
                }
            }
        }
    }
}