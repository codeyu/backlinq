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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml;

    #endregion

    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                Run(args);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Trace.TraceError(e.ToString());
                return -1;
            }
        }

        private static void Run(string[] args) 
        {
            if (args.Length == 0)
                throw new ApplicationException("Missing NUnit XML output file name argument.");

            const string testCaseXPath = "//test-suite[@name='EnumerableFixture']/results/test-case";

            var reports = (
                    from arg in args 
                    let parts = arg.Split(new[]{'='}, 2) 
                    let path = parts.Last()
                    select new {
                        Title = parts.Length > 1 && !string.IsNullOrEmpty(parts[0])
                                ? parts[0] 
                                : Path.GetFileNameWithoutExtension(path),
                        TestCases = LoadXmlDocument(path).SelectNodes(testCaseXPath).Cast<XmlElement>().ToArray()
                    }
                ).ToArray();

            var baseHeaders = new[]
            {
                "Method under test", 
                "Test condition", 
                "Expected result"
            };

            var headers = reports.Select(r => "*" + r.Title + "*").Concat(baseHeaders);
            Console.WriteLine("|| " + string.Join(" || ", headers.ToArray()) + " ||");

            var testByName = reports.SelectMany(r => r.TestCases).GroupBy(test => test.GetAttribute("name"));

            const string msdnUrlFormat = @"http://msdn.microsoft.com/en-us/library/system.linq.enumerable.{0}.aspx";

            var rows = from e in testByName
                       where e.Key.Split('_').Length > 1
                       let testMethod = new TestMethod(e.Key)
                       let results = (
                           from x in e
                           let success = "true".Equals(x.GetAttribute("success"), StringComparison.OrdinalIgnoreCase)
                           select new
                           {
                               Success = success,
                               Executed = "true".Equals(x.GetAttribute("executed"), StringComparison.OrdinalIgnoreCase),
                               Message = success ? null : x.SelectSingleNode("*/message").InnerText
                           })
                           .ToArray()
                       select new { Test = testMethod, Results = results };

            foreach (var row in rows)
            {
                var testMethod = row.Test;
                Console.WriteLine("|| {0} ||", 
                    string.Join(" || ",
                        row.Results
                        .Select(r => r.Executed ? (r.Success ? "PASS" : "*FAIL*") : "-")
                        .Concat(new[] {
                           string.Format(@"[{0} {1}]{2}",
                              /* 0 */ string.Format(msdnUrlFormat, testMethod.MethodName.ToLowerInvariant()),
                              /* 1 */ testMethod.MethodName,
                              /* 2 */ testMethod.ArgumentsInBrackets),
                           testMethod.TestCondition,
                           testMethod.Expectation 
                        })
                        .ToArray()));
            }
        }

        private static XmlDocument LoadXmlDocument(string path)
        {
            var document = new XmlDocument();
            document.Load(path);
            return document;
        }
    }
}
