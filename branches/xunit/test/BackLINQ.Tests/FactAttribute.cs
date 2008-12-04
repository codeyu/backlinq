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
    using System.Collections.Generic;
    using System.Reflection;
    using NUnit.Framework;
    using Xunit.Sdk;

    #endregion

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class FactAttribute : Xunit.FactAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(MethodInfo method)
        {
            var ee = (ExpectedExceptionAttribute) GetCustomAttribute(method, typeof(ExpectedExceptionAttribute));
            yield return ee != null
                       ? new NErrorTestCommand(ee.ExceptionType, method)
                       : new NTestCommand(method);
        }

        /// <summary>
        /// An <see cref="ITestCommand"/> implementation that runs 
        /// NUnit-style set-up and tear-down methods on the fixture
        /// before and after each test.
        /// </summary>

        private class NTestCommand : TestCommand
        {
            public NTestCommand(MethodInfo method)
                : base(method) { }

            public override MethodResult Execute(object testClass)
            {
                try
                {
                    var setup = testClass as ITestSetUp;
                    if (setup != null) setup.SetUp();
                    return base.Execute(testClass);
                }
                finally
                {
                    var tearDown = testClass as ITestTearDown;
                    if (tearDown != null) tearDown.TearDown();
                }
            }
        }

        /// <summary>
        /// An <see cref="ITestCommand"/> that uses 
        /// <see cref="ExpectedExceptionAttribute"/> from NUnit to 
        /// automatically test via <see cref="Xunit.Assert.Throws"/> from
        /// xUnit.
        /// </summary>

        private class NErrorTestCommand : NTestCommand
        {
            private readonly Type _exceptionType;

            public NErrorTestCommand(Type exceptionType, MethodInfo method)
                : base(method)
            {
                if (exceptionType == null) throw new ArgumentNullException("exceptionType");
                _exceptionType = exceptionType;
            }

            public override MethodResult Execute(object testClass)
            {
                Xunit.Assert.Throws(_exceptionType, () => BaseExecute(testClass));
                return new PassedResult(testMethod, DisplayName);
            }

            private MethodResult BaseExecute(object testClass)
            {
                return base.Execute(testClass);
            }
        }
    }

    //
    // Interfaces for a fixture to advertise that it implements 
    // per-test set-up and tear-down...
    //

    internal interface ITestSetUp
    {
        void SetUp();
    }

    internal interface ITestTearDown
    {
        void TearDown();
    }
}
