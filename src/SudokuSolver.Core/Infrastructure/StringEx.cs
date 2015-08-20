using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public static class BclEx
    {
        public static string JoinStrings<T>(this IEnumerable<T> source,
                                                Func<T, string> projection, string separator)
        {
            var builder = new StringBuilder();
            bool first = true;
            foreach (T element in source)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(separator);
                }
                builder.Append(projection(element));
            }
            return builder.ToString();
        }

        public static string JoinStrings<T>(this IEnumerable<T> source, string separator)
        {
            return JoinStrings(source, t => t.ToString(), separator);
        }

        public static bool AreEqualSequences<T>(List<T> sequence1, List<T> sequence2) where T : IComparable
        {
            if (sequence1.Count != sequence2.Count)
                return false;

            for (int counter = 0; counter < sequence1.Count; counter++)
            {
                if (!sequence1[counter].Equals(sequence2[counter]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
