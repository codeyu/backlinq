using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace TestResults2Wiki {
    class Program {
        static void Main(string[] args) {
            if (args.Length != 1) {
                Console.WriteLine("usage: Pass the name of the NUnit XML output as argument.");
                System.Environment.Exit(-1);
            }
            XmlDocument testResults = new XmlDocument()/*.Load(args[0])*/;
            testResults.Load(args[0]);
            Console.WriteLine("|| *Method under test* || *Test condition* || *Expected result* ||");

            var testCases = testResults.SelectNodes("//test-suite[@name='EnumerableFixture']/results/test-case");


            var testMethods = from XmlElement e in testCases
                              where e.GetAttribute("name").Split('_').Length > 1
                              let testMethod = new TestMethod(e.Attributes["name"].Value)
                              select
                                  "|| [http://msdn.microsoft.com/en-us/library/system.ling.enumerable." +
                                  testMethod.MethodName + ".aspx " + testMethod.MethodName + "]" +
                                  testMethod.ArgumentsInBrackets + " || " + testMethod.TestCondition + " || " +
                                  testMethod.Expectation + " ||";
     
            
            

            foreach (var s in testMethods) {
                Console.WriteLine(s);
            }
            Console.ReadLine();
        }
    }

    /// <summary>Helps to extract method name, parameters, test condition and expectation.</summary>
    class TestMethod {
        private string name;

        private static string[] exceptions = new[] {
            "ArgumentOutOfRangeException", 
            "InvalidOperationException", 
            "ArgumentNullException",
            "ArgumentException",
            "OverflowException",
            "InvalidCastException"
        };

         /// <param name="name">String like BackLinq.Tests.EnumerableFixture.Distinct_ComparerArg_NonDistinctValues_ReturnsOnlyDistinctValues</param>
        public TestMethod(string name) {
            this.name = name;
        }

        /// <summary>Name of the method to test.</summary>
        public string MethodName {
            get {
                return (name.Split('_')[0].Split('.')[3]);
            }
        }

        /// <summary>Arguments of the methode to test.</summary>
        public string[] Arguments {
            get {
                if (name.Split('_').Length != 4) return new string[0];
                return name.Split('_')[1].Substring(0, name.Split('_')[1].Length - 3).Split(new[] {"Arg"},
                                                                                            StringSplitOptions.
                                                                                                RemoveEmptyEntries);
            }
        }

        public string ArgumentsInBrackets {
            get {
                var arguments = Arguments;
                if (arguments.Length == 0) return "()";
                StringBuilder retVal = new StringBuilder().Append("(");
                for (int i = 0; i < arguments.Length -1; i++) {
                    retVal.Append(arguments[i]);
                    retVal.Append(" ");
                }
                retVal.Append(arguments.Last());
                retVal.Append(")");
                return retVal.ToString();
            }
        }

        public string TestCondition {
            get {
                var strCondition = getSplitElement(name, 1);
                return CamelCaseToSentence(strCondition);
            }
        }

        public string Expectation {
            get {
                var strExpectation = getSplitElement(name, 0);
                return CamelCaseToSentence(strExpectation);
            }
        }

        private string getSplitElement(string input, int index) {
            var splits = input.Split('_');
            return splits[splits.Length - ++index];
        }

        private static string CamelCaseToSentence(string camelCase) {
            return string.Join(" ", 
                             SplitCamelCaseWords(camelCase)
                             .Select((word, i) => i > 0 ? Capitalize(word) : word)
                             .ToArray());
        }

        private static string Capitalize(string word) {
            if (!exceptions.Contains(word))
                return Char.ToLower(word[0]) + word.Substring(1);
            return word;
        }

        private static IEnumerable<string> SplitCamelCaseWordsOld(string camelCase) {
            var sb = new StringBuilder();
            foreach (var ch in camelCase) {
                if (char.IsUpper(ch) && sb.Length > 0) {
                    yield return sb.ToString();
                    sb.Length = 0;
                }
                sb.Append(ch);
            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }

        private static IEnumerable<string> SplitCamelCaseWords(string camelCase) {
            var sb = new StringBuilder();
            for (int i = 0; i < camelCase.Length; i++ ) {

                var l = checkException(camelCase.Substring(i));
                if (l > -1) {
                    yield return sb.ToString();
                    sb.Length = 0;
                    yield return camelCase.Substring(i, l);
                    i += l;
                    continue;
                }

                var ch = camelCase[i];
                if (char.IsUpper(ch) && sb.Length > 0) {
                    yield return sb.ToString();
                    sb.Length = 0;
                }
                sb.Append(ch);

            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }

        private static int checkException(string remaining) {
            foreach (var s in exceptions) {
                if (remaining.StartsWith(s)) {
                    return s.Length;
                }
            }
            return -1;
        }
    }
}
