#region License, Terms and Author(s)
//
// BackLINQ
// Copyright (c) 2008 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the New BSD License, a copy of which should have 
// been delivered along with this distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
#endregion

namespace BackLinq
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Provides LINQ Standard Query Operators as plain static 
    /// (non-extension) methods for compilation and consumption
    /// within C# 2.0 sources.
    /// </summary>

    public static partial class Enumerable
    {
        public static IEnumerable<TResult> Cast<TResult>(
            IEnumerable source
        )
        {
            CheckNotNull(source, "source");

            foreach (var item in source)
                yield return (TResult) item;
        }

        /// <summary>
        /// Generates a sequence of integral numbers within a specified range.
        /// </summary>
        /// <param name="start">The value of the first integer in the sequence.</param>
        /// <param name="count">The number of sequential integers to generate.</param>

        public static IEnumerable<int> Range(int start, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", count, null);

            var end = (long) start + count;
            if (end - 1 >= int.MaxValue)
                throw new ArgumentOutOfRangeException("count", count, null);

            for (var i = start; i < end; i++)
                yield return i;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>

        public static IEnumerable<TSource> Where<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            CheckNotNull(source, "source");
            CheckNotNull(predicate, "predicate");
            
            foreach (var item in source)
                if (predicate(item))
                    yield return item;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate. 
        /// Each element's index is used in the logic of the predicate function.
        /// </summary>

        public static IEnumerable<TSource> Where<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, int, bool> predicate)
        {
            CheckNotNull(source, "source");
            CheckNotNull(predicate, "predicate");

            var i = 0;
            foreach (var item in source)
                if (predicate(item, i++))
                    yield return item;
        }

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>

        public static IEnumerable<TResult> Select<TSource, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            CheckNotNull(source, "source");
            CheckNotNull(selector, "selector");

            foreach (var item in source)
                yield return selector(item);
        }

        /// <summary>
        /// Projects each element of a sequence into a new form by 
        /// incorporating the element's index.
        /// </summary>

        public static IEnumerable<TResult> Select<TSource, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, int, TResult> selector)
        {
            CheckNotNull(source, "source");
            CheckNotNull(selector, "selector");
            
            var i = 0;
            foreach (var item in source)
                yield return selector(item, i++);
        }

        /// <summary>
        /// Returns elements from a sequence as long as a specified condition is true.
        /// </summary>

        public static IEnumerable<TSource> TakeWhile<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            CheckNotNull(source, "source");
            CheckNotNull(predicate, "predicate");
            
            foreach (var item in source)
                if (predicate(item))
                    yield return item;
                else
                    break;
        }

        /// <summary>
        /// Returns elements from a sequence as long as a specified condition is true.
        /// The element's index is used in the logic of the predicate function.
        /// </summary>

        public static IEnumerable<TSource> TakeWhile<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, int, bool> predicate)
        {
            CheckNotNull(source, "source");
            CheckNotNull(predicate, "predicate");

            var i = 0;
            foreach (var item in source)
                if (predicate(item, i++))
                    yield return item;
                else
                    break;
        }

        /// <summary>
        /// Returns the first element of a sequence.
        /// </summary>

        public static TSource First<TSource>(
            IEnumerable<TSource> source)
        {
            CheckNotNull(source, "source");

            var list = source as IList<TSource>;    // optimized case for lists
            if (list != null)
            {
                if (list.Count == 0)
                    throw new InvalidOperationException();

                return list[0];
            }

            using (var e = source.GetEnumerator())  // fallback for enumeration
            {
                if (!e.MoveNext())
                    throw new InvalidOperationException();

                return e.Current;
            }
        }

        /// <summary>
        /// Returns the first element in a sequence that satisfies a specified condition.
        /// </summary>

        public static TSource First<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            return First(Where(source, predicate));
        }

        /// <summary>
        /// Returns the first element of a sequence, or a default value if 
        /// the sequence contains no elements.
        /// </summary>

        public static TSource FirstOrDefault<TSource>(
            IEnumerable<TSource> source)
        {
            CheckNotNull(source, "source");

            using (var e = source.GetEnumerator())
                return !e.MoveNext() ? default(TSource) : e.Current;
        }

        /// <summary>
        /// Returns the first element of the sequence that satisfies a 
        /// condition or a default value if no such element is found.
        /// </summary>

        public static TSource FirstOrDefault<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            return FirstOrDefault(Where(source, predicate));
        }

        /// <summary>
        /// Inverts the order of the elements in a sequence.
        /// </summary>
 
        public static IEnumerable<TSource> Reverse<TSource>(
            IEnumerable<TSource> source)
        {
            CheckNotNull(source, "source");

            var stack = new Stack<TSource>();
            foreach (var item in source)
                stack.Push(item);

            return stack;
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the start 
        /// of a sequence.
        /// </summary>

        public static IEnumerable<TSource> Take<TSource>(
            IEnumerable<TSource> source,
            int count)
        {
            CheckNotNull(source, "source");

            using (var e = source.GetEnumerator())
            {
                for (var i = 0; i < count && e.MoveNext(); i++)
                    yield return e.Current;
            }
        }

        /// <summary>
        /// Bypasses a specified number of elements in a sequence and then 
        /// returns the remaining elements.
        /// </summary>

        public static IEnumerable<TSource> Skip<TSource>(
            IEnumerable<TSource> source,
            int count)
        {
            CheckNotNull(source, "source");

            using (var e = source.GetEnumerator())
            {
                for (var i = 0; e.MoveNext(); i++)
                    if (i >= count) yield return e.Current;
            }
        }

        /// <summary>
        /// Returns the number of elements in a sequence.
        /// </summary>

        public static int Count<TSource>(
            IEnumerable<TSource> source)
        {
            CheckNotNull(source, "source");

            var collection = source as ICollection;
            if (collection != null)
                return collection.Count;

            checked
            {
                var count = 0;
                
                using (var e = source.GetEnumerator())
                {
                    while (e.MoveNext())
                        count++;
                }

                return count;
            }
        }

        /// <summary>
        /// Returns a number that represents how many elements in the 
        /// specified sequence satisfy a condition.
        /// </summary>

        public static int Count<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            return Count(Where(source, predicate));
        }

        /// <summary>
        /// Returns an <see cref="Int64"/> that represents the total number 
        /// of elements in a sequence.
        /// </summary>

        public static long LongCount<TSource>(
            IEnumerable<TSource> source)
        {
            CheckNotNull(source, "source");

            var array = source as Array;
            if (array != null)
                return array.LongLength;

            var count = 0L;
            
            using (var e = source.GetEnumerator())
            {
                while (e.MoveNext())
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Returns an <see cref="Int64"/> that represents how many elements 
        /// in a sequence satisfy a condition.
        /// </summary>

        public static long LongCount<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            return LongCount(Where(source, predicate));
        }

        /// <summary>
        /// Concatenates two sequences.
        /// </summary>

        public static IEnumerable<TSource> Concat<TSource>(
            IEnumerable<TSource> first,
            IEnumerable<TSource> second)
        {
            CheckNotNull(first, "first");
            CheckNotNull(second, "second");

            foreach (var item in first)
                yield return item;

            foreach (var item in second)
                yield return item;
        }

        /// <summary>
        /// Creates a <see cref="List{T}"/> from an <see cref="IEnumerable{T}"/>.
        /// </summary>

        public static List<TSource> ToList<TSource>(
            IEnumerable<TSource> source)
        {
            CheckNotNull(source, "source");

            var list = new List<TSource>();
            foreach (var item in source)
                list.Add(item);

            return list;
        }

        /// <summary>
        /// Creates an array from an <see cref="IEnumerable{T}"/>.
        /// </summary>

        public static TSource[] ToArray<TSource>(
            IEnumerable<TSource> source)
        {
            return ToList(source).ToArray();
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using the default 
        /// equality comparer to compare values.
        /// </summary>

        public static IEnumerable<TSource> Distinct<TSource>(
            IEnumerable<TSource> source)
        {
            return Distinct(source, /* comparer */ null);
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using a specified 
        /// <see cref="IEqualityComparer{T}"/> to compare values.
        /// </summary>

        public static IEnumerable<TSource> Distinct<TSource>(
            IEnumerable<TSource> source,
            IEqualityComparer<TSource> comparer)
        {
            CheckNotNull(source, "source");

            var set = new Dictionary<TSource, object>(comparer);

            foreach (var item in source)
            {
                if (set.ContainsKey(item)) 
                    continue;
                
                set.Add(item, null);
                yield return item;
            }
        }

        /// <summary>
        /// Creates a <see cref="Lookup{TKey,TElement}" /> from an 
        /// <see cref="IEnumerable{T}" /> according to a specified key 
        /// selector function.
        /// </summary>

        public static Lookup<TKey, TSource> ToLookup<TSource, TKey>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return ToLookup(source, keySelector, e => e, /* comparer */ null);
        }

        /// <summary>
        /// Creates a <see cref="Lookup{TKey,TElement}" /> from an 
        /// <see cref="IEnumerable{T}" /> according to a specified key 
        /// selector function and a key comparer.
        /// </summary>

        public static Lookup<TKey, TSource> ToLookup<TSource, TKey>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            return ToLookup(source, keySelector, e => e, comparer);
        }

        /// <summary>
        /// Creates a <see cref="Lookup{TKey,TElement}" /> from an 
        /// <see cref="IEnumerable{T}" /> according to specified key 
        /// and element selector functions.
        /// </summary>

        public static Lookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            return ToLookup(source, keySelector, elementSelector, /* comparer */ null);
        }

        /// <summary>
        /// Creates a <see cref="Lookup{TKey,TElement}" /> from an 
        /// <see cref="IEnumerable{T}" /> according to a specified key 
        /// selector function, a comparer and an element selector function.
        /// </summary>

        public static Lookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            CheckNotNull(source, "source");
            CheckNotNull(keySelector, "keySelector");
            CheckNotNull(elementSelector, "elementSelector");

            var lookup = new Lookup<TKey, TElement>(comparer);
            
            foreach (var item in source)
            {
                var key = keySelector(item);

                var grouping = (Grouping<TKey, TElement>) lookup.Find(key);
                if (grouping == null)
                {
                    grouping = new Grouping<TKey, TElement>(key);
                    lookup.Add(grouping);
                }

                grouping.Add(elementSelector(item));
            }

            return lookup;
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key 
        /// selector function.
        /// </summary>

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return GroupBy(source, keySelector, /* comparer */ null);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key 
        /// selector function and compares the keys by using a specified 
        /// comparer.
        /// </summary>

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            return GroupBy(source, keySelector, e => e, comparer);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key 
        /// selector function and projects the elements for each group by 
        /// using a specified function.
        /// </summary>

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            return GroupBy(source, keySelector, elementSelector, /* comparer */ null);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key 
        /// selector function and creates a result value from each group and 
        /// its key.
        /// </summary>

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            CheckNotNull(source, "source");
            CheckNotNull(keySelector, "keySelector");
            CheckNotNull(elementSelector, "elementSelector");

            return ToLookup(source, keySelector, elementSelector, comparer);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a key selector 
        /// function. The keys are compared by using a comparer and each 
        /// group's elements are projected by using a specified function.
        /// </summary>

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
        {
            return GroupBy(source, keySelector, resultSelector, /* comparer */ null);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key 
        /// selector function and creates a result value from each group and 
        /// its key. The elements of each group are projected by using a 
        /// specified function.
        /// </summary>

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            CheckNotNull(source, "source");
            CheckNotNull(keySelector, "keySelector");
            CheckNotNull(resultSelector, "resultSelector");

            return Query.From(ToLookup(source, keySelector, comparer))
                        .Select(g => resultSelector(g.Key, g));
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key 
        /// selector function and creates a result value from each group and 
        /// its key. The keys are compared by using a specified comparer.
        /// </summary>

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        {
            return GroupBy(source, keySelector, elementSelector, resultSelector, /* comparer */ null);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key 
        /// selector function and creates a result value from each group and 
        /// its key. Key values are compared by using a specified comparer, 
        /// and the elements of each group are projected by using a 
        /// specified function.
        /// </summary>

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            CheckNotNull(source, "source");
            CheckNotNull(keySelector, "keySelector");
            CheckNotNull(elementSelector, "elementSelector");
            CheckNotNull(resultSelector, "resultSelector");

            return Query.From(ToLookup(source, keySelector, elementSelector, comparer))
                        .Select(g => resultSelector(g.Key, g));
        }

        /// <summary>
        /// Applies an accumulator function over a sequence.
        /// </summary>

        public static TSource Aggregate<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, TSource, TSource> func)
        {
            return Aggregate(source, First(source), func);
        }

        /// <summary>
        /// Applies an accumulator function over a sequence. The specified 
        /// seed value is used as the initial accumulator value.
        /// </summary>

        public static TAccumulate Aggregate<TSource, TAccumulate>(
            IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func)
        {
            return Aggregate(source, seed, func, r => r);
        }

        /// <summary>
        /// Applies an accumulator function over a sequence. The specified 
        /// seed value is used as the initial accumulator value, and the 
        /// specified function is used to select the result value.
        /// </summary>

        public static TResult Aggregate<TSource, TAccumulate, TResult>(
            IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
            Func<TAccumulate, TResult> resultSelector)
        {
            CheckNotNull(source, "source");
            CheckNotNull(func, "func");
            CheckNotNull(resultSelector, "resultSelector");

            var result = seed;

            foreach (var item in source)
                result = func(result, item);

            return resultSelector(result);
        }

        /// <summary>
        /// Produces the set union of two sequences by using the default 
        /// equality comparer.
        /// </summary>

        public static IEnumerable<TSource> Union<TSource>(
            IEnumerable<TSource> first,
            IEnumerable<TSource> second)
        {
            return Union(first, second, /* comparer */ null);
        }

        /// <summary>
        /// Produces the set union of two sequences by using a specified 
        /// <see cref="IEqualityComparer{T}" />.
        /// </summary>

        public static IEnumerable<TSource> Union<TSource>(
            IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer)
        {
            CheckNotNull(first, "first");
            CheckNotNull(second, "second");

            return Query.From(first).Concat(second).Distinct(comparer);
        }

        private static void CheckNotNull<T>(T value, string name) where T : class
        {
            if (value == null) 
                throw new ArgumentNullException(name);
        }

        private sealed class Grouping<K, V> : List<V>, IGrouping<K, V>
        {
            internal Grouping(K key)
            {
                Key = key;
            }

            public K Key { get; private set; }
        }
    }
}
