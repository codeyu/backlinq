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
    using System.Diagnostics;
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
            if (args.Length != 1)
                throw new ApplicationException("Missing NUnit XML output file name argument.");

            XmlDocument testResults = new XmlDocument()/*.Load(args[0])*/;
            testResults.Load(args[0]);
            Console.WriteLine("|| *Method under test* || *Test condition* || *Expected result* ||");

            var testCases = testResults.SelectNodes("//test-suite[@name='EnumerableFixture']/results/test-case");

            const string msdnUrlFormat = @"http://msdn.microsoft.com/en-us/library/system.linq.enumerable.{0}.aspx";

            var testMethods = from XmlElement e in testCases
                              where e.GetAttribute("name").Split('_').Length > 1
                              let testMethod = new TestMethod(e.Attributes["name"].Value)
                              select
                                  string.Format("|| [{0} {1}]{2} || {3} || {4} ||", 
                                                /* 0 */ string.Format(msdnUrlFormat, testMethod.MethodName.ToLowerInvariant()),
                                                /* 1 */ testMethod.MethodName,
                                                /* 2 */ testMethod.ArgumentsInBrackets,
                                                /* 3 */ testMethod.TestCondition,
                                                /* 4 */ testMethod.Expectation);

            foreach (var s in testMethods)
            {
                Console.WriteLine(s);
            }
        }
    }
}
