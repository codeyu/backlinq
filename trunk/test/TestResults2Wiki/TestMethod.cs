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

namespace TestResults2Wiki
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion

    /// <summary>Helps to extract method name, parameters, test condition and expectation.</summary>
    internal sealed class TestMethod
    {
        private static readonly string[] exceptions = new[] 
        {
            "ArgumentOutOfRangeException", 
            "InvalidOperationException", 
            "ArgumentNullException",
            "ArgumentException",
            "OverflowException",
            "InvalidCastException"
        };

        private readonly string name;

        /// <param name="name">String like BackLinq.Tests.EnumerableFixture.Distinct_ComparerArg_NonDistinctValues_ReturnsOnlyDistinctValues</param>
        public TestMethod(string name)
        {
            this.name = name;
        }

        /// <summary>Name of the method to test.</summary>
        public string MethodName
        {
            get
            {
                return (name.Split('_')[0].Split('.')[3]);
            }
        }

        /// <summary>Arguments of the methode to test.</summary>
        public string[] Arguments
        {
            get
            {
                if (name.Split('_').Length != 4) return new string[0];
                return name.Split('_')[1].Substring(0, name.Split('_')[1].Length - 3).Split(new[] { "Arg" },
                                                                                            StringSplitOptions.
                                                                                                RemoveEmptyEntries);
            }
        }

        public string ArgumentsInBrackets
        {
            get
            {
                var arguments = Arguments;
                if (arguments.Length == 0) return "()";
                StringBuilder retVal = new StringBuilder().Append("(");
                for (int i = 0; i < arguments.Length - 1; i++)
                {
                    retVal.Append(arguments[i]);
                    retVal.Append(" ");
                }
                retVal.Append(arguments.Last());
                retVal.Append(")");
                return retVal.ToString();
            }
        }

        public string TestCondition
        {
            get
            {
                var strCondition = GetSplitElement(name, 1);
                return CamelCaseToSentence(strCondition);
            }
        }

        public string Expectation
        {
            get
            {
                var strExpectation = GetSplitElement(name, 0);
                return CamelCaseToSentence(strExpectation);
            }
        }

        private static string GetSplitElement(string input, int index)
        {
            var splits = input.Split('_');
            return splits[splits.Length - ++index];
        }

        private static string CamelCaseToSentence(string camelCase)
        {
            return string.Join(" ",
                               SplitCamelCaseWords(camelCase)
                                   .Select((word, i) => i > 0 ? Capitalize(word) : word)
                                   .ToArray());
        }

        private static string Capitalize(string word)
        {
            if (!exceptions.Contains(word))
                return Char.ToLower(word[0]) + word.Substring(1);
            return word;
        }

        private static IEnumerable<string> SplitCamelCaseWordsOld(string camelCase)
        {
            var sb = new StringBuilder();
            foreach (var ch in camelCase)
            {
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

        private static IEnumerable<string> SplitCamelCaseWords(string camelCase)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < camelCase.Length; i++)
            {

                var l = TryGetExceptionLength(camelCase.Substring(i));
                if (l.HasValue)
                {
                    yield return sb.ToString();
                    sb.Length = 0;
                    yield return camelCase.Substring(i, l.Value);
                    i += l.Value;
                    continue;
                }

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

        private static int? TryGetExceptionLength(string remaining)
        {
            foreach (var s in exceptions)
            {
                if (remaining.StartsWith(s))
                    return s.Length;
            }

            return null;
        }
    }
}