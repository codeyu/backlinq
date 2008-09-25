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
    using System.Linq;

    #endregion

    internal sealed class OrderedEnumerable<T, K> : IOrderedEnumerable<T>
    {
        private readonly IEnumerable<T> _source;
        private readonly Comparison<Tuple<T, int>> _comparison;

        public OrderedEnumerable(IEnumerable<T> source, 
            Func<T, K> keySelector, IComparer<K> comparer, bool descending) :
            this(source, null, keySelector, comparer, descending) {}

        private OrderedEnumerable(IEnumerable<T> source, Comparison<Tuple<T, int>> parent,
            Func<T, K> keySelector, IComparer<K> comparer, bool descending)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");

            _source = source;
            
            comparer = comparer ?? Comparer<K>.Default;
            Comparison<Tuple<T, int>> comparison = (x, y) =>
            {
                var result = comparer.Compare(keySelector(x.First), keySelector(y.First));
                return (descending ? -1 : 1) * (result != 0 ? result : /* stabilizer */ x.Second.CompareTo(y.Second));
            };            
            _comparison = parent == null ? comparison : (x, y) =>
            {
                var result = parent(x, y);
                return result != 0 ? result : comparison(x, y);
            };
        }

        public IOrderedEnumerable<T> CreateOrderedEnumerable<KK>(
            Func<T, KK> keySelector, IComparer<KK> comparer, bool descending)
        {
            return new OrderedEnumerable<T, KK>(_source, _comparison, keySelector, comparer, descending);
        }

        public IEnumerator<T> GetEnumerator()
        {
            //
            // We convert the source sequence into a sequence of tuples 
            // where the second element tags the position of the element 
            // from the source sequence (First). The position is then used 
            // to perform a stable sort so where two keys compare equal,
            // the position can be used to break the tie.
            //

            var list = _source.Select((e, i) => new Tuple<T, int>(e, i)).ToList();
            list.Sort(_comparison);
            return list.Select(pv => pv.First).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}