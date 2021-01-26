using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace DotNetProjects.IndexedLinq.Tests
{
    public class ObservableObject : INotifyPropertyChanged
    {
        private int _someMutable;

        public int SomeMutable
        {
            get { return _someMutable; }
            set
            {
                if (_someMutable == value) return;
                _someMutable = value;
                OnPropertyChanged("SomeMutable");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [TestFixture]
    public class IndexTests
    {
        [Test]
        public void IndexRecognizesChangeInAChildProperty()
        {
            var someObservableObject = new ObservableObject { SomeMutable = 6 };
            var someCollection = new ObservableCollection<ObservableObject>(
                    new List<ObservableObject> { someObservableObject });
            var indexSpec = IndexSpecification<ObservableObject>.Build()
                    .With(child => child.SomeMutable);
            var someIndex = IndexBuilder.BuildIndicesFor(someCollection, indexSpec);
            someObservableObject.SomeMutable = 3;
            Assert.AreEqual((from v in someIndex where v.SomeMutable == 3 select v).Count(), 1);
        }

        [Test]
        public void IndexRecognizesItemAddedToSourceCollection()
        {
            var someObservableObject = new ObservableObject { SomeMutable = 6 };
            var someCollection = new ObservableCollection<ObservableObject>(
                    new List<ObservableObject> { someObservableObject });
            var indexSpec = IndexSpecification<ObservableObject>.Build()
                    .With(child => child.SomeMutable);
            var someIndex = IndexBuilder.BuildIndicesFor(someCollection, indexSpec);
            someCollection.Add(new ObservableObject { SomeMutable = 3 });
            Assert.AreEqual((from v in someIndex where v.SomeMutable == 3 select v).Count(), 1);
        }

        public class SimpleClass
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public Color FavoriteColor { get; set; }
        }

        public int ResolvesToZero()
        {
            return 2 - 2;
        }

        [Test]
        public void EquatableIndexLookupResolves()
        {
            SimpleClass[] someItems = {
                                                                new SimpleClass {Name = "Jason", Age = 25},
                                                                new SimpleClass {Name = "Aaron", Age = 37}
                                                        };
            var indexOnSomeItems =
                    new EqualityIndex<SimpleClass>(
                            someItems,
                            typeof(SimpleClass).GetProperty("Age"));
            var jason = indexOnSomeItems.WhereThroughIndex(item => item.Age == 25).First();
            Assert.AreEqual("Jason", jason.Name);
        }

        [Test]
        public void EquatableIndexLookupWithComplexRightConditionResolves()
        {
            SimpleClass[] someItems = {
                                                                new SimpleClass {Name = "Jason", Age = 25},
                                                                new SimpleClass {Name = "Aaron", Age = 37}
                                                        };
            var indexOnSomeItems =
                    new EqualityIndex<SimpleClass>(
                            someItems,
                            typeof(SimpleClass).GetProperty("Age"));
            var jason = indexOnSomeItems.WhereThroughIndex(item => item.Age == (someItems[0].Age + ResolvesToZero())).First();
            Assert.AreEqual("Jason", jason.Name);
        }

        [Test]
        public void ComparableIndexLookupWithLessThan()
        {
            SimpleClass[] someItems = {
                                                                new SimpleClass {Name = "Jason", Age = 25},
                                                                new SimpleClass {Name = "Aaron", Age = 37},
                                                                new SimpleClass {Name = "Erin", Age=34},
                                                                new SimpleClass {Name = "Adriana", Age=13},
                                                        };
            var indexOnSomeItems =
                    new ComparisonIndex<SimpleClass, int>(
                            someItems,
                            typeof(SimpleClass).GetProperty("Age"));
            var youngerThan34 = indexOnSomeItems.WhereThroughIndex(item => item.Age < 34);
            Assert.AreEqual(2, youngerThan34.Count());
        }

        [Test]
        public void ComparableIndexLookupWithLessThanOrEqualTo()
        {
            SimpleClass[] someItems = {
                                                                new SimpleClass {Name = "Jason", Age = 25},
                                                                new SimpleClass {Name = "Aaron", Age = 37},
                                                                new SimpleClass {Name = "Erin", Age=34},
                                                                new SimpleClass {Name = "Adriana", Age=13},
                                                        };
            var indexOnSomeItems =
                    new ComparisonIndex<SimpleClass, int>(
                            someItems,
                            typeof(SimpleClass).GetProperty("Age"));
            var youngerThan34Or34 = indexOnSomeItems.WhereThroughIndex(item => item.Age <= 34);
            Assert.AreEqual(3, youngerThan34Or34.Count());
        }

        [Test]
        public void ComparableIndexLookupWithGreaterThan()
        {
            SimpleClass[] someItems = {
                                                                new SimpleClass {Name = "Jason", Age = 25},
                                                                new SimpleClass {Name = "Aaron", Age = 37},
                                                                new SimpleClass {Name = "Erin", Age=34},
                                                                new SimpleClass {Name = "Adriana", Age=13},
                                                        };
            var indexOnSomeItems =
                    new ComparisonIndex<SimpleClass, int>(
                            someItems,
                            typeof(SimpleClass).GetProperty("Age"));
            var olderThan34 = indexOnSomeItems.WhereThroughIndex(item => item.Age > 34);
            Assert.AreEqual(1, olderThan34.Count());
        }

        [Test]
        public void ComparableIndexLookupWithGreaterThanOrEqualTo()
        {
            SimpleClass[] someItems = {
                                                                new SimpleClass {Name = "Jason", Age = 25},
                                                                new SimpleClass {Name = "Aaron", Age = 37},
                                                                new SimpleClass {Name = "Erin", Age=34},
                                                                new SimpleClass {Name = "Adriana", Age=13},
                                                        };
            var indexOnSomeItems =
                    new ComparisonIndex<SimpleClass, int>(
                            someItems,
                            typeof(SimpleClass).GetProperty("Age"));
            var olderThan34Or34 = indexOnSomeItems.WhereThroughIndex(item => item.Age >= 34);
            Assert.AreEqual(2, olderThan34Or34.Count());
        }

        [Test]
        public void BuilderReturnsComparisonIndexForComparable()
        {
            SimpleClass[] someItems = {
                                                                new SimpleClass {Name = "Jason", Age = 25},
                                                                new SimpleClass {Name = "Aaron", Age = 37},
                                                                new SimpleClass {Name = "Erin", Age=34},
                                                                new SimpleClass {Name = "Adriana", Age=13},
                                                        };
            var theRightIndex
                    = IndexBuilder.GetIndexFor(
                            someItems,
                            typeof(SimpleClass).GetProperty("Age")
                    );
            Assert.AreEqual(typeof(ComparisonIndex<SimpleClass, int>), theRightIndex.GetType());
        }

        [Test]
        public void BuilderReturnsEqualityIndexForNotComparable()
        {
            SimpleClass[] someItems = {
                                                                new SimpleClass {Name = "Jason", Age = 25},
                                                                new SimpleClass {Name = "Aaron", Age = 37, FavoriteColor=Color.Green},
                                                                new SimpleClass {Name = "Erin", Age=34},
                                                                new SimpleClass {Name = "Adriana", Age=13},
                                                        };
            var theRightIndex
                    = IndexBuilder.GetIndexFor(
                            someItems,
                            typeof(SimpleClass).GetProperty("FavoriteColor")
                    );
            Assert.AreEqual(typeof(EqualityIndex<SimpleClass>), theRightIndex.GetType());
        }

        [Test]
        public void ComplexQuery()
        {
            SimpleClass[] someItems = {
                                                                                    new SimpleClass {Name = "Jason", Age = 25},
                                                                                    new SimpleClass {Name = "Aaron", Age = 37, FavoriteColor = Color.Green},
                                                                                    new SimpleClass {Name = "Erin", Age = 34},
                                                                                    new SimpleClass {Name = "Adriana", Age = 13},
                                                                            };
            var indexSpec = IndexSpecification<SimpleClass>.Build()
                    .With(person => person.FavoriteColor)
                    .And(person => person.Age);
            var theIndexSet = new IndexSet<SimpleClass>(someItems, indexSpec);
            var twoResults =
                    from item in theIndexSet
                    where item.FavoriteColor == Color.Green || item.Age == 13 && item.Name == "Adriana"
                    select item;
            Assert.AreEqual(2, twoResults.Count());
        }

        [Test]
        public void SuperComplexQuery()
        {
            SimpleClass[] someItems = {
                                                                                    new SimpleClass {Name = "Jason Jarett", Age = 25, FavoriteColor = Color.Aqua},
                                                                                    new SimpleClass {Name = "Aaron Erickson", Age = 37, FavoriteColor = Color.Green},
                                                                                    new SimpleClass {Name = "Erin Erickson", Age = 34, FavoriteColor = Color.Green},
                                                                                    new SimpleClass {Name = "Adriana Erickson", Age = 13, FavoriteColor = Color.Aqua},
                                                                            };
            var indexSpec = IndexSpecification<SimpleClass>.Build()
                    .With(person => person.FavoriteColor)
                    .And(person => person.Age)
                    .And(person => person.Name);
            var theIndexSet = new IndexSet<SimpleClass>(someItems, indexSpec);
            var oneResult =
                    from item in theIndexSet
                    where
                    (
                        (item.FavoriteColor == Color.Green && item.Age == 37) ||
                        (item.Name == "Adriana Erickson" && item.Age == 13) ||
                        (item.Name == "Jason Jarett" && item.Age == 25)
                    ) && item.Name == "Aaron Erickson"
                    select item;
            Assert.AreEqual(1, oneResult.Count());
        }


        [Test]
        public void PerformanceComparisonTest()
        {
            Stopwatch globalSw = new Stopwatch();
            globalSw.Start();
            List<SimpleClass> someItems = new List<SimpleClass> {
                                                                                    new SimpleClass {Name = "Jason Jarett", Age = 25, FavoriteColor = Color.Aqua},
                                                                                    new SimpleClass {Name = "Aaron Erickson", Age = 37, FavoriteColor = Color.Green},
                                                                                    new SimpleClass {Name = "Erin Erickson", Age = 34, FavoriteColor = Color.Green},
                                                                                    new SimpleClass {Name = "Adriana Erickson", Age = 13, FavoriteColor = Color.Aqua},

        };

            var rnd = new Random();

            var moreItems = Enumerable.Range(1, 100000).Select(r => new SimpleClass() { Name = "AdrBla" + rnd.NextDouble(), Age = rnd.Next(0, 100), FavoriteColor = new Color(rnd.Next(0, 0xFFFFFF)) });

            someItems.AddRange(moreItems);

            someItems = someItems.OrderBy(r => rnd.Next()).ToList(); // Shuffle items

            Assert.AreEqual(100004, someItems.Count());

            Console.WriteLine("Arrange test list : " + globalSw.ElapsedMilliseconds);

            globalSw.Reset();

            var indexSpec = IndexSpecification<SimpleClass>.Build()
                .With(person => person.Name)
                .With(person => person.Age);

            var theIndexSet = new IndexSet<SimpleClass>(someItems, indexSpec);

            theIndexSet.UnableToUseIndex += (e, args) =>
            {
                throw new Exception(args.Message);
            };

            Console.WriteLine("Define index : " + globalSw.ElapsedMilliseconds);

            globalSw.Reset();

            Stopwatch sw = new Stopwatch();

            // Search a 1000 times in index

            globalSw.Start();
            for (int i = 0; i < 1000; i++)
            {
                sw.Start();

                var inIndexSearch = theIndexSet.Where(item => item.Age == 13 && item.Name == "Adriana Erickson");
                Assert.AreEqual(1, inIndexSearch.Count());

                sw.Stop();

                if (i == 0)
                    Console.WriteLine("In index (first iteration) : " + sw.ElapsedMilliseconds);

                if (i == 1)
                    Console.WriteLine("In index (second iteration) : " + sw.ElapsedMilliseconds);

                //Console.WriteLine("In index : " + sw.ElapsedMilliseconds);

                sw.Reset();
            }

            Console.WriteLine("In index global time : " + globalSw.ElapsedMilliseconds);

            globalSw.Reset();

            sw.Reset();

            // Search a 1000 times in list

            globalSw.Start();
            for (int i = 0; i < 1000; i++)
            {
                sw.Start();
                var inListSearch = someItems.Where(item => item.Age == 13 && item.Name == "Adriana Erickson");
                Assert.AreEqual(1, inListSearch.Count());

                sw.Stop();
                //Console.WriteLine("In list : " + sw.ElapsedMilliseconds);
                sw.Reset();
            }

            Console.WriteLine("In list global time : " + globalSw.ElapsedMilliseconds);


        }
    }

    public struct Color
    {
        private readonly int _value;

        public static Color Green = new Color(0x4f);
        public static Color Aqua = new Color(30);

        public Color(int value)
            : this()
        {
            _value = value;
        }

        public static bool operator ==(Color left, Color right)
        {
            return left._value == right._value;
        }

        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }

        public bool Equals(Color other)
        {
            return other._value == _value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Color)) return false;
            return Equals((Color)obj);
        }

        public override int GetHashCode()
        {
            return _value;
        }
    }
}