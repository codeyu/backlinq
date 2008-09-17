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

    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using NUnit.Framework.SyntaxHelpers;

    #endregion

    public static class Tester {
        public static void Compare<T>(this IEnumerable<T> result, params T[] list) {
            IEnumerator<T> enumerator = result.GetEnumerator();
            foreach (T item in list) {
                enumerator.MoveNext(); Assert.That(enumerator.Current, Is.EqualTo(item));
            }
            Assert.That(enumerator.MoveNext(), Is.False);
        }

        public static void Compare<TKey, TElement>(this IGrouping<TKey, TElement> result, TKey key, params TElement[] list) {
            Assert.That(result.Key, Is.EqualTo(key));
            IEnumerator<TElement> enumerator = result.GetEnumerator();
            foreach (TElement item in list) {
                enumerator.MoveNext(); Assert.That(enumerator.Current, Is.EqualTo(item));
            }
            Assert.That(enumerator.MoveNext(), Is.False);
        }
    }
}