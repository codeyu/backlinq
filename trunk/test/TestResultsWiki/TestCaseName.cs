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

namespace TestResultsWiki
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion

    /// <summary>
    /// Helps to extract method name, parameters, test condition and expectation.
    /// </summary>

    [ Serializable ]
    internal sealed class TestCaseName
    {
        private static readonly string[] zeroArguments = new string[0];

        /// <param name="name">
        /// String like <c>"Distinct_ComparerArg_NonDistinctValues_ReturnsOnlyDistinctValues"</c>.</param>

        public TestCaseName(string name)
        {
            var parts = new Queue<string>(name.Split(new[] {'_'}, 4, StringSplitOptions.RemoveEmptyEntries));

            if (parts.Count < 2)
                throw new ArgumentException(null, "name");

            MethodName = parts.Dequeue();

            Arguments = parts.Count < 3 
                      ? zeroArguments 
                      : parts.Dequeue()
                             .Split(new[] {"Arg"}, StringSplitOptions.RemoveEmptyEntries)
                             .TakeWhile(s => s.Length > 0)
                             .ToArray();

            StateUnderTest = parts.Count > 1 
                           ? CamelCaseToSentence(parts.Dequeue()) 
                           : string.Empty;

            ExpectedBehavior = parts.Dequeue();

            var throwsWord = "Throws";
            if (ExpectedBehavior.StartsWith(throwsWord, StringComparison.InvariantCultureIgnoreCase)
                && ExpectedBehavior.EndsWith("Exception", StringComparison.InvariantCultureIgnoreCase))
            {
                ExpectedBehavior = ExpectedBehavior.Substring(0, throwsWord.Length) 
                                 + " " + ExpectedBehavior.Substring(throwsWord.Length);
            }
            else
            {
                ExpectedBehavior = CamelCaseToSentence(ExpectedBehavior);
            }
        }

        public string MethodName { get; private set; }
        public IList<string> Arguments { get; private set; }
        public string StateUnderTest { get; private set; }
        public string ExpectedBehavior { get; private set; }

        /// <summary>
        /// Creates a sentence out of a camel-cased string of words. 
        /// For example, <c>"TheQuickBrownFox"</c> is converted to 
        /// <c>"The quick brown fox"</c>.
        /// </summary>
        
        private static string CamelCaseToSentence(string camelCase)
        {
            return string.Join(" ",
                               SplitCamelCaseWords(camelCase)
                                   .Select((word, i) => i > 0 ? Decap(word) : word)
                                   .ToArray());
        }

        /// <summary>
        /// De-capitalizes by changing the first letter of the given word to 
        /// lower case.
        /// </summary>

        private static string Decap(string word)
        {
            return string.IsNullOrEmpty(word)
                 ? string.Empty
                 : char.IsUpper(word, 0)
                   ? Char.ToLower(word[0]) + word.Substring(1)
                   : word;
        }

        /// <summary>
        /// Parses and yields words from a string of camel-cased words.
        /// For example, <c>"HelloWorld"</c> is parse 
        /// into the sequence { <c>"Hello"</c>, <c>"World"</c> }.
        /// </summary>

        private static IEnumerable<string> SplitCamelCaseWords(string camelCase)
        {
            var sb = new StringBuilder();
            
            for (var i = 0; i < camelCase.Length; i++)
            {
                var ch = camelCase[i];
                if (char.IsUpper(ch) && sb.Length > 0)
                {
                    yield return sb.ToString();
                    sb.Length = 0;
                }
                sb.Append(ch);
            }

            if (sb.Length > 0)
                yield return sb.ToString();
        }
    }
}
