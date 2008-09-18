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

namespace BackLinq.Tests {

    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using NUnit.Framework;
    using System.Linq;
    using System.Diagnostics;
    using Constraint = NUnit.Framework.Constraints.Constraint;
    using NUnit.Framework.SyntaxHelpers;

    #endregion

    [TestFixture]
    public sealed class EnumerableFixture {

        /// <summary>
        /// Stores the Culture of the Thread do undo the change in the TearDown.
        /// </summary>
        private CultureInfo initialCulture;

        [SetUp]
        public void SetUp() {
            initialCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-CH");
        }

        [TearDown]
        public void TearDown() {
            System.Threading.Thread.CurrentThread.CurrentCulture = initialCulture;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Aggregate_FuncArg_EmptySource_ThrowsInvalidOperationException() {
            var source = new OnceEnumerable<int>(new int[0]);
            source.Aggregate((a, b) => a + b);
        }

        [Test]
        public void Aggregate_FuncArg_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            var result = source.Aggregate((a, b) => a + b);
            Assert.That(result, Is.EqualTo(55));
        }

        [Test]
        public void Aggregate_AccumulatorArgFuncArg_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var result = source.Aggregate(100, (a, b) => a + b);
            Assert.That(result, Is.EqualTo(155));
        }

        [Test]
        public void Empty__YieldsNoResults() {
            var source = Enumerable.Empty<String>();
            Assert.That(source, Is.Not.Null);

            var e = source.GetEnumerator();
            Assert.That(e, Is.Not.Null);

            Assert.That(e.MoveNext(), Is.False);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Cast_NullAsArgument_ThrowsArgumentNullException() {
            Enumerable.Cast<Object>(null);
        }

        [Test]
        [ExpectedException(typeof(InvalidCastException))]
        public void Cast_InvalidSource_ThrowsInvalidCastException() {
            var list = new OnceEnumerable<object>( new List<object>() {1000, "hello", new object()});
            IEnumerable<byte> target = Enumerable.Cast<byte>(list);
            // do something with the results so Cast will really be executed (deferred execution)
            StringBuilder sb = new StringBuilder();
            foreach (var b in target) {
                sb.Append(b.ToString());
            }
        }

        /// <summary>Tests a downcast from object to int.</summary>
        [Test]
        public void Cast_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<object>(new List<object>() {1, 10, 100});
            Enumerable.Cast<int>(source).Compare(1, 10, 100);
        }

        /// <summary>Tests an upcast from int to object.</summary>
        [Test]
        public void Cast_ArrayOfInts_CorrectCastToObjects() {
            Enumerable.Cast<object>(new OnceEnumerable<int>(new int[] {1, 10, 100})).Compare(1, 10, 100);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void All_Null_ThrowsArgumentNullException() {
            Enumerable.All(null, (int i) => {
                                     return i >= 0;
                                 });
        }

        [Test]
        public void All_ArgumentsNotSatifyingCondition_ReturnsFalse() {
            var source = new OnceEnumerable<int>(new List<int>() {-100, -1, 0, 1, 100});
            Assert.That(Enumerable.All(source, i => i >= 0), Is.False);
        }

        [Test]
        public void All_ArgumentsSatisfyingCondition_ReturnsTrue() {
            var source = new OnceEnumerable<int>(new List<int>() { -100, -1, 0, 1, 100 });
            Assert.That(Enumerable.All(source, i => i >= -100), Is.True);
        }

        /// <summary>Tests weather Any() returns false for an empty list.
        /// Tests weather Any() returns true for a list with one element.</summary>
        [Test]
        public void Any_EmptySource_ReturnsFalse() {
            var source = new object[0];
            Assert.That(source.Any(), Is.False);
        }

        [Test]
        public void Any_NonEmptySource_ReturnsTrue() {
            var source = new OnceEnumerable<object>(new[] {new object()});
            Assert.That(source.Any(), Is.True);
        }

        /// <summary>
        /// Tests weather Any() works if you call the overload which takes a Func(<T>, bool)
        /// </summary>
        [Test]
        public void Any_PredicateArg_EmptySource_ReturnsFalse() {
            var func = new Func<int, bool>(i => i > 0);
            var source = new OnceEnumerable<int>(new List<int>());
            Assert.That(source.Any(func), Is.False);
        }

        [Test]
        public void Any_PredicateArg_NonEmptySource_ReturnsTrue() {
            var func = new Func<int, bool>(i => i > 0);
            var list = new List<int>(0);
            var source = new OnceEnumerable<int>(list);
            Assert.That(source.Any(func), Is.False);
            list.Add(100);
            source = new OnceEnumerable<int>(list);
            Assert.That(source.Any(func), Is.True);
        }

        [Test]
        public void Average_Decimals_CorrectResult() {
            var source = new OnceEnumerable<decimal>(new List<decimal>() {-10000, 2.0001m, 50});
            Assert.That(source.Average(), Is.EqualTo(-3315.999966).Within(0.00001));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Concat_FirstArgumentNull_ThrowsArgumentNullException() {
            Enumerable.Concat(null, new int[] {3, 5});
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Concat_SecondArgumentNull_ThrowsArgumentNullException() {
            var result = Enumerable.Concat(new int[] { 3, 5 }, null);
        }

        [Test]
        public void Concat_TwoLists_CorrectOrder() {
            var list1 = new OnceEnumerable<int>(new int[] {1, 2, 3});
            var list2 = new OnceEnumerable<int>(new int[] {4, 5, 6});
            Enumerable.Concat(list1, list2).Compare(1, 2, 3, 4, 5, 6);
        }

        [Test]
        public void DefaultIfEmpty_NotEmpty_ReturnsItself() {
            var source = new OnceEnumerable<int>(new int[] { 1, 2, 3 });
            source.DefaultIfEmpty(1).Compare(1, 2, 3);
        }

        [Test]
        public void DefaultIfEmpty_Empty_ReturnsZero() {
            var source = new OnceEnumerable<int>(new int[0]);
            source.DefaultIfEmpty().Compare(0);
        }

        [Test]
        public void DefaultIfEmpty_DefaultValueArg_Empty_ReturnsDefault() {
            var source = new OnceEnumerable<int>(new int[0]);
            source.DefaultIfEmpty(5).Compare(5);
        }

        [Test]
        public void DefaultIfEmpty_DefaultValue_NotEmpty_ReturnsItself() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3});
            source.DefaultIfEmpty(5).Compare(1, 2, 3);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Distinct_ArgumentNull_ThrowsArgumentNullException() {
            Enumerable.Distinct<int>(null);
        }

        [Test]
        public void Distinct_ArgumentWithNonDistinctValues_ReturnsOnlyDistinctValues() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 2, 3});
            source.Distinct().Compare(1, 2, 3);
        }

        [Test]
        public void Distinct_ComparerArg_NonDistinctValues_ReturnsOnlyDistinctValues() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 2, 3});
            source.Distinct(EqualityComparer<int>.Default).Compare(1, 2, 3);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ElementAt_ArgumentOutOfRange_ThrowsArgumentOutOfRangeException() {
            var source = new OnceEnumerable<int>(new int[] {3, 5, 7});
            source.ElementAt(3);
        }

        [Test]
        public void ElementAt_ValidArguments_ReturnsCorrectValues() {
            var source = new int[] {15, 2, 7};
            Assert.That(source.ElementAt(0), Is.EqualTo(15));
            Assert.That(source.ElementAt(1), Is.EqualTo(2));
            Assert.That(source.ElementAt(2), Is.EqualTo(7));
        }

        [Test]
        public void ElementAtOrDefault_IntArray_ReturnsZeroIfIndexOutOfRange() {
            var source = new OnceEnumerable<int>(new int[] {3, 6, 8});
            Assert.That(source.ElementAtOrDefault(3), Is.EqualTo(0));
        }

        [Test]
        public void ElementAtOrDefault_IntArray_ReturnsCorrectValue() {
            var source = new OnceEnumerable<int>(new int[] {3, 6, 9});
            Assert.That(source.ElementAtOrDefault(2), Is.EqualTo(9));
        }

        [Test]
        public void ElementAtOrDefault_ObjectArray_ReturnsNullIfIndexOutOfRange() {
            var source = new OnceEnumerable<object>(new object[] {new object(), new object()});
            Assert.That(source.ElementAtOrDefault(2), Is.EqualTo(null));
        }

        [Test]
        public void ElementAtOrDefault_ObjectArray_ReturnsCorrectValue() {
            object obj1 = new object();
            object obj2 = new object();
            var source = new OnceEnumerable<object>(new object[] {obj1, obj2});
            Assert.That(source.ElementAt(0), Is.EqualTo(obj1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Except_secondArg_ArgumentNull_ThrowsArgumentNullException() {
            var source = new OnceEnumerable<int>(new int[] {1, 4, 7});
            source.Except(null);
        }

        [Test]
        public void Except_secondArg_ValidArgument_ReturnsCorrectEnumerable() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            var argument = new OnceEnumerable<int>(new int[] {1, 3, 5, 7, 9});
            source.Except(argument).Compare(2, 4, 6, 8, 10);
        }

        [Test]
        public void Except_secondArgComparerArg_ComparerIsUsed() {
            var source = new OnceEnumerable<string>( new string[] {"albert", "john", "simon"});
            var argument = new OnceEnumerable<string>(new string[] {"ALBERT"});
            source.Except(argument, StringComparer.CurrentCultureIgnoreCase).Compare("john", "simon");
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void First_EmptyArray_ThrowsInvalidOperationException() {
            var source = new int[0];
            source.First();
        }

        [Test]
        public void First_IntArray_ReturnsFirst() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4});
            Assert.That(source.First(), Is.EqualTo(1));
        }

        [Test]
        public void First_PredicateArg_IntArray_PredicateIsUsed() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4});
            Assert.That(source.First(i => i%2 == 0), Is.EqualTo(2));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void First_PredicateArg_NoElementMatches_InvalidOperationExceptionIsThrown() {
            var source = new OnceEnumerable<int>(new int[] { 1, 2, 3, 4 });
            Assert.That(source.First(i => i > 5), Is.EqualTo(0));
        }

        [Test]
        public void FirstOrDefault_EmptyBoolArray_FalseIsReturned() {
            var source = new OnceEnumerable<bool>(new bool[0]);
            Assert.That(source.FirstOrDefault(), Is.False);
        }

        [Test]
        public void FirstOrDefault_ObjectArray_FirstIsReturned() {
            object obj1 = new object();
            object obj2 = new object();
            var source = new OnceEnumerable<object>(new object[] {obj1, obj2});
            Assert.That(source.FirstOrDefault(), Is.EqualTo(obj1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FirstOrDefault_PredicateArg_NullAsPredicate_ArgumentNullExceptionIsThrown() {
            var source = new int[] {3, 5, 7};
            source.FirstOrDefault(null);
        }

        [Test]
        public void FirstOrDefault_PredicateArg_ValidPredicate_FirstMatchingItemIsReturned() {
            var source = new OnceEnumerable<int>(new int[] {1, 4, 8});
            Assert.That(source.FirstOrDefault(i => i%2 == 0), Is.EqualTo(4));
        }

        [Test]
        public void FirstOrDefault_PredicateArg_NoMatchesInArray_DefaultValueOfTypeIsReturned() {
            var source = new OnceEnumerable<int>(new int[] {1, 4, 6});
            Assert.That(source.FirstOrDefault(i => i > 10), Is.EqualTo(0));
        }

        private class Person {
            public string FirstName { get; set; }
            public string FamilyName { get; set; }
            public int Age { get; set; }
            public static Person[] CreatePersons() {
                return new Person[]
                           {
                               new Person() {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                               new Person() {FamilyName = "Müller", FirstName = "Herbert", Age = 22},
                               new Person() {FamilyName = "Meier", FirstName = "Hubert", Age = 23},
                               new Person() {FamilyName = "Meier", FirstName = "Isidor", Age = 24}   
                           };
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupBy_KeySelectorArg_NullAsKeySelector_ThrowsArgumentNullException() {
            var source = new OnceEnumerable<Person>(Person.CreatePersons());
            source.GroupBy<Person, String>(null);
        }

        [Test]
        public void GroupBy_KeySelectorArg_ValidArguments_CorrectGrouping() {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var result = new Reader<IGrouping<string, Person>>(persons.GroupBy(person => person.FamilyName));

            var mueller = result.Read();
            Assert.That(mueller.Key, Is.EqualTo("Müller"));
            Assert.That(Array.ConvertAll(ToArray(mueller), p => p.FirstName), 
                        Is.EqualTo(new[] { "Peter", "Herbert" }));

            var meier = result.Read();
            Assert.That(meier.Key, Is.EqualTo("Meier"));
            Assert.That(Array.ConvertAll(ToArray(meier), p => p.FirstName),
                        Is.EqualTo(new[] { "Hubert", "Isidor" }));

            result.AssertEnded();
        }

        private static T[] ToArray<T>(IEnumerable<T> source) {
            return new List<T>(source).ToArray();
        }

        [Test]
        // TODO: make better
        public void GroupBy_KeySelectorArg_ValidArguments_CorrectCaseSensitiveGrouping() {
            var persons = new OnceEnumerable<Person>( new Person[]
                                 {
                                     new Person() {FamilyName = "Müller", FirstName = "Peter"},
                                     new Person() {FamilyName = "müller", FirstName = "Herbert"},
                                     new Person() {FamilyName = "Meier", FirstName = "Hubert"},
                                     new Person() {FamilyName = "meier", FirstName = "Isidor"}
                                 });
            var result = persons.GroupBy<Person, String>(person => person.FamilyName);
            var enumerator = result.GetEnumerator();
            enumerator.MoveNext();
            Assert.That(enumerator.Current.Key, Is.EqualTo("Müller"));
            Assert.That(enumerator.Current.ElementAt(0).FirstName, Is.EqualTo("Peter"));
            enumerator.MoveNext();
            Assert.That(enumerator.Current.Key, Is.EqualTo("müller"));
            Assert.That(enumerator.Current.ElementAt(0).FirstName, Is.EqualTo("Herbert"));
            enumerator.MoveNext();
            Assert.That(enumerator.Current.Key, Is.EqualTo("Meier"));
            Assert.That(enumerator.Current.ElementAt(0).FirstName, Is.EqualTo("Hubert"));
            enumerator.MoveNext();
            Assert.That(enumerator.Current.Key, Is.EqualTo("meier"));
            Assert.That(enumerator.Current.ElementAt(0).FirstName, Is.EqualTo("Isidor"));

            Assert.That(enumerator.MoveNext(), Is.False);
        }

        [Test]
        public void GroupBy_KeySelectorArgComparerArg_KeysThatDifferInCasingNonCaseSensitiveStringComparer_CorrectGrouping() {
            var persons = new OnceEnumerable<Person>( new Person[]
                                 {
                                     new Person() {FamilyName = "Müller", FirstName = "Peter"},
                                     new Person() {FamilyName = "müller", FirstName = "Herbert"},
                                     new Person() {FamilyName = "Meier", FirstName = "Hubert"},
                                     new Person() {FamilyName = "meier", FirstName = "Isidor"}
                                 });
            var result = persons.GroupBy<Person, String>(person => person.FamilyName, StringComparer.InvariantCultureIgnoreCase);
            var enumerator = result.GetEnumerator();
            enumerator.MoveNext();
            Assert.That(enumerator.Current.Key, Is.EqualTo("Müller"));
            Assert.That(enumerator.Current.ElementAt(0).FirstName, Is.EqualTo("Peter"));
            Assert.That(enumerator.Current.ElementAt(1).FirstName, Is.EqualTo("Herbert"));

            enumerator.MoveNext();
            Assert.That(enumerator.Current.Key, Is.EqualTo("Meier"));
            Assert.That(enumerator.Current.ElementAt(0).FirstName, Is.EqualTo("Hubert"));
            Assert.That(enumerator.Current.ElementAt(1).FirstName, Is.EqualTo("Isidor"));

            Assert.That(enumerator.MoveNext(), Is.False);
        }

        [Test]
        public void GroupBy_KeySelectorArgElementSelectorArg_ValidArguments_CorrectGroupingAndProjection() {
            var enumerable = new OnceEnumerable<Person>(Person.CreatePersons());
            var result = enumerable.GroupBy(person => person.FamilyName, person => person.Age);
            var enumerator = result.GetEnumerator();
            enumerator.MoveNext();
            Assert.That(enumerator.Current.Key, Is.EqualTo("Müller"));
            Assert.That(enumerator.Current.ElementAt(0), Is.EqualTo(21));
            Assert.That(enumerator.Current.ElementAt(1), Is.EqualTo(22));
            
            enumerator.MoveNext();
            Assert.That(enumerator.Current.Key, Is.EqualTo("Meier"));
            Assert.That(enumerator.Current.ElementAt(0), Is.EqualTo(23));
            Assert.That(enumerator.Current.ElementAt(1), Is.EqualTo(24));
        }

        [Test]
        public void GroupBy_KeySelectorArgResultSelectorArg_ValidArguments_CorrectGroupingProcessing() {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());

            IEnumerable<int> result = persons.GroupBy<Person, string, int>(person => person.FamilyName,
                                                (string key, IEnumerable<Person> group) => {
                                                    int ageSum = 0;
                                                    foreach(Person p in group) {
                                                        ageSum += p.Age;
                                                    }
                                                    return ageSum;                                                                                            }
                                                );
            result.Compare(43, 47);
        }

        [Test]
        public void GroupByKey_KeySelectorArgElementSelectorArgComparerArg_ValidArguments_CorrectGroupingAndProcessing() {
            var persons = new OnceEnumerable<Person>( new Person[]
                                 {
                                     new Person() {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person() {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person() {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person() {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });

            IEnumerable<IGrouping<string, int>> result = persons.GroupBy<Person, string, int>(person => person.FamilyName,
                                                person => person.Age,
                                                StringComparer.CurrentCultureIgnoreCase);
            IEnumerator<IGrouping<string, int>> enumerator = result.GetEnumerator();
            enumerator.MoveNext();
            Assert.That(enumerator.Current.ElementAt(0), Is.EqualTo(21));
            Assert.That(enumerator.Current.ElementAt(1), Is.EqualTo(22));
            enumerator.MoveNext();
            Assert.That(enumerator.Current.ElementAt(0), Is.EqualTo(23));
            Assert.That(enumerator.Current.ElementAt(1), Is.EqualTo(24));
            Assert.That(enumerator.MoveNext(), Is.False);
        }

        [Test]
        public void GroupBy_KeySelectorArgElementSelectorArgResultSelectorArg_ValidArguments_CorrectGroupingAndTransforming() {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var result = persons.GroupBy<Person, string, int, int>(p => p.FamilyName, p => p.Age,
                                                                      (string name, IEnumerable<int> enumerable2) => {
                                                                          int totalAge = 0;
                                                                          foreach (var i in enumerable2) {
                                                                              totalAge += i;
                                                                          }
                                                                          return totalAge;
                                                                      });
            result.Compare(43, 47);
        }

        [Test]
        public void GroupBy_KeySelectorArgResultSelectorArgComparerArg_ValidArguments_CorrectGroupingAndTransforming() {
            var persons = new OnceEnumerable<Person>( new Person[]
                                 {
                                     new Person() {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person() {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person() {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person() {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });
            var result = persons.GroupBy<Person, string, int>(p => p.FamilyName, 
                                                                      (string name, IEnumerable<Person> enumerable2) => {
                                                                          int totalAge = 0;
                                                                          foreach (var i in enumerable2) {
                                                                              totalAge += i.Age;
                                                                          }
                                                                          return totalAge;
                                                                      },
                                                                      StringComparer.CurrentCultureIgnoreCase);
            result.Compare(43, 47);
        }

        [Test]
        public void GroupBy_KeySelectorArgElementSelectorArgResultSelectorArgComparerArg_ValidArguments_CorrectGroupingAndTransforming() {
            var persons = new OnceEnumerable<Person>(new Person[]
                                 {
                                     new Person() {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person() {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person() {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person() {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });
            var result = persons.GroupBy<Person, string, int, int>(p => p.FamilyName, p => p.Age,
                                                                      (string name, IEnumerable<int> enumerable2) => {
                                                                          int totalAge = 0;
                                                                          foreach (var i in enumerable2) {
                                                                              totalAge += i;
                                                                          }
                                                                          return totalAge;
                                                                      }, StringComparer.CurrentCultureIgnoreCase);
            result.Compare(43, 47);
        }

        class Pet {
            public string Name { get; set; }
            public string Owner { get; set; }
        }

        [Test]
        public void GroupJoinInnerOuterKeySelectorInnerKeySelectorResultSelector_ValidArguments_CorrectGroupingAndJoining() {
            var persons = new OnceEnumerable<Person>( new Person[]
                                 {
                                     new Person() {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person() {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person() {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person() {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });

            var pets = new OnceEnumerable<Pet>( new Pet[]
                           {
                               new Pet() {Name = "Barley", Owner = "Peter"},
                               new Pet() {Name = "Boots", Owner = "Herbert"},
                               new Pet() {Name = "Whiskers", Owner = "Herbert"},
                               new Pet() {Name = "Daisy", Owner = "Isidor"}
                           });

            var result = persons.GroupJoin(pets, person => person.FirstName, pet => pet.Owner,
                              (person, petCollection) =>
                              new { OwnerName = person.FirstName, Pets = petCollection.Select(pet => pet.Name) });

            var enumerator = result.GetEnumerator();
            enumerator.MoveNext(); Assert.That(enumerator.Current.OwnerName, Is.EqualTo("Peter"));
            var petEnumerator = enumerator.Current.Pets.GetEnumerator();
            petEnumerator.MoveNext(); Assert.That(petEnumerator.Current, Is.EqualTo("Barley"));
            Assert.That(petEnumerator.MoveNext(), Is.False);
            enumerator.MoveNext(); Assert.That(enumerator.Current.OwnerName, Is.EqualTo("Herbert"));
            petEnumerator = enumerator.Current.Pets.GetEnumerator();
            petEnumerator.MoveNext(); Assert.That(petEnumerator.Current, Is.EqualTo("Boots"));
            petEnumerator.MoveNext(); Assert.That(petEnumerator.Current, Is.EqualTo("Whiskers"));
            Assert.That(petEnumerator.MoveNext(), Is.False);
            enumerator.MoveNext(); Assert.That(enumerator.Current.OwnerName, Is.EqualTo("Hubert"));
            petEnumerator = enumerator.Current.Pets.GetEnumerator();
            Assert.That(petEnumerator.MoveNext(), Is.False);
            enumerator.MoveNext(); Assert.That(enumerator.Current.OwnerName, Is.EqualTo("Isidor"));
            petEnumerator = enumerator.Current.Pets.GetEnumerator();
            petEnumerator.MoveNext(); Assert.That(petEnumerator.Current, Is.EqualTo("Daisy"));
            Assert.That(petEnumerator.MoveNext(), Is.False);
            Assert.That(enumerator.MoveNext(), Is.False);

            //foreach (var owner in result) {
            //    Debug.WriteLine(owner.OwnerName);
            //    Debug.Indent();
            //    foreach (var petName in owner.Pets) {
            //        Debug.WriteLine("    " + petName);
            //    }
            //    Debug.Unindent();
            //}
        }

        [Test]
        [Ignore("Pending resolution of issue #14 (http://code.google.com/p/backlinq/issues/detail?id=14).")]
        public void GroupJoinInnerOuterKeySelectorInnerKeySelectorResultSelectorComparer_ValidArguments_CorrectGroupingAndJoining()
        {
            var persons = new OnceEnumerable<Person>( new Person[]
                                 {
                                     new Person() {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person() {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person() {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person() {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });

            var pets = new OnceEnumerable<Pet>( new Pet[]
                           {
                               new Pet() {Name = "Barley", Owner = "Peter"},
                               new Pet() {Name = "Boots", Owner = "Herbert"},
                               new Pet() {Name = "Whiskers", Owner = "herbert"}, // This pet is not associated to "Herbert"
                               new Pet() {Name = "Daisy", Owner = "Isidor"}
                           });

            var result = persons.GroupJoin(pets, person => person.FirstName, pet => pet.Owner,
                              (person, petCollection) =>
                              new { OwnerName = person.FirstName, Pets = petCollection.Select(pet => pet.Name) },
                              StringComparer.CurrentCultureIgnoreCase);

            var enumerator = result.GetEnumerator();
            enumerator.MoveNext(); Assert.That(enumerator.Current.OwnerName, Is.EqualTo("Peter"));
            var petEnumerator = enumerator.Current.Pets.GetEnumerator();
            petEnumerator.MoveNext(); Assert.That(petEnumerator.Current, Is.EqualTo("Barley"));
            Assert.That(petEnumerator.MoveNext(), Is.False);
            enumerator.MoveNext(); Assert.That(enumerator.Current.OwnerName, Is.EqualTo("Herbert"));
            petEnumerator = enumerator.Current.Pets.GetEnumerator();
            petEnumerator.MoveNext(); Assert.That(petEnumerator.Current, Is.EqualTo("Boots"));
            petEnumerator.MoveNext(); Assert.That(petEnumerator.Current, Is.EqualTo("Whiskers"));
            Assert.That(petEnumerator.MoveNext(), Is.False);
            enumerator.MoveNext(); Assert.That(enumerator.Current.OwnerName, Is.EqualTo("Hubert"));
            petEnumerator = enumerator.Current.Pets.GetEnumerator();
            Assert.That(petEnumerator.MoveNext(), Is.False);
            enumerator.MoveNext(); Assert.That(enumerator.Current.OwnerName, Is.EqualTo("Isidor"));
            petEnumerator = enumerator.Current.Pets.GetEnumerator();
            petEnumerator.MoveNext(); Assert.That(petEnumerator.Current, Is.EqualTo("Daisy"));
            Assert.That(petEnumerator.MoveNext(), Is.False);
            Assert.That(enumerator.MoveNext(), Is.False);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupJoinInnerOuterKeySelectorInnerKeySelectorResultSelector_PassNullAsInner_ArgumentNullExceptionIsThrown() {
            var persons = new OnceEnumerable<Person>( new Person[]
                                 {
                                     new Person() {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person() {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person() {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person() {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });

            var pets = new OnceEnumerable<Pet>( new Pet[]
                           {
                               new Pet() {Name = "Barley", Owner = "Peter"},
                               new Pet() {Name = "Boots", Owner = "Herbert"},
                               new Pet() {Name = "Whiskers", Owner = "Herbert"},
                               new Pet() {Name = "Daisy", Owner = "Isidor"}
                           });

            persons.GroupJoin(pets, null, pet => pet.Owner,
                              (person, petCollection) =>
                              new { OwnerName = person.FirstName, Pets = petCollection.Select(pet => pet.Name) });

        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Intersect_PassNullAsArgument_ThrowsArgumentNullException() {
            var source = new OnceEnumerable<int>( new int[] {1, 2, 3});
            source.Intersect(null);
        }

        [Test]
        public void Intersect_ValidArguments_ReturnsIntersection() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3});
            var argument =new OnceEnumerable<int>(new int[] {2, 3, 4});
            var result = source.Intersect(argument);
            result.Compare(2, 3);
        }

        [Test]
        public void Intersect_WithEqualityComparer_EqualityComparerIsUsed() {
            var enumerable = new OnceEnumerable<string>(new string[] {"Heinrich", "Hubert", "Thomas"});
            var argument = new OnceEnumerable<string>(new string[] {"Heinrich", "hubert", "Joseph"});
            var result = enumerable.Intersect(argument, StringComparer.CurrentCultureIgnoreCase);
            result.Compare("Heinrich", "Hubert");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Join_PassNullAsArgument_ArgumentNullExceptionIsThrown() {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            persons.Join<Person, string, string, string>(null, null, null, null);
        }

        [Test]
        public void Join_ValidArguments_CorrectResult() {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var pets = new OnceEnumerable<Pet>( new Pet[]
                           {
                               new Pet() {Name = "Barley", Owner = "Peter"},
                               new Pet() {Name = "Boots", Owner = "Herbert"},
                               new Pet() {Name = "Whiskers", Owner = "Herbert"},
                               new Pet() {Name = "Daisy", Owner = "Isidor"}
                           });
            var result = persons.Join(pets, aPerson => aPerson.FirstName, aPet => aPet.Owner,
                         (aPerson, aPet) => new {Owner = aPerson.FirstName, Pet = aPet.Name});

            var enumerator = result.GetEnumerator();
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current.Owner, Is.EqualTo("Peter"));
            Assert.That(enumerator.Current.Pet, Is.EqualTo("Barley"));

            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current.Owner, Is.EqualTo("Herbert"));
            Assert.That(enumerator.Current.Pet, Is.EqualTo("Boots"));

            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current.Owner, Is.EqualTo("Herbert"));
            Assert.That(enumerator.Current.Pet, Is.EqualTo("Whiskers"));

            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current.Owner, Is.EqualTo("Isidor"));
            Assert.That(enumerator.Current.Pet, Is.EqualTo("Daisy"));

            Assert.That(enumerator.MoveNext(), Is.False);
        }

        [Test]
        public void Join_ValidArgumentsAndComparer_CorrectResult() {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var pets = new OnceEnumerable<Pet>(new Pet[]
                           {
                               new Pet() {Name = "Barley", Owner = "Peter"},
                               new Pet() {Name = "Boots", Owner = "Herbert"},
                               new Pet() {Name = "Whiskers", Owner = "herbert"},
                               new Pet() {Name = "Daisy", Owner = "Isidor"}
                           });
            var result = persons.Join(pets, aPerson => aPerson.FirstName, aPet => aPet.Owner,
                         (aPerson, aPet) => new { Owner = aPerson.FirstName, Pet = aPet.Name },
                         StringComparer.CurrentCultureIgnoreCase);

            var enumerator = result.GetEnumerator();
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current.Owner, Is.EqualTo("Peter"));
            Assert.That(enumerator.Current.Pet, Is.EqualTo("Barley"));

            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current.Owner, Is.EqualTo("Herbert"));
            Assert.That(enumerator.Current.Pet, Is.EqualTo("Boots"));

            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current.Owner, Is.EqualTo("Herbert"));
            Assert.That(enumerator.Current.Pet, Is.EqualTo("Whiskers"));

            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current.Owner, Is.EqualTo("Isidor"));
            Assert.That(enumerator.Current.Pet, Is.EqualTo("Daisy"));

            Assert.That(enumerator.MoveNext(), Is.False);

            //foreach (var i in result) {
            //    Debug.WriteLine(String.Format("Owner = {0}; Pet = {1}", i.Owner, i.Pet));
            //}
        }

        [Test]
        public void Last_ListOfInts_LastElementIsReturned() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3});
            Assert.That(source.Last(), Is.EqualTo(3));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LastPredicate_NullAsPredicate_ThrowsArgumentNullException() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3});
            source.Last(null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LastPredicate_NoMatchingElement_ThrowsInvalidOperationException() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5});
            source.Last(i => i > 10);
        }

        [Test]
        public void LastPredicate_ListOfInts_LastMatchingElementIsReturned() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5});
            Assert.That(source.Last(i => i%2 == 0), Is.EqualTo(4));
        }

        [Test]
        public void LastOrDefault_EmptySource_0IsReturned() {
            var source = new OnceEnumerable<int>(new int[0]);
            Assert.That(source.LastOrDefault(), Is.EqualTo(0));
        }

        [Test]
        public void LastOrDefault_NonEmptyList_LastElementIsReturned() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5});
            Assert.That(source.LastOrDefault(), Is.EqualTo(5));
        }

        [Test]
        public void LastOrDefaultPredicate_ValidArguments_LastMatchingElementIsReturned() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5});
            Assert.That(source.LastOrDefault(i => i%2 == 0), Is.EqualTo(4));
        }

        [Test]
        public void LastOrDefaultPredicate_NoMatchingElement_0IsReturned() {
            var source = new OnceEnumerable<int>(new int[] {1, 3, 5, 7});
            Assert.That(source.LastOrDefault(i => i%2 == 0), Is.EqualTo(0));
        }

        [Test]
        public void LongCount_ValidArgument_ReturnsCorrectNumberOfElementsAsLong() {
            var source = new OnceEnumerable<int>(new int[] {1, 4, 7, 10});
            Assert.That(source.LongCount(), Is.EqualTo(4));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LongCountPredicate_NullAsPredicate_ThrowsArgumentNullException() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4});
            source.LongCount(null);
        }

        [Test] 
        public void LongCountPredicate_ValidArguments_ReturnsCorrectNumerOfMatchingElements() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5});
            Assert.That(source.LongCount(i => i%2 == 0), Is.EqualTo(2));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Max_EmptyList_ThrowsInvalidOperationException()
        {
            var source = new OnceEnumerable<int>(new int[0]);
            var result = source.Max();
        }

        [Test]
        public void Max_ListOfInts_MaxValueIsReturned() {
            var source = new OnceEnumerable<int>(new int[] {1000, 203, -9999});
            Assert.That(source.Max(), Is.EqualTo(1000));
        }

        [Test]
        public void Max_ListWithNullableType_ReturnsMaximum() {
            var source = new OnceEnumerable<int?>(new int?[] {1, 4, null, 10});
            Assert.That(source.Max(), Is.EqualTo(10));
        }

        [Test]
        public void Max_ListOfObjectsAndSelector_MaxSelectedValueIsReturned() {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            Assert.That(persons.Max(p => p.Age), Is.EqualTo(24));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Min_EmptyList_ThrowsInvalidOperationException() {
            var source = new OnceEnumerable<int>(new int[0]);
            source.Min();
        }

        [Test]
        [Ignore("Pending resolution of issue #8 (http://code.google.com/p/backlinq/issues/detail?id=8).")]
        public void Min_ListWithNullables_MinimumNonNullValueIsReturned()
        {
            var source = new OnceEnumerable<int?>(new int?[] {199, 15, null, 30});
            Assert.That(source.Min(), Is.EqualTo(15));
        }

        [Test]
        [Ignore("Pending resolution of issue #8 (http://code.google.com/p/backlinq/issues/detail?id=8).")]
        public void MinSelector_ValidArguments_MinimumNonNullValueIsReturned()
        {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            Assert.That(persons.Min<Person, int?>((Person p) => {
                                        if (p.Age == 21) return null; // to test behavior if null belongs to result of transformation
                                        else return (p.Age);
                                    }), Is.EqualTo(22));
        }
       
        [Test]
        public void OfType_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<object>(new[] {1, "Hello", 1.234m, new object()});
            var result = source.OfType<decimal>();
            result.Compare(1.234m);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrderByKeySelector_NullAsKeySelector_ThrowsArgumentNullException() {
            var source = new OnceEnumerable<int>(new int[] {1, 4, 2});
            source.OrderBy<int, int>(null);
        }

        [Test]
        public void OrderByKeySelector_ValidArguments_CorrectOutput() {
            var persons = Person.CreatePersons();
            var stack = new Stack<Person>();
            foreach (var person in persons) {
                stack.Push(person);
            }
            var reversePersons = new List<Person>();
            foreach (var person in stack) {
                reversePersons.Add(person);
            }
            var source = new OnceEnumerable<Person>(reversePersons);
            var result = source.OrderBy(p => p.Age);

            int age = 20;
            foreach (var person in result) {
                age++;
                Assert.That(person.Age, Is.EqualTo(age));
            }
            Assert.That(age, Is.EqualTo(24));
        }

        /// <summary>
        /// To sort ints in descending order.
        /// </summary>
        class ReverseComparer : IComparer<int> {
            public int Compare(int x, int y) {
                return y.CompareTo(x);
            }
        }

        [Test]
        public void OrderByKeySelectorComparer_ValidArguments_CorrectOutput() {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var result = persons.OrderBy<Person, int>(p => p.Age, new ReverseComparer());
            var age = 25;
            foreach (var person in result) {
                age--;
                Assert.That(person.Age, Is.EqualTo(age));
            }
            Assert.That(age, Is.EqualTo(21));

        }

        [Test]
        public void OrderDescendingByKeySelector_ValidArguments_CorrectOutput() {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var result = persons.OrderByDescending(p => p.Age);
            int age = 25;
            foreach (var person in result) {
                age--;
                Assert.That(person.Age, Is.EqualTo(age));
            }
            Assert.That(age, Is.EqualTo(21));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Range_ProduceRangeThatLeadsToOverflow_ThrowsArgumentOutOfRangeException() {
            var result = Enumerable.Range(int.MaxValue - 3, 5);
        }

        [Test]
        public void Range_ValidArguments_CorrectOutput() {
            var result = Enumerable.Range(10, 5);
            result.Compare(10, 11, 12, 13, 14);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Repeat_PassNegativeCount_ThrowsArgumentOutOfRangeException() {
            var result = Enumerable.Repeat("Hello World", -2);
        }

        [Test]
        public void Repeat_ValidArguments_CorrectResult() {
            var result = Enumerable.Repeat("Hello World", 2);
            result.Compare("Hello World", "Hello World");
        }

        [Test]
        public void Reverse_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5});
            source.Reverse().Compare(5, 4, 3, 2, 1);
        }

        [Test]
        public void Select_ValidArguments_CorrectResult() {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            persons.Select(p => p.Age).Compare(21, 22, 23, 24);
        }

        [Test]
        public void SelectSelectorWith3Args_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {0, 1, 2, 3});
            source.Select((int i, int index) => { return i*index; }).Compare(0, 1, 4, 9);
        }

        [Test]
        public void SelectManySelector_ValidArguments_CorrectOutput() {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var result = persons.SelectMany(p => p.FirstName.ToCharArray());
            var check = "PeterHerbertHubertIsidor".ToCharArray();
            int count = 0;
            foreach (var c in result) {
                Assert.That(c, Is.EqualTo(check[count]));
                count++;
            }
        }

        class PetOwner {
            public string Name { get; set; }
            public List<string> Pets { get; set; }
        }

        [Test]
        public void SelectManySelectorWith3Arguments_ValidArguments_CorrectOutput() {
            var petOwners = new OnceEnumerable<PetOwner>( new PetOwner[] 
                { new PetOwner { Name="Higa, Sidney", 
                      Pets = new List<string>{ "Scruffy", "Sam" } },
                  new PetOwner { Name="Ashkenazi, Ronen", 
                      Pets = new List<string>{ "Walker", "Sugar" } },
                  new PetOwner { Name="Price, Vernette", 
                      Pets = new List<string>{ "Scratches", "Diesel" } },
                  new PetOwner { Name="Hines, Patrick", 
                      Pets = new List<string>{ "Dusty" } } });
            IEnumerable<string> result =
                petOwners.SelectMany((petOwner, index) =>
                             petOwner.Pets.Select(pet => index + pet));
            result.Compare("0Scruffy", "0Sam", "1Walker", "1Sugar", "2Scratches", "2Diesel", "3Dusty");
        }

        [Test]
        public void SelectManyThirdOverload_ValidArguments_CorrectOutput() {
            var petOwners = new OnceEnumerable<PetOwner>( new PetOwner[]
                { new PetOwner { Name="Higa", 
                      Pets = new List<string>{ "Scruffy", "Sam" } },
                  new PetOwner { Name="Ashkenazi", 
                      Pets = new List<string>{ "Walker", "Sugar" } },
                  new PetOwner { Name="Price", 
                      Pets = new List<string>{ "Scratches", "Diesel" } },
                  new PetOwner { Name="Hines", 
                      Pets = new List<string>{ "Dusty" } } });
            var result = petOwners.SelectMany(petOwner => petOwner.Pets, (petOwner, petName) => new {petOwner.Name, petName});

            // compare result with result from Microsoft implementation
            StringBuilder sb = new StringBuilder();
            foreach (var s in result) {
                sb.Append(s.ToString());
            }
            Assert.That(sb.ToString(), Is.EqualTo("{ Name = Higa, petName = Scruffy }{ Name = Higa, petName = Sam }{ Name = Ashkenazi, petName = Walker }{ Name = Ashkenazi, petName = Sugar }{ Name = Price, petName = Scratches }{ Name = Price, petName = Diesel }{ Name = Hines, petName = Dusty }"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SequenceEqual_NullAsArgument_ThrowsArgumentNullException() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3});
            source.SequenceEqual(null);
        } 

        [Test]
        public void SequenceEqual_EqualArgument_ResultIsTrue() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3});
            var argument = new OnceEnumerable<int>(new int[] {1, 2, 3});
            Assert.That(source.SequenceEqual(argument), Is.True);
        }

        [Test]
        public void SequenceEqual_DifferentArgument_ResultIsFalse() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3});
            var argument = new OnceEnumerable<int>(new int[] {1, 2, 3, 4});
            Assert.That(source.SequenceEqual(argument), Is.False);
        }

        class FloatComparer : IEqualityComparer<float> {
            public bool Equals(float x, float y) {
                return Math.Abs(x - y) < 0.1f;
            }
            public int GetHashCode(float x) {
                return -1;
            }
        }

        [Test]
        public void SequenceEqualComparer_ValidArguments_ComparerIsUsed() {
            var source = new OnceEnumerable<float>(new float[] {1f, 2f, 3f});
            var argument = new OnceEnumerable<float>(new float[] {1.03f, 1.99f, 3.02f});
            Assert.That(source.SequenceEqual<float>(argument, new FloatComparer()), Is.True);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Single_EmptySource_ThrowsInvalidOperationException() {
            var source = new int[0];
            source.Single();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Single_SourceWithMoreThanOneElement_ThrowsInvalidOperationException() {
            var source = new int[] {3, 6};
            source.Single();
        }

        [Test]
        public void Single_SourceWithOneElement_SingleElementIsReturned() {
            var source = new int[] {1};
            Assert.That(source.Single(), Is.EqualTo(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SinglePredicate_PassNullAsPredicate_ThrowsArgumentNullException() {
            var source = new int[] {1, 2, 3};
            source.Single(null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SinglePredicate_NoElementSatisfiesCondition_ThrowsInvalidOperationException() {
            var source = new int[] {1, 3, 5};
            source.Single(i => i%2 == 0);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SinglePredicate_MoreThanOneElementSatisfiedCondition_ThrowsInvalidOperationException() {
            var source = new int[] {1, 2, 3, 4};
            source.Single(i => i%2 == 0);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SinglePredicate_SourceIsEmpty_ThrowsInvalidOperationException() {
            var source = new int[0];
            source.Single(i => i%2 == 0);
        }

        [Test]
        public void SinglePredicate_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3});
            Assert.That(source.Single(i => i % 2 == 0), Is.EqualTo(2));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleOrDefault_MoreThanOneElementInSource_ThrowsInvalidOperationException() {
            var source = new int[] {1, 2, 3};
            source.SingleOrDefault();
        }

        [Test]
        public void SingleOrDefault_EmptySource_0IsReturned() {
            var source = new int[0];
            Assert.That(source.SingleOrDefault(), Is.EqualTo(0));
        }

        [Test]
        public void SingleOrDefault_SourceWithOneElement_SingleElementIsReturned() {
            var source = new int[] {5};
            Assert.That(source.SingleOrDefault(), Is.EqualTo(5));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SingleOrDefaultPredicate_PassNullAsPredicate_ThrowsArgumentNullException() {
            var source = new int[] {4};
            source.SingleOrDefault(null);
        }

        [Test]
        public void SingleOrDefaultPredicate_EmptySource_ZeroIsReturned() {
            var source = new int[0];
            Assert.That(source.SingleOrDefault(i => i % 2 == 0), Is.EqualTo(0));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleOrDefaultPredicate_MoreThanOneElementSatisfiesCondition_ThrowsInvalidOperationException() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5});
            source.SingleOrDefault(i => i%2 == 0);
        }

        [Test]
        public void SingleOrDefaultPredicate_NoElementSatisfiesCondition_ZeroIsReturned() {
            var source = new OnceEnumerable<int>(new int[] {1, 3, 5});
            Assert.That(source.SingleOrDefault(i => i % 2 == 0), Is.EqualTo(0));
        }

        [Test]
        public void SingleOrDefaultPredicate_OneElementSatisfiesCondition_CorrectElementIsReturned() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3});
            Assert.That(source.SingleOrDefault(i => i%2 == 0), Is.EqualTo(2));
        }

        [Test]
        public void Skip_ValidArguments_CorrectOutput() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            source.Skip(5).Compare(6, 7, 8, 9, 10);
        }

        [Test]
        public void Skip_PassNegativeValueAsCount_SameBehaviorAsMicrosoftImplementation() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5});
            source.Skip(-5).Compare(1, 2, 3, 4, 5);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SkipWhile_PassNullAsPredicate_ThrowsArgumentNullException() {
            var source = new int[] {1, 2, 3, 4, 5};
            Func<int, bool> predicate = null;
            source.SkipWhile(predicate);
        }

        [Test]
        public void SkipWhile_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5});
            source.SkipWhile(i => i < 3).Compare(3, 4, 5);
        }

        [Test]
        public void SkipWhile_ValidArguments_CorrectResult2() {
            var source = new OnceEnumerable<int>(new int[] { 1, 2, 3, 4, 5, 1, 2, 3});
            source.SkipWhile(i => i < 3).Compare(3, 4, 5, 1, 2, 3);
        }

        [Test]
        public void SkipWhile_PredicateAlwaysTrue_EmptyResult() {
            var source = new OnceEnumerable<int>(new int[] { 1, 2, 3 });
            var result = source.SkipWhile(i => true);
            Assert.That(result.GetEnumerator().MoveNext(), Is.False);
        }

        [Test]
        public void SkipWhilePredicateWith3Arguments_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9});
            source.SkipWhile((i, index) => index < 5).Compare(6, 7, 8, 9);
        }

        [Test]
        [ExpectedException(typeof(OverflowException))]
        public void Sum_SumOfArgumentsCausesOverflow_ThrowsOverflowException() {
            var source = new OnceEnumerable<int>(new int[] {int.MaxValue - 1, 2});
            source.Sum();
        }

        [Test]
        public void Sum_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            Assert.That(source.Sum(), Is.EqualTo(55));
        }

        [Test]
        public void SumNullableInts_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int?>(new int?[] {1, 2, null});
            Assert.That(source.Sum(), Is.EqualTo(3));
        }

        [Test]
        public void SumSelector_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<string>(new string[] {"dog", "cat", "eagle"});
            Assert.That(source.Sum(s => s.Length), Is.EqualTo(11));
        }

        [Test]
        public void Take_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5, 6});
            source.Take(3).Compare(1, 2, 3);
        }

        [Test]
        public void Take_CountBiggerThanList() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5});
            source.Take(10).Compare(1, 2, 3, 4, 5);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TakeWhile_PassNullAsPredicate_ThrowsArgumentNullException() {
            var source = new int[] {1, 2, 3, 4, 5};
            Func<int, bool> func = null;
            source.TakeWhile(func);
        }

        [Test]
        public void TakeWhile_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            source.TakeWhile(i => i*i < 50).Compare(1, 2, 3, 4, 5, 6, 7);
        }

        [Test]
        public void ToArray_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new List<int>() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            var result = source.ToArray();
            Assert.That(result, Is.TypeOf(typeof (int[])));
            result.Compare(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToDictionary_KeySelectorProducesNullKey_ThrowsArgumentNullException() {
            var source = new string[] {"eagle", "deer"};
            source.ToDictionary<string, string>(s => null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ToDictionary_DuplicateKeys_ThrowsArgumentException() {
            var source = new string[] {"eagle", "deer", "cat", "dog"};
            source.ToDictionary(s => s.Length);
        }

        [Test]
        public void ToDictionary_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<string>(new string[] {"1", "2", "3"});
            var result = source.ToDictionary(s => int.Parse(s));
            int check = 1;
            foreach (var pair in result) {
                Assert.That(pair.Key, Is.EqualTo(check));
                Assert.That(pair.Value, Is.EqualTo(check.ToString()));
                check++;
            }
            Assert.That(check, Is.EqualTo(4));
        }

        [Test]
        public void ToDictionaryElementSelector_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            var result = source.ToDictionary(i => i.ToString(), i => Math.Sqrt(double.Parse(i.ToString())));
            int check = 1;
            foreach (var pair in result) {
                Assert.That(pair.Key, Is.EqualTo(check.ToString()));
                Assert.That(pair.Value, Is.EqualTo(Math.Sqrt(double.Parse(check.ToString()))).Within(0.00001));
                check++;
            }
        }

        [Test]
        public void ToList_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            var result = source.ToList();
            Assert.That(result, Is.TypeOf(typeof(List<int>)));
            result.Compare(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        }

        [Test]
        public void ToLookup_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<string>(new string[] {"eagle", "dog", "cat", "bird", "camel"});
            var result = source.ToLookup(s => s.Length);

            result[3].Compare("dog", "cat");
            result[4].Compare("bird");
            result[5].Compare("eagle", "camel");
        }

        [Test]
        public void ToLookupElementSelector_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<string>(new string[] { "eagle", "dog", "cat", "bird", "camel" });
            var result = source.ToLookup(s => s.Length, str => str.ToCharArray().Reverse());
            var enumerator = result[3].GetEnumerator();
            enumerator.MoveNext(); Assert.That(enumerator.Current.ToString(), Is.EqualTo("dog".ToCharArray().Reverse().ToString()));
            enumerator.MoveNext(); Assert.That(enumerator.Current.ToString(), Is.EqualTo("cat".ToCharArray().Reverse().ToString()));
            Assert.That(enumerator.MoveNext(), Is.False);

            enumerator = result[4].GetEnumerator();
            enumerator.MoveNext(); Assert.That(enumerator.Current.ToString(), Is.EqualTo("bird".ToCharArray().Reverse().ToString()));
            Assert.That(enumerator.MoveNext(), Is.False);

            enumerator = result[5].GetEnumerator();
            enumerator.MoveNext(); Assert.That(enumerator.Current.ToString(), Is.EqualTo("eagle".ToCharArray().Reverse().ToString()));
            enumerator.MoveNext(); Assert.That(enumerator.Current.ToString(), Is.EqualTo("camel".ToCharArray().Reverse().ToString()));
            Assert.That(enumerator.MoveNext(), Is.False);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Union_PassNullAsArgument_ThrowsArgumentNullException() {
            var source = new int[] {5, 3, 9, 7, 5, 9, 3, 7};
            source.Union(null);
        }

        [Test]
        public void Union_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] { 5, 3, 9, 7, 5, 9, 3, 7 });
            var argument = new OnceEnumerable<int>(new int[] {8, 3, 6, 4, 4, 9, 1, 0});
            source.Union(argument).Compare(5, 3, 9, 7, 8, 6, 4, 1, 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Where_PassNullAsPredicate_ThrowsArgumentNullException() {
            var source = new int[] {1, 2, 3, 4};
            Func<int, bool> func = null; 
            source.Where(func);
        }

        [Test]
        public void Where_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<int>(new int[] {1, 2, 3, 4, 5});
            source.Where(i => i%2 == 0).Compare(2, 4);
        }

        [Test]
        public void WherePredicateWith3Arguments_ValidArguments_CorrectResult() {
            var source = new OnceEnumerable<string>(new string[] {"Camel", "Marlboro", "Parisienne", "Lucky Strike"});
            source.Where((s, i) => i%2 == 0).Compare("Camel", "Parisienne");
        }

        private sealed class Reader<T> {                    
            private readonly IEnumerator<T> e;
            
            public Reader(IEnumerable<T> values) {
                Debug.Assert(values != null);
                e = values.GetEnumerator();
            }

            /// <summary>Returns true if there are no more elements in the collection. </summary>
            public void AssertEnded() {
                NUnit.Framework.Assert.That(e.MoveNext(), Is.False, "Too many elements in source.");
            }

            /// <returns>Next element in collection.</returns>
            /// <exception cref="InvalidOperationException" if there are no more elements.<exception>
            public T Read() {
                if (!e.MoveNext())
                    throw new InvalidOperationException("No more elements in the source sequence.");
                return e.Current;
            }

            /// <param name="constraint">Checks constraint for the next element.</param>
            /// <returns>Itself</returns>
            public Reader<T> Next(Constraint constraint) {
                Debug.Assert(constraint != null);
                NUnit.Framework.Assert.That(Read(), constraint);
                return this;
            }

            /// <summary>
            /// Checks first constraint for first elements, second constraint for second element...
            /// </summary>
            /// <param name="constraints"></param>
            /// <returns></returns>
            public Reader<T> Ensure(params Constraint[] constraints) {
                Debug.Assert(constraints != null);
                foreach (var constraint in constraints) {
                    Next(constraint);
                }
                return this;
            }

            public void Compare(params T[] lst) {
                foreach (var t1 in lst) {
                    Next(Is.EqualTo(t1));
                }
                AssertEnded();
            }
        }

        [Test]
        public void EnsureTest() {
            var enumerable = new int[] {1, 2, 3};
            Reader<int> rd = new Reader<int>(enumerable);
            rd.Ensure(Is.EqualTo(1), Is.EqualTo(2), Is.EqualTo(3));
        }

        [Test]
        public void CompareTest() {
            var enumerable = new int[] {1, 2, 3};
            var reader = new Reader<int>(enumerable);
            reader.Compare(1, 2, 3);
        }
    }
}
