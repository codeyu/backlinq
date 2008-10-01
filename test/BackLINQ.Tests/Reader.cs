#region License, Terms and Author(s)
//
// BackLINQ
// Copyright (c) 2008 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Dominik Hug, http://www.dominikhug.ch
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

namespace BackLinq.Tests
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using NUnit.Framework.SyntaxHelpers;

    #endregion

    internal sealed class Reader<T> : IEnumerable<T>
    {
        private IEnumerable<T> source;
        private IEnumerator<T> e;

        public Reader(IEnumerable<T> values)
        {
            Debug.Assert(values != null);
            source = values;
        }

        private IEnumerator<T> Enumerator
        {
            get
            {
                if (e == null)
                    e = GetEnumerator();
                return e;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (source == null) throw new Exception("A LINQ Operator called GetEnumerator() twice.");
            var enumerator = source.GetEnumerator();
            source = null;
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>Returns true if there are no more elements in the collection. </summary>
        public void AssertEnded()
        {
            Assert.That(Enumerator.MoveNext(), Is.False, "Too many elements in source.");
        }

        /// <returns>Next element in collection.</returns>
        /// <exception cref="InvalidOperationException" if there are no more elements.<exception>
        public T Read()
        {
            if (!Enumerator.MoveNext())
                throw new InvalidOperationException("No more elements in the source sequence.");
            return Enumerator.Current;
        }

        /// <param name="constraint">Checks constraint for the next element.</param>
        /// <returns>Itself</returns>
        public Reader<T> Next(Constraint constraint)
        {
            Debug.Assert(constraint != null);
            Assert.That(Read(), constraint);
            return this;
        }

        /// <summary>
        /// Checks first constraint for first elements, second constraint for second element...
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public Reader<T> Ensure(params Constraint[] constraints)
        {
            Debug.Assert(constraints != null);
            foreach (var constraint in constraints)
            {
                Next(constraint);
            }
            return this;
        }

        public void Compare(params T[] lst)
        {
            foreach (var t1 in lst)
            {
                Next(Is.EqualTo(t1));
            }
            AssertEnded();
        }
    }
}