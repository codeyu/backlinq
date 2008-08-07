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
    /// Class that serves as a convenience starting point for LINQ queries
    /// by chaining method call without relying on extension methods.
    /// </summary>

    public static class Query
    {
        public static Query<T> From<T>(IEnumerable<T> source)
        {
            return new Query<T>(source);
        }

        public static Query<object> From(IEnumerable source)
        {
            return new Query<object>(Enumerable.Cast<object>(source));
        }

        public static Query<int> Range(int start, int count)
        {
            return new Query<int>(Enumerable.Range(start, count));
        }
    }

    /// <summary>
    /// Class that serves as a fluent interface for composing LINQ operations.
    /// </summary>

    [ Serializable ]
    public class Query<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> source;

        public Query(IEnumerable<T> source)
        {
            if (source == null) 
                throw new ArgumentNullException("source");

            this.source = source;
        }

        // TODO: Find a way to copy doc summaries from Enumerable method to corresponding ones here.

        public Query<R> Select<R>(Func<T, R> selector) { return Query.From(Enumerable.Select(source, selector)); }
        public Query<R> Select<R>(Func<T, int, R> predicate) { return Query.From(Enumerable.Select(source, predicate)); }
        public Query<T> Where(Func<T, bool> predicate) { return Query.From(Enumerable.Where(source, predicate)); }
        public Query<T> Where(Func<T, int, bool> predicate) { return Query.From(Enumerable.Where(source, predicate)); }
        public Query<T> TakeWhile(Func<T, bool> predicate) { return Query.From(Enumerable.TakeWhile(source, predicate)); }
        public Query<T> TakeWhile(Func<T, int, bool> predicate) { return Query.From(Enumerable.TakeWhile(source, predicate)); }
        public T First() { return Enumerable.First(source); }
        public T First(Func<T, bool> predicate) { return Enumerable.First(source, predicate); }
        public T FirstOrDefault() { return Enumerable.FirstOrDefault(source); }
        public T FirstOrDefault(Func<T, bool> predicate) { return Enumerable.FirstOrDefault(source, predicate); }
        public Query<T> Reverse() { return Query.From(Enumerable.Reverse(source)); }
        public Query<T> Take(int count) { return Query.From(Enumerable.Take(source, count)); }
        public Query<T> Skip(int count) { return Query.From(Enumerable.Skip(source, count)); }
        public int Count() { return Enumerable.Count(source); }
        public int Count(Func<T, bool> predicate) { return Enumerable.Count(source, predicate); }
        public long LongCount() { return Enumerable.LongCount(source); }
        public long LongCount(Func<T, bool> predicate) { return Enumerable.LongCount(source, predicate); }
        public Query<R> Cast<R>() { return new Query<R>(Enumerable.Cast<R>(source)); }
        public Query<T> Concat(IEnumerable<T> second) { return new Query<T>(Enumerable.Concat(source, second)); }
        public T[] ToArray() { return Enumerable.ToArray(source); }

        public IEnumerator<T> GetEnumerator()
        {
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
