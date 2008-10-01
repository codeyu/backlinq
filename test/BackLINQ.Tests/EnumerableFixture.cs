﻿#region License, Terms and Author(s)
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
    using System.Globalization;
    using System.Text;
    using NUnit.Framework;
    using System.Linq;
    using System.Diagnostics;
    using Constraint = NUnit.Framework.Constraints.Constraint;
    using NUnit.Framework.SyntaxHelpers;

    #endregion

    [TestFixture]
    public sealed class EnumerableFixture
    {
        private CultureInfo initialCulture; // Thread culture saved during Setup to be undone in TearDown.

        [SetUp]
        public void SetUp()
        {
            initialCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-CH");
        }

        [TearDown]
        public void TearDown()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = initialCulture;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Aggregate_EmptySource_ThrowsInvalidOperationException()
        {
            var source = new OnceEnumerable<int>(new int[0]);
            source.Aggregate((a, b) => { throw new NotImplementedException(); });
        }

        [Test]
        public void Aggregate_AddFuncOnIntegers_ReturnsTotal()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var result = source.Aggregate((a, b) => a + b);
            Assert.That(result, Is.EqualTo(55));
        }

        [Test]
        public void Aggregate_AddFuncOnIntegersWithSeed_ReturnsTotal()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var result = source.Aggregate(100, (a, b) => a + b);
            Assert.That(result, Is.EqualTo(155));
        }

        [Test]
        public void Empty_YieldsEmptySource()
        {
            var source = Enumerable.Empty<String>();
            Assert.That(source, Is.Not.Null);
            var e = source.GetEnumerator();
            Assert.That(e, Is.Not.Null);
            Assert.That(e.MoveNext(), Is.False);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Cast_NullSource_ThrowsArgumentNullException()
        {
            Enumerable.Cast<object>(null);
        }

        [Test]
        [ExpectedException(typeof(InvalidCastException))]
        public void Cast_InvalidSource_ThrowsInvalidCastException()
        {
            // .........................................V----V Needed for Mono (CS0029)
            var source = new OnceEnumerable<object>(new object[] { 1000, "hello", new object() });
            var target = source.Cast<byte>();
            // do something with the results so Cast will really be executed (deferred execution)
            var sb = new StringBuilder();
            foreach (var b in target)
            {
                sb.Append(b.ToString());
            }
        }

        [Test]
        public void Cast_ObjectSourceContainingIntegers_YieldsDowncastedIntegers()
        {
            var source = new OnceEnumerable<object>(new List<object> { 1, 10, 100 });
            source.Cast<int>().Compare(1, 10, 100);
        }

        [Test]
        public void Cast_Integers_YieldsUpcastedObjects()
        {
            new OnceEnumerable<int>(new[] { 1, 10, 100 }).Cast<object>().Compare(1, 10, 100);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void All_NullSource_ThrowsArgumentNullException()
        {
            Enumerable.All(null, (int i) => { throw new NotImplementedException(); });
        }

        [Test]
        public void All_SomeSourceElementsNotSatifyingPredicate_ReturnsFalse()
        {
            var source = new OnceEnumerable<int>(new[] { -100, -1, 0, 1, 100 });
            Assert.That(source.All(i => i >= 0), Is.False);
        }

        [Test]
        public void All_SourceElementsSatisfyingPredicate_ReturnsTrue()
        {
            var source = new OnceEnumerable<int>(new[] { -100, -1, 0, 1, 100 });
            Assert.That(source.All(i => i >= -100), Is.True);
        }

        [Test]
        public void Any_EmptySource_ReturnsFalse()
        {
            var source = new object[0];
            Assert.That(source.Any(), Is.False);
        }

        [Test]
        public void Any_NonEmptySource_ReturnsTrue()
        {
            var source = new OnceEnumerable<object>(new[] { new object() });
            Assert.That(source.Any(), Is.True);
        }

        [Test]
        public void Any_PredicateArg_EmptySource_ReturnsFalse()
        {
            var source = new OnceEnumerable<int>(new int[0]);
            Assert.That(source.Any(i => { throw new NotImplementedException(); }), Is.False);
        }

        [Test]
        public void Any_PredicateArg_NonEmptySource_ReturnsTrue()
        {
            var func = new Func<int, bool>(i => i > 0);
            var list = new List<int>(0);
            var source = new OnceEnumerable<int>(list);
            Assert.That(source.Any(func), Is.False);
            list.Add(100);
            source = new OnceEnumerable<int>(list);
            Assert.That(source.Any(func), Is.True);
        }

        [Test]
        public void Average_Decimals_ReturnsToleratableAverage()
        {
            var source = new OnceEnumerable<decimal>(new List<decimal> { -10000, 2.0001m, 50 });
            Assert.That(source.Average(), Is.EqualTo(-3315.999966).Within(0.00001));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Concat_FirstSourceNull_ThrowsArgumentNullException()
        {
            Enumerable.Concat(null, new object[0]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Concat_SecondSourceNull_ThrowsArgumentNullException()
        {
            new object[0].Concat(null);
        }

        [Test]
        public void Concat_TwoLists_CorrectOrder()
        {
            var first = new OnceEnumerable<int>(new[] { 1, 2, 3 });
            var second = new OnceEnumerable<int>(new[] { 4, 5, 6 });
            first.Concat(second).Compare(1, 2, 3, 4, 5, 6);
        }

        [Test]
        public void DefaultIfEmpty_Inegers_YieldsIntegersInOrder()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3 });
            source.DefaultIfEmpty(1).Compare(1, 2, 3);
        }

        [Test]
        public void DefaultIfEmpty_EmptyIntegerArray_ReturnsZero()
        {
            var source = new OnceEnumerable<int>(new int[0]);
            source.DefaultIfEmpty().Compare(0);
        }

        [Test]
        public void DefaultIfEmpty_DefaultValueArg_EmptyIntegerArray_ReturnsDefault()
        {
            var source = new OnceEnumerable<int>(new int[0]);
            source.DefaultIfEmpty(5).Compare(5);
        }

        [Test]
        public void DefaultIfEmpty_DefaultValueArg_Integers_YieldsIntegersInOrder()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3 });
            source.DefaultIfEmpty(5).Compare(1, 2, 3);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Distinct_NullSource_ThrowsArgumentNullException()
        {
            Enumerable.Distinct<object>(null);
        }

        [Test]
        public void Distinct_IntegersWithSomeDuplicates_YieldsIntegersInSourceOrderWithoutDuplicates()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 2, 3, 4, 4, 4, 4, 5 });
            source.Distinct().Compare(1, 2, 3, 4, 5);
        }

        [Test]
        public void Distinct_MixedSourceStringsWithCaseIgnoringComparer_YieldsFirstCaseOfEachDistinctStringInSourceOrder()
        {
            var source = new OnceEnumerable<string>("Foo Bar BAZ BaR baz FOo".Split());
            source.Distinct(StringComparer.InvariantCultureIgnoreCase).Compare("Foo", "Bar", "BAZ");
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ElementAt_IndexOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            var source = new OnceEnumerable<int>(new[] { 3, 5, 7 });
            source.ElementAt(3);
        }

        [Test]
        public void ElementAt_Integers_ReturnsCorrectValues()
        {
            var source = new[] { 15, 2, 7 };
            Assert.That(new OnceEnumerable<int>(source).ElementAt(0), Is.EqualTo(15));
            Assert.That(new OnceEnumerable<int>(source).ElementAt(1), Is.EqualTo(2));
            Assert.That(new OnceEnumerable<int>(source).ElementAt(2), Is.EqualTo(7));
        }

        [Test]
        public void ElementAtOrDefault_Integers_ReturnsZeroIfIndexOutOfRange()
        {
            var source = new OnceEnumerable<int>(new[] { 3, 6, 8 });
            Assert.That(source.ElementAtOrDefault(3), Is.EqualTo(0));
        }

        [Test]
        public void ElementAtOrDefault_IntArray_ReturnsCorrectValue()
        {
            var source = new OnceEnumerable<int>(new[] { 3, 6, 9 });
            Assert.That(source.ElementAtOrDefault(2), Is.EqualTo(9));
        }

        [Test]
        public void ElementAtOrDefault_ObjectArray_ReturnsNullIfIndexOutOfRange()
        {
            var source = new OnceEnumerable<object>(new[] { new object(), new object() });
            Assert.That(source.ElementAtOrDefault(2), Is.EqualTo(null));
        }

        [Test]
        public void ElementAtOrDefault_ObjectArray_ReturnsCorrectValue()
        {
            var first = new object();
            var source = new OnceEnumerable<object>(new[] { first, new object() });
            Assert.That(source.ElementAt(0), Is.EqualTo(first));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Except_secondArg_ArgumentNull_ThrowsArgumentNullException()
        {
            new object[0].Once().Except(null);
        }

        [Test]
        public void Except_secondArg_ValidArgument_ReturnsCorrectEnumerable()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var argument = new OnceEnumerable<int>(new[] { 1, 3, 5, 7, 9 });
            source.Except(argument).Compare(2, 4, 6, 8, 10);
        }

        [Test]
        public void Except_secondArgComparerArg_ComparerIsUsed()
        {
            var source = new OnceEnumerable<string>(new[] { "albert", "john", "simon" });
            var argument = new OnceEnumerable<string>(new[] { "ALBERT" });
            source.Except(argument, StringComparer.CurrentCultureIgnoreCase).Compare("john", "simon");
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void First_EmptyArray_ThrowsInvalidOperationException()
        {
            var source = new int[0];
            source.First();
        }

        [Test]
        public void First_IntArray_ReturnsFirst()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4 });
            Assert.That(source.First(), Is.EqualTo(1));
        }

        [Test]
        public void First_PredicateArg_IntArray_PredicateIsUsed()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4 });
            Assert.That(source.First(i => i % 2 == 0), Is.EqualTo(2));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void First_PredicateArg_NoElementMatches_InvalidOperationExceptionIsThrown()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4 });
            Assert.That(source.First(i => i > 5), Is.EqualTo(0));
        }

        [Test]
        public void FirstOrDefault_EmptyBoolArray_FalseIsReturned()
        {
            var source = new OnceEnumerable<bool>(new bool[0]);
            Assert.That(source.FirstOrDefault(), Is.False);
        }

        [Test]
        public void FirstOrDefault_ObjectArray_FirstIsReturned()
        {
            var first = new object();
            var source = new OnceEnumerable<object>(new[] { first, new object() });
            Assert.That(source.FirstOrDefault(), Is.EqualTo(first));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FirstOrDefault_PredicateArg_NullAsPredicate_ArgumentNullExceptionIsThrown()
        {
            var source = new[] { 3, 5, 7 };
            source.FirstOrDefault(null);
        }

        [Test]
        public void FirstOrDefault_PredicateArg_ValidPredicate_FirstMatchingItemIsReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 4, 8 });
            Assert.That(source.FirstOrDefault(i => i % 2 == 0), Is.EqualTo(4));
        }

        [Test]
        public void FirstOrDefault_PredicateArg_NoMatchesInArray_DefaultValueOfTypeIsReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 4, 6 });
            Assert.That(source.FirstOrDefault(i => i > 10), Is.EqualTo(0));
        }

        private class Person
        {
            public string FirstName { get; set; }
            public string FamilyName { get; set; }
            public int Age { get; set; }
            public static Person[] CreatePersons()
            {
                return new[]
                           {
                               new Person {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                               new Person {FamilyName = "Müller", FirstName = "Herbert", Age = 22},
                               new Person {FamilyName = "Meier", FirstName = "Hubert", Age = 23},
                               new Person {FamilyName = "Meier", FirstName = "Isidor", Age = 24}   
                           };
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupBy_KeySelectorArg_NullAsKeySelector_ThrowsArgumentNullException()
        {
            new object[0].Once().GroupBy<object, object>(null);
        }

        [Test]
        public void GroupBy_KeySelectorArg_ValidArguments_CorrectGrouping()
        {
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

        private static T[] ToArray<T>(IEnumerable<T> source)
        {
            return new List<T>(source).ToArray();
        }

        [Test]
        // TODO: make better
        public void GroupBy_KeySelectorArg_ValidArguments_CorrectCaseSensitiveGrouping()
        {
            var persons = new OnceEnumerable<Person>(new[]
                                 {
                                     new Person {FamilyName = "Müller", FirstName = "Peter"},
                                     new Person {FamilyName = "müller", FirstName = "Herbert"},
                                     new Person {FamilyName = "Meier", FirstName = "Hubert"},
                                     new Person {FamilyName = "meier", FirstName = "Isidor"}
                                 });
            var result = persons.GroupBy(person => person.FamilyName);
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
        public void GroupBy_KeySelectorArgComparerArg_KeysThatDifferInCasingNonCaseSensitiveStringComparer_CorrectGrouping()
        {
            var persons = new OnceEnumerable<Person>(new[]
                                 {
                                     new Person {FamilyName = "Müller", FirstName = "Peter"},
                                     new Person {FamilyName = "müller", FirstName = "Herbert"},
                                     new Person {FamilyName = "Meier", FirstName = "Hubert"},
                                     new Person {FamilyName = "meier", FirstName = "Isidor"}
                                 });
            var result = persons.GroupBy(person => person.FamilyName, StringComparer.InvariantCultureIgnoreCase);
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
        public void GroupBy_KeySelectorArgElementSelectorArg_ValidArguments_CorrectGroupingAndProjection()
        {
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
        public void GroupBy_KeySelectorArgResultSelectorArg_ValidArguments_CorrectGroupingProcessing()
        {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());

            IEnumerable<int> result = persons.GroupBy(person => person.FamilyName,
                                                (key, group) =>
                                                {
                                                    int ageSum = 0;
                                                    foreach (Person p in group)
                                                    {
                                                        ageSum += p.Age;
                                                    }
                                                    return ageSum;
                                                }
                                                );
            result.Compare(43, 47);
        }

        [Test]
        public void GroupByKey_KeySelectorArgElementSelectorArgComparerArg_ValidArguments_CorrectGroupingAndProcessing()
        {
            var persons = new OnceEnumerable<Person>(new[]
                                 {
                                     new Person {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });

            IEnumerable<IGrouping<string, int>> result = persons.GroupBy(person => person.FamilyName,
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
        public void GroupBy_KeySelectorArgElementSelectorArgResultSelectorArg_ValidArguments_CorrectGroupingAndTransforming()
        {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var result = persons.GroupBy(p => p.FamilyName, p => p.Age,
                                                                      (name, enumerable2) =>
                                                                      {
                                                                          int totalAge = 0;
                                                                          foreach (var i in enumerable2)
                                                                          {
                                                                              totalAge += i;
                                                                          }
                                                                          return totalAge;
                                                                      });
            result.Compare(43, 47);
        }

        [Test]
        public void GroupBy_KeySelectorArgResultSelectorArgComparerArg_ValidArguments_CorrectGroupingAndTransforming()
        {
            var persons = new OnceEnumerable<Person>(new[]
                                 {
                                     new Person {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });
            var result = persons.GroupBy(p => p.FamilyName,
                                                                      (name, enumerable2) =>
                                                                      {
                                                                          int totalAge = 0;
                                                                          foreach (var i in enumerable2)
                                                                          {
                                                                              totalAge += i.Age;
                                                                          }
                                                                          return totalAge;
                                                                      },
                                                                      StringComparer.CurrentCultureIgnoreCase);
            result.Compare(43, 47);
        }

        [Test]
        public void GroupBy_KeySelectorArgElementSelectorArgResultSelectorArgComparerArg_ValidArguments_CorrectGroupingAndTransforming()
        {
            var persons = new OnceEnumerable<Person>(new[]
                                 {
                                     new Person {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });
            var result = persons.GroupBy(p => p.FamilyName, p => p.Age,
                                                                      (name, enumerable2) =>
                                                                      {
                                                                          int totalAge = 0;
                                                                          foreach (var i in enumerable2)
                                                                          {
                                                                              totalAge += i;
                                                                          }
                                                                          return totalAge;
                                                                      }, StringComparer.CurrentCultureIgnoreCase);
            result.Compare(43, 47);
        }

        class Pet
        {
            public string Name { get; set; }
            public string Owner { get; set; }
        }

        [Test]
        public void GroupJoin_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArg_ValidArguments_CorrectGroupingAndJoining()
        {
            var persons = new OnceEnumerable<Person>(new[]
                                 {
                                     new Person {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });

            var pets = new OnceEnumerable<Pet>(new[]
                           {
                               new Pet {Name = "Barley", Owner = "Peter"},
                               new Pet {Name = "Boots", Owner = "Herbert"},
                               new Pet {Name = "Whiskers", Owner = "Herbert"},
                               new Pet {Name = "Daisy", Owner = "Isidor"}
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
        public void GroupJoin_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArgComparerArg_ValidArguments_CorrectGroupingAndJoining()
        {
            var persons = new OnceEnumerable<Person>(new[]
                                 {
                                     new Person {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });

            var pets = new OnceEnumerable<Pet>(new[]
                           {
                               new Pet {Name = "Barley", Owner = "Peter"},
                               new Pet {Name = "Boots", Owner = "Herbert"},
                               new Pet {Name = "Whiskers", Owner = "herbert"}, // This pet is not associated to "Herbert"
                               new Pet {Name = "Daisy", Owner = "Isidor"}
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
        public void GroupJoin_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArg_PassNullAsInner_ThrowsArgumentNullException()
        {
            var persons = new OnceEnumerable<Person>(new[]
                                 {
                                     new Person {FamilyName = "Müller", FirstName = "Peter", Age = 21},
                                     new Person {FamilyName = "müller", FirstName = "Herbert", Age = 22},
                                     new Person {FamilyName = "Meier", FirstName = "Hubert", Age= 23},
                                     new Person {FamilyName = "meier", FirstName = "Isidor", Age = 24}
                                 });

            var pets = new OnceEnumerable<Pet>(new[]
                           {
                               new Pet {Name = "Barley", Owner = "Peter"},
                               new Pet {Name = "Boots", Owner = "Herbert"},
                               new Pet {Name = "Whiskers", Owner = "Herbert"},
                               new Pet {Name = "Daisy", Owner = "Isidor"}
                           });

            persons.GroupJoin(pets, null, pet => pet.Owner,
                              (person, petCollection) =>
                              new { OwnerName = person.FirstName, Pets = petCollection.Select(pet => pet.Name) });
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Intersect_SecondArg_PassNullAsArgument_ThrowsArgumentNullException()
        {
            new object[0].Once().Intersect(null);
        }

        [Test]
        public void Intersect_SecondArg_ValidArguments_ReturnsIntersection()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3 });
            var argument = new OnceEnumerable<int>(new[] { 2, 3, 4 });
            source.Intersect(argument).Compare(2, 3);
        }

        [Test]
        public void Intersect_SecondArgComparerArg_ValidArguments_EqualityComparerIsUsed()
        {
            var enumerable = new OnceEnumerable<string>(new[] { "Heinrich", "Hubert", "Thomas" });
            var argument = new OnceEnumerable<string>(new[] { "Heinrich", "hubert", "Joseph" });
            var result = enumerable.Intersect(argument, StringComparer.CurrentCultureIgnoreCase);
            result.Compare("Heinrich", "Hubert");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Join_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArg_PassNullAsArgument_ThrowsArgumentNullException()
        {
            new object[0].Once().Join<object, object, object, object>(null, null, null, null);
        }

        [Test]
        public void Join_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArg_PassingPetsAndOwners_PetsAreCorrectlyAssignedToOwners()
        {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var pets = new OnceEnumerable<Pet>(new[]
                           {
                               new Pet {Name = "Barley", Owner = "Peter"},
                               new Pet {Name = "Boots", Owner = "Herbert"},
                               new Pet {Name = "Whiskers", Owner = "Herbert"},
                               new Pet {Name = "Daisy", Owner = "Isidor"}
                           });
            var result = persons.Join(pets, aPerson => aPerson.FirstName, aPet => aPet.Owner,
                         (aPerson, aPet) => new { Owner = aPerson.FirstName, Pet = aPet.Name });

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
        public void Join_InnerArgOuterKeySelectorArgInnerKeySelectorArgResultSelectorArgComparerArg_PetOwnersNamesCasingIsInconsistent_CaseInsensitiveJoinIsPerformed()
        {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var pets = new OnceEnumerable<Pet>(new[]
                           {
                               new Pet {Name = "Barley", Owner = "Peter"},
                               new Pet {Name = "Boots", Owner = "Herbert"},
                               new Pet {Name = "Whiskers", Owner = "herbert"},
                               new Pet {Name = "Daisy", Owner = "Isidor"}
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
        public void Last_Integers_ReturnsLastElement()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3 });
            Assert.That(source.Last(), Is.EqualTo(3));
        }

        [Test]
        public void Last_IntegerListOptimization_ReturnsLastElementWithoutEnumerating()
        {
            var source = new NonEnumerableList<int>(new[] { 1, 2, 3 });
            Assert.That(source.Last(), Is.EqualTo(3));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Last_EmptyIntegerListOptimization_ThrowsInvalidOperationException()
        {
            new NonEnumerableList<int>().Last();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Last_PredicateArg_NullAsPredicate_ThrowsArgumentNullException()
        {
            new object[0].Once().Last(null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Last_PredicateArg_NoMatchingElement_ThrowsInvalidOperationException()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5 });
            source.Last(i => i > 10);
        }

        [Test]
        public void Last_PredicateArg_ListOfInts_LastMatchingElementIsReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5 });
            Assert.That(source.Last(i => i % 2 == 0), Is.EqualTo(4));
        }

        [Test]
        public void LastOrDefault_EmptySource_ZeroIsReturned()
        {
            var source = new OnceEnumerable<int>(new int[0]);
            Assert.That(source.LastOrDefault(), Is.EqualTo(0));
        }

        [Test]
        public void LastOrDefault_NonEmptyList_LastElementIsReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5 });
            Assert.That(source.LastOrDefault(), Is.EqualTo(5));
        }

        [Test]
        public void LastOrDefault_PredicateArg_ValidArguments_LastMatchingElementIsReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5 });
            Assert.That(source.LastOrDefault(i => i % 2 == 0), Is.EqualTo(4));
        }

        [Test]
        public void LastOrDefault_PredicateArg_NoMatchingElement_ZeroIsReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 3, 5, 7 });
            Assert.That(source.LastOrDefault(i => i % 2 == 0), Is.EqualTo(0));
        }

        [Test]
        public void LongCount_ValidArgument_ReturnsCorrectNumberOfElements()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 4, 7, 10 });
            Assert.That(source.LongCount(), Is.EqualTo(4));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LongCount_PredicateArg_NullAsPredicate_ThrowsArgumentNullException()
        {
            new object[0].Once().LongCount(null);
        }

        [Test]
        public void LongCount_PredicateArg_ValidArguments_ReturnsCorrectNumerOfMatchingElements()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5 });
            Assert.That(source.LongCount(i => i % 2 == 0), Is.EqualTo(2));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Max_EmptyList_ThrowsInvalidOperationException()
        {
            var source = new OnceEnumerable<int>(new int[0]);
            source.Max();
        }

        [Test]
        public void Max_ListOfInts_MaxValueIsReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1000, 203, -9999 });
            Assert.That(source.Max(), Is.EqualTo(1000));
        }

        [Test]
        public void Max_ListWithNullableType_ReturnsMaximum()
        {
            var source = new OnceEnumerable<int?>(new int?[] { 1, 4, null, 10 });
            Assert.That(source.Max(), Is.EqualTo(10));
        }

        [Test]
        public void Max_NullableList_ReturnsMaxNonNullValue()
        {
            var source = new OnceEnumerable<int?>(new int?[] { -5, -2, null });
            Assert.That(source.Max(), Is.EqualTo(-2));
        }

        [Test]
        public void Max_SelectorArg_ListOfObjects_MaxSelectedValueIsReturned()
        {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            // .....................V-----------------V Needed for Mono (CS0121)
            Assert.That(persons.Max((Func<Person, int>)(p => p.Age)), Is.EqualTo(24));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Min_EmptyList_ThrowsInvalidOperationException()
        {
            var source = new OnceEnumerable<int>(new int[0]);
            source.Min();
        }

        [Test]
        [Ignore("Pending resolution of issue #8 (http://code.google.com/p/backlinq/issues/detail?id=8).")]
        public void Min_ListWithNullables_MinimumNonNullValueIsReturned()
        {
            var source = new OnceEnumerable<int?>(new int?[] { 199, 15, null, 30 });
            Assert.That(source.Min(), Is.EqualTo(15));
        }

        [Test]
        [Ignore("Pending resolution of issue #8 (http://code.google.com/p/backlinq/issues/detail?id=8).")]
        public void Min_Selector_ValidArguments_MinimumNonNullValueIsReturned()
        {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            Assert.That(persons.Min<Person, int?>(p =>
            {
                if (p.Age == 21) return null; // to test behavior if null belongs to result of transformation
                else return (p.Age);
            }), Is.EqualTo(22));
        }

        [Test]
        public void OfType_EnumerableWithElementsOfDifferentTypes_OnlyDecimalsAreReturned()
        {
            // .........................................V----V Needed for Mono (CS0029)
            var source = new OnceEnumerable<object>(new object[] { 1, "Hello", 1.234m, new object() });
            var result = source.OfType<decimal>();
            result.Compare(1.234m);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrderBy_KeySelectorArg_NullAsKeySelector_ThrowsArgumentNullException()
        {
            new object[0].Once().OrderBy<object, object>(null);
        }

        [Test]
        public void OrderBy_KeySelector_ArrayOfPersons_PersonsAreOrderedByAge()
        {
            var persons = Person.CreatePersons();
            var reversePersons = (Person[]) persons.Clone();
            Array.Reverse(reversePersons);
            var source = new OnceEnumerable<Person>(reversePersons);
            var result = source.OrderBy(p => p.Age);

            var age = 21;
            foreach (var person in result)
                Assert.That(person.Age, Is.EqualTo(age++));

            Assert.That(age, Is.EqualTo(25));
        }

        [Test]
        public void OrderBy_KeySelector_DataWithDuplicateKeys_YieldsStablySortedData()
        {
            var data = new[]
            {
                new { Number = 4, Text = "four" },
                new { Number = 4, Text = "quatre" },
                new { Number = 4, Text = "vier" },
                new { Number = 4, Text = "quattro" },
                new { Number = 1, Text = "one" },
                new { Number = 2, Text = "two" },
                new { Number = 2, Text = "deux" },
                new { Number = 3, Text = "three" },
                new { Number = 3, Text = "trois" },
                new { Number = 3, Text = "drei" },
            };

            var result = data.Once().OrderBy(e => e.Number);
            using (var e = result.GetEnumerator())
            {
                e.MoveNext(); Assert.That(e.Current.Text, Is.EqualTo("one"));
                e.MoveNext(); Assert.That(e.Current.Text, Is.EqualTo("two"));
                e.MoveNext(); Assert.That(e.Current.Text, Is.EqualTo("deux"));
                e.MoveNext(); Assert.That(e.Current.Text, Is.EqualTo("three"));
                e.MoveNext(); Assert.That(e.Current.Text, Is.EqualTo("trois"));
                e.MoveNext(); Assert.That(e.Current.Text, Is.EqualTo("drei"));
                e.MoveNext(); Assert.That(e.Current.Text, Is.EqualTo("four"));
                e.MoveNext(); Assert.That(e.Current.Text, Is.EqualTo("quatre"));
                e.MoveNext(); Assert.That(e.Current.Text, Is.EqualTo("vier"));
                e.MoveNext(); Assert.That(e.Current.Text, Is.EqualTo("quattro"));
                Assert.That(e.MoveNext(), Is.False);
            }
        }

        [Test]
        public void ThenBy_KeySelector_DataWithDuplicateKeys_YieldsStablySortedData()
        {
            var data = new[]
            {
                new { Position = 1, LastName = "Smith", FirstName = "John" },
                new { Position = 2, LastName = "Smith", FirstName = "Jack" },
                new { Position = 3, LastName = "Smith", FirstName = "John" },
                new { Position = 4, LastName = "Smith", FirstName = "Jack" },
                new { Position = 5, LastName = "Smith", FirstName = "John" },
                new { Position = 6, LastName = "Smith", FirstName = "Jack" },
            };

            var result = data.Once().OrderBy(e => e.LastName).ThenBy(e => e.FirstName);
            using (var e = result.GetEnumerator())
            {
                e.MoveNext(); Assert.That(e.Current.Position, Is.EqualTo(2));
                e.MoveNext(); Assert.That(e.Current.Position, Is.EqualTo(4));
                e.MoveNext(); Assert.That(e.Current.Position, Is.EqualTo(6));
                e.MoveNext(); Assert.That(e.Current.Position, Is.EqualTo(1));
                e.MoveNext(); Assert.That(e.Current.Position, Is.EqualTo(3));
                e.MoveNext(); Assert.That(e.Current.Position, Is.EqualTo(5));
                Assert.That(e.MoveNext(), Is.False);
            }
        }

        /// <summary>
        /// To sort ints in descending order.
        /// </summary>
        class ReverseComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return y.CompareTo(x);
            }
        }

        [Test]
        public void OrderBy_KeySelectorArgComparerArg_ArrayOfPersonsAndReversecomparer_PersonsAreOrderedByAgeUsingReversecomparer()
        {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var result = persons.OrderBy(p => p.Age, new ReverseComparer());
            var age = 25;
            foreach (var person in result)
            {
                age--;
                Assert.That(person.Age, Is.EqualTo(age));
            }
            Assert.That(age, Is.EqualTo(21));

        }

        [Test]
        public void OrderByDescending_KeySelectorArg_ArrayOfPersons_PersonsAreOrderedByAgeDescending()
        {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var result = persons.OrderByDescending(p => p.Age);
            int age = 25;
            foreach (var person in result)
            {
                age--;
                Assert.That(person.Age, Is.EqualTo(age));
            }
            Assert.That(age, Is.EqualTo(21));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Range_ProduceRangeThatLeadsToOverflow_ThrowsArgumentOutOfRangeException()
        {
            Enumerable.Range(int.MaxValue - 3, 5);
        }

        [Test]
        public void Range_Start10Count5_IntsFrom10To14()
        {
            var result = Enumerable.Range(10, 5);
            result.Compare(10, 11, 12, 13, 14);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Repeat_PassNegativeCount_ThrowsArgumentOutOfRangeException()
        {
            Enumerable.Repeat("Hello World", -2);
        }

        [Test]
        public void Repeat_StringArgumentCount2_ReturnValueContainsStringArgumentTwice()
        {
            var result = Enumerable.Repeat("Hello World", 2);
            result.Compare("Hello World", "Hello World");
        }

        [Test]
        public void Reverse_SeriesOfInts_IntsAreCorrectlyReversed()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5 });
            source.Reverse().Compare(5, 4, 3, 2, 1);
        }

        [Test]
        public void Select_ArrayOfPersons_AgeOfPersonsIsSelectedAccordingToPassedLambdaExpression()
        {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            persons.Select(p => p.Age).Compare(21, 22, 23, 24);
        }

        [Test]
        public void Select_SelectorArg_LambdaThatTakesIndexAsArgument_ReturnValueContainsElementsMultipliedByIndex()
        {
            var source = new OnceEnumerable<int>(new[] { 0, 1, 2, 3 });
            source.Select((i, index) => i * index).Compare(0, 1, 4, 9);
        }

        [Test]
        public void SelectMany_SelectorArg_ArrayOfPersons_ReturnsASequenceWithAllLettersOfFirstnames()
        {
            var persons = new OnceEnumerable<Person>(Person.CreatePersons());
            var result = persons.SelectMany(p => p.FirstName.ToCharArray());
            var check = "PeterHerbertHubertIsidor".ToCharArray();
            int count = 0;
            foreach (var c in result)
            {
                Assert.That(c, Is.EqualTo(check[count]));
                count++;
            }
        }

        class PetOwner
        {
            public string Name { get; set; }
            public List<string> Pets { get; set; }
        }

        [Test]
        public void SelectMany_Selector3Arg_ArrayOfPetOwners_SelectorUsesElementIndexArgument()
        {
            var petOwners = new OnceEnumerable<PetOwner>(new[] 
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
        public void SelectMany_CollectionSelectorArgResultSelectorArg_ArrayOfPetOwner_ResultContainsElementForEachPetAPetOwnerHas()
        {
            var petOwners = new OnceEnumerable<PetOwner>(new[]
                { new PetOwner { Name="Higa", 
                      Pets = new List<string>{ "Scruffy", "Sam" } },
                  new PetOwner { Name="Ashkenazi", 
                      Pets = new List<string>{ "Walker", "Sugar" } },
                  new PetOwner { Name="Price", 
                      Pets = new List<string>{ "Scratches", "Diesel" } },
                  new PetOwner { Name="Hines", 
                      Pets = new List<string>{ "Dusty" } } });
            var result = petOwners.SelectMany(petOwner => petOwner.Pets, (petOwner, petName) => new { petOwner.Name, petName });

            // compare result with result from Microsoft implementation
            var sb = new StringBuilder();
            foreach (var s in result)
            {
                sb.Append(s.ToString());
            }
            Assert.That(sb.ToString(), Is.EqualTo("{ Name = Higa, petName = Scruffy }{ Name = Higa, petName = Sam }{ Name = Ashkenazi, petName = Walker }{ Name = Ashkenazi, petName = Sugar }{ Name = Price, petName = Scratches }{ Name = Price, petName = Diesel }{ Name = Hines, petName = Dusty }"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SequenceEqual_SecondArg_NullAsArgument_ThrowsArgumentNullException()
        {
            new object[0].Once().SequenceEqual(null);
        }

        [Test]
        public void SequenceEqual_SecondArg_EqualArgument_ResultIsTrue()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3 });
            var argument = new OnceEnumerable<int>(new[] { 1, 2, 3 });
            Assert.That(source.SequenceEqual(argument), Is.True);
        }

        [Test]
        public void SequenceEqual_SecondArg_DifferentArgument_ResultIsFalse()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3 });
            var argument = new OnceEnumerable<int>(new[] { 1, 2, 3, 4 });
            Assert.That(source.SequenceEqual(argument), Is.False);
        }

        class FloatComparer : IEqualityComparer<float>
        {
            public bool Equals(float x, float y)
            {
                return Math.Abs(x - y) < 0.1f;
            }
            public int GetHashCode(float x)
            {
                return -1;
            }
        }

        [Test]
        public void SequenceEqual_SecondArgComparerArg_ValidArguments_ComparerIsUsed()
        {
            var source = new OnceEnumerable<float>(new[] { 1f, 2f, 3f });
            var argument = new OnceEnumerable<float>(new[] { 1.03f, 1.99f, 3.02f });
            Assert.That(source.SequenceEqual(argument, new FloatComparer()), Is.True);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Single_EmptySource_ThrowsInvalidOperationException()
        {
            var source = new int[0];
            source.Single();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Single_SourceWithMoreThanOneElement_ThrowsInvalidOperationException()
        {
            var source = new[] { 3, 6 };
            source.Single();
        }

        [Test]
        public void Single_SourceWithOneElement_SingleElementIsReturned()
        {
            var source = new[] { 1 };
            Assert.That(source.Single(), Is.EqualTo(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Single_PredicateArg_PassNullAsPredicate_ThrowsArgumentNullException()
        {
            new object[0].Once().Single(null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Single_PredicateArg_NoElementSatisfiesCondition_ThrowsInvalidOperationException()
        {
            var source = new[] { 1, 3, 5 };
            source.Single(i => i % 2 == 0);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Single_PredicateArg_MoreThanOneElementSatisfiedCondition_ThrowsInvalidOperationException()
        {
            var source = new[] { 1, 2, 3, 4 };
            source.Single(i => i % 2 == 0);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Single_PredicateArg_SourceIsEmpty_ThrowsInvalidOperationException()
        {
            var source = new int[0];
            source.Single(i => i % 2 == 0);
        }

        [Test]
        public void Single_PredicateArg_ArrayOfIntWithOnlyOneElementSatisfyingCondition_OnlyThisElementIsReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3 });
            Assert.That(source.Single(i => i % 2 == 0), Is.EqualTo(2));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleOrDefault_MoreThanOneElementInSource_ThrowsInvalidOperationException()
        {
            var source = new[] { 1, 2, 3 };
            source.SingleOrDefault();
        }

        [Test]
        public void SingleOrDefault_EmptySource_ZeroIsReturned()
        {
            var source = new int[0];
            Assert.That(source.SingleOrDefault(), Is.EqualTo(0));
        }

        [Test]
        public void SingleOrDefault_SourceWithOneElement_SingleElementIsReturned()
        {
            var source = new[] { 5 };
            Assert.That(source.SingleOrDefault(), Is.EqualTo(5));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SingleOrDefault_PredicateArg_PassNullAsPredicate_ThrowsArgumentNullException()
        {
            new object[0].Once().SingleOrDefault(null);
        }

        [Test]
        public void SingleOrDefault_PredicateArg_EmptySource_ZeroIsReturned()
        {
            var source = new int[0];
            Assert.That(source.SingleOrDefault(i => i % 2 == 0), Is.EqualTo(0));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleOrDefault_PredicateArg_MoreThanOneElementSatisfiesCondition_ThrowsInvalidOperationException()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5 });
            source.SingleOrDefault(i => i % 2 == 0);
        }

        [Test]
        public void SingleOrDefault_PredicateArg_NoElementSatisfiesCondition_ZeroIsReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 3, 5 });
            Assert.That(source.SingleOrDefault(i => i % 2 == 0), Is.EqualTo(0));
        }

        [Test]
        public void SingleOrDefault_PredicateArg_OneElementSatisfiesCondition_CorrectElementIsReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3 });
            Assert.That(source.SingleOrDefault(i => i % 2 == 0), Is.EqualTo(2));
        }

        [Test]
        public void Skip_IntsFromOneToTenAndFifeAsSecondArg_IntsFromSixToTen()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            source.Skip(5).Compare(6, 7, 8, 9, 10);
        }

        [Test]
        public void Skip_PassNegativeValueAsCount_SameBehaviorAsMicrosoftImplementation()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5 });
            source.Skip(-5).Compare(1, 2, 3, 4, 5);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SkipWhile_PredicateArg_PassNullAsPredicate_ThrowsArgumentNullException()
        {
            new object[0].Once().SkipWhile((Func<object, bool>) null);
        }

        [Test]
        public void SkipWhile_PredicateArg_IntsFromOneToFive_ElementsAreSkippedAsLongAsConditionIsSatisfied()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5 });
            source.SkipWhile(i => i < 3).Compare(3, 4, 5);
        }

        [Test]
        public void SkipWhile_PredicateArg_ArrayOfIntsWithElementsNotSatisfyingConditionAtTheEnd_IntsAtTheEndArePartOfResult()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5, 1, 2, 3 });
            source.SkipWhile(i => i < 3).Compare(3, 4, 5, 1, 2, 3);
        }

        [Test]
        public void SkipWhile_PredicateArg_PredicateAlwaysTrue_EmptyResult()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3 });
            var result = source.SkipWhile(i => true);
            Assert.That(result.GetEnumerator().MoveNext(), Is.False);
        }

        [Test]
        public void SkipWhile_Predicate3Arg_IntsFromOneToNine_ElementsAreSkippedWhileIndexLessThanFive()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            source.SkipWhile((i, index) => index < 5).Compare(6, 7, 8, 9);
        }

        [Test]
        [ExpectedException(typeof(OverflowException))]
        public void Sum_SumOfArgumentsCausesOverflow_ThrowsOverflowException()
        {
            var source = new OnceEnumerable<int>(new[] { int.MaxValue - 1, 2 });
            source.Sum();
        }

        [Test]
        public void Sum_IntsFromOneToTen_ResultIsFiftyFive()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            Assert.That(source.Sum(), Is.EqualTo(55));
        }

        [Test]
        public void Sum_NullableIntsAsArguments_CorrectSumIsReturned()
        {
            var source = new OnceEnumerable<int?>(new int?[] { 1, 2, null });
            Assert.That(source.Sum(), Is.EqualTo(3));
        }

        [Test]
        public void Sum_SelectorArg_StringArray_ResultIsSumOfStringLengthes()
        {
            var source = new OnceEnumerable<string>(new[] { "dog", "cat", "eagle" });
            // ....................V-----------------V Needed for Mono (CS0121)
            Assert.That(source.Sum((Func<string, int>)(s => s.Length)), Is.EqualTo(11));
        }

        [Test]
        public void Take_IntsFromOneToSixAndThreeAsCount_IntsFromOneToThreeAreReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5, 6 });
            source.Take(3).Compare(1, 2, 3);
        }

        [Test]
        public void Take_CountBiggerThanList_ReturnsAllElements()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5 });
            source.Take(10).Compare(1, 2, 3, 4, 5);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TakeWhile_PassNullAsPredicate_ThrowsArgumentNullException()
        {
            new object[0].TakeWhile((Func<object, bool>) null);
        }

        [Test]
        public void TakeWhile_IntsFromOneToTenAndConditionThatSquareIsSmallerThan50_IntsFromOneToSeven()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            source.TakeWhile(i => i * i < 50).Compare(1, 2, 3, 4, 5, 6, 7);
        }

        [Test]
        public void ToArray_IntsFromOneToTen_ResultIsIntArrayContainingAllElements()
        {
            var source = new OnceEnumerable<int>(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var result = source.ToArray();
            Assert.That(result, Is.TypeOf(typeof(int[])));
            result.Compare(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToDictionary_KeySelectorArg_KeySelectorProducesNullKey_ThrowsArgumentNullException()
        {
            var source = new[] { "eagle", "deer" };
            source.ToDictionary<string, string>(s => null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ToDictionary_KeySelectorArg_DuplicateKeys_ThrowsArgumentException()
        {
            var source = new[] { "eagle", "deer", "cat", "dog" };
            source.ToDictionary(s => s.Length);
        }

        [Test]
        public void ToDictionary_KeySelectorArg_ValidArguments_KeySelectorIsUsedForKeysInDictionary()
        {
            var source = new OnceEnumerable<string>(new[] { "1", "2", "3" });
            var result = source.ToDictionary(s => int.Parse(s));
            int check = 1;
            foreach (var pair in result)
            {
                Assert.That(pair.Key, Is.EqualTo(check));
                Assert.That(pair.Value, Is.EqualTo(check.ToString()));
                check++;
            }
            Assert.That(check, Is.EqualTo(4));
        }

        [Test]
        public void ToDictionary_KeySelectorArgElementSelectorArg_IntsFromOneToTen_KeySelectorAndElementSelectorAreUsedForDictionaryElements()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var result = source.ToDictionary(i => i.ToString(), i => Math.Sqrt(double.Parse(i.ToString())));
            int check = 1;
            foreach (var pair in result)
            {
                Assert.That(pair.Key, Is.EqualTo(check.ToString()));
                Assert.That(pair.Value, Is.EqualTo(Math.Sqrt(double.Parse(check.ToString()))).Within(0.00001));
                check++;
            }
        }

        [Test]
        public void ToList_IntsFromOneToTen_ListOfIntsContainingAllElementsIsReturned()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var result = source.ToList();
            Assert.That(result, Is.TypeOf(typeof(List<int>)));
            result.Compare(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        }

        [Test]
        public void ToLookup_KeySelectorArg_Strings_LookupArrayWithStringLengthAsKeyIsReturned()
        {
            var source = new OnceEnumerable<string>(new[] { "eagle", "dog", "cat", "bird", "camel" });
            var result = source.ToLookup(s => s.Length);

            result[3].Compare("dog", "cat");
            result[4].Compare("bird");
            result[5].Compare("eagle", "camel");
        }

        [Test]
        public void ToLookup_KeySelectorArgElementSelectorArg_Strings_ElementSelectorIsUsed()
        {
            var source = new OnceEnumerable<string>(new[] { "eagle", "dog", "cat", "bird", "camel" });
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
        public void Union_SecondArg_PassNullAsArgument_ThrowsArgumentNullException()
        {
            new object[0].Once().Union(null);
        }

        [Test]
        public void Union_SecondArg_ValidIntArguments_NoDuplicatesAndInSourceOrder()
        {
            var source = new OnceEnumerable<int>(new[] { 5, 3, 9, 7, 5, 9, 3, 7 });
            var argument = new OnceEnumerable<int>(new[] { 8, 3, 6, 4, 4, 9, 1, 0 });
            source.Union(argument).Compare(5, 3, 9, 7, 8, 6, 4, 1, 0);
        }

        [Test]
        public void Union_SecondArgComparerArg_UpperCaseAndLowerCaseStrings_PassedComparerIsUsed()
        {
            var source = new OnceEnumerable<string>(new[] { "A", "B", "C", "D", "E", "F" });
            var argument = new OnceEnumerable<string>(new[] { "a", "b", "c", "d", "e", "f" });
            source.Union(argument, StringComparer.CurrentCultureIgnoreCase).Compare("A", "B", "C", "D", "E", "F");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Where_PredicateArg_PassNullAsPredicate_ThrowsArgumentNullException()
        {
            new object[0].Once().Where((Func<object, bool>) null);
        }

        [Test]
        public void Where_PredicateArg_Integers_YieldsEvenIntegers()
        {
            var source = new OnceEnumerable<int>(new[] { 1, 2, 3, 4, 5 });
            source.Where(i => i % 2 == 0).Compare(2, 4);
        }

        [Test]
        public void Where_Predicate3Arg_Strings_YieldsElementsWithEvenIndex()
        {
            var source = new OnceEnumerable<string>(new[] { "Camel", "Marlboro", "Parisienne", "Lucky Strike" });
            source.Where((s, i) => i % 2 == 0).Compare("Camel", "Parisienne");
        }

        private sealed class Reader<T>
        {
            private readonly IEnumerator<T> e;

            public Reader(IEnumerable<T> values)
            {
                Debug.Assert(values != null);
                e = values.GetEnumerator();
            }

            /// <summary>Returns true if there are no more elements in the collection. </summary>
            public void AssertEnded()
            {
                Assert.That(e.MoveNext(), Is.False, "Too many elements in source.");
            }

            /// <returns>Next element in collection.</returns>
            /// <exception cref="InvalidOperationException" if there are no more elements.<exception>
            public T Read()
            {
                if (!e.MoveNext())
                    throw new InvalidOperationException("No more elements in the source sequence.");
                return e.Current;
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

        [Test]
        public void EnsureTest()
        {
            var rd = new Reader<int>(new[] { 1, 2, 3 });
            rd.Ensure(Is.EqualTo(1), Is.EqualTo(2), Is.EqualTo(3));
        }

        [Test]
        public void CompareTest()
        {
            var enumerable = new[] { 1, 2, 3 };
            var reader = new Reader<int>(enumerable);
            reader.Compare(1, 2, 3);
        }
    }

    [ Serializable ]
    internal sealed class NonEnumerableList<T> : List<T>, IEnumerable<T>
    {
        public NonEnumerableList() {}

        public NonEnumerableList(IEnumerable<T> collection) : 
            base(collection) {}

        // Re-implement GetEnumerator to be undefined.

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }
    }
}