using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonobo.Git.Server
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }

            return source.OrderByDescending(selector);
        }
    }
}