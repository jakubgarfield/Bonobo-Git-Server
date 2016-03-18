/*--------------------------------------------------------------------------
* AnonymousComparer - lambda compare selector for Linq
* ver 1.3.0.0 (Oct. 18th, 2010)
*
* created and maintained by neuecc <ils@neue.cc>
* licensed under Microsoft Public License(Ms-PL)
* http://neue.cc/
* http://linqcomparer.codeplex.com/
*--------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace System.Linq
{
    public static class AnonymousComparer
    {
        #region IComparer<T>

        /// <summary>Example:AnonymousComparer.Create&lt;int&gt;((x, y) => y - x)</summary>
        public static IComparer<T> Create<T>(Func<T, T, int> compare)
        {
            if (compare == null) throw new ArgumentNullException("compare");

            return new Comparer<T>(compare);
        }

        private class Comparer<T> : IComparer<T>
        {
            private readonly Func<T, T, int> compare;

            public Comparer(Func<T, T, int> compare)
            {
                this.compare = compare;
            }

            public int Compare(T x, T y)
            {
                return compare(x, y);
            }
        }

        #endregion

        #region IEqualityComparer<T>

        /// <summary>Example:AnonymousComparer.Create((MyClass mc) => mc.MyProperty)</summary>
        public static IEqualityComparer<T> Create<T, TKey>(Func<T, TKey> compareKeySelector)
        {
            if (compareKeySelector == null) throw new ArgumentNullException("compareKeySelector");

            return new EqualityComparer<T>(
                (x, y) =>
                {
                    if (object.ReferenceEquals(x, y)) return true;
                    if (x == null || y == null) return false;
                    return compareKeySelector(x).Equals(compareKeySelector(y));
                },
                obj =>
                {
                    if (obj == null) return 0;
                    return compareKeySelector(obj).GetHashCode();
                });
        }

        public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            if (equals == null) throw new ArgumentNullException("equals");
            if (getHashCode == null) throw new ArgumentNullException("getHashCode");

            return new EqualityComparer<T>(equals, getHashCode);
        }

        private class EqualityComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> equals;
            private readonly Func<T, int> getHashCode;

            public EqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
            {
                this.equals = equals;
                this.getHashCode = getHashCode;
            }

            public bool Equals(T x, T y)
            {
                return equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                return getHashCode(obj);
            }
        }

        #endregion

        #region Extensions for LINQ Standard Query Operators

        // IComparer<T>

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> compare)
        {
            return source.OrderBy(keySelector, AnonymousComparer.Create(compare));
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> compare)
        {
            return source.OrderByDescending(keySelector, AnonymousComparer.Create(compare));
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> compare)
        {
            return source.ThenBy(keySelector, AnonymousComparer.Create(compare));
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> compare)
        {
            return source.ThenByDescending(keySelector, AnonymousComparer.Create(compare));
        }

        // IEqualityComparer<T>

        public static bool Contains<TSource, TCompareKey>(this IEnumerable<TSource> source, TSource value, Func<TSource, TCompareKey> compareKeySelector)
        {
            return source.Contains(value, AnonymousComparer.Create(compareKeySelector));
        }

        public static IEnumerable<TSource> Distinct<TSource, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TCompareKey> compareKeySelector)
        {
            return source.Distinct(AnonymousComparer.Create(compareKeySelector));
        }

        public static IEnumerable<TSource> Except<TSource, TCompareKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TCompareKey> compareKeySelector)
        {
            return first.Except(second, AnonymousComparer.Create(compareKeySelector));
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, Func<TKey, TCompareKey> compareKeySelector)
        {
            return source.GroupBy(keySelector, resultSelector, AnonymousComparer.Create(compareKeySelector));
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, TCompareKey> compareKeySelector)
        {
            return source.GroupBy(keySelector, elementSelector, AnonymousComparer.Create(compareKeySelector));
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, Func<TKey, TCompareKey> compareKeySelector)
        {
            return source.GroupBy(keySelector, elementSelector, resultSelector, AnonymousComparer.Create(compareKeySelector));
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult, TCompareKey>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, Func<TKey, TCompareKey> compareKeySelector)
        {
            return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, AnonymousComparer.Create(compareKeySelector));
        }

        public static IEnumerable<TSource> Intersect<TSource, TCompareKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TCompareKey> compareKeySelector)
        {
            return first.Intersect(second, AnonymousComparer.Create(compareKeySelector));
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult, TCompareKey>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, Func<TKey, TCompareKey> compareKeySelector)
        {
            return outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector, AnonymousComparer.Create(compareKeySelector));
        }

        public static bool SequenceEqual<TSource, TCompareKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TCompareKey> compareKeySelector)
        {
            return first.SequenceEqual(second, AnonymousComparer.Create(compareKeySelector));
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, TCompareKey> compareKeySelector)
        {
            return source.ToDictionary(keySelector, elementSelector, AnonymousComparer.Create(compareKeySelector));
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement, TCompareKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, TCompareKey> compareKeySelector)
        {
            return source.ToLookup(keySelector, elementSelector, AnonymousComparer.Create(compareKeySelector));
        }

        public static IEnumerable<TSource> Union<TSource, TCompareKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TCompareKey> compareKeySelector)
        {
            return first.Union(second, AnonymousComparer.Create(compareKeySelector));
        }

        #endregion
    }
}