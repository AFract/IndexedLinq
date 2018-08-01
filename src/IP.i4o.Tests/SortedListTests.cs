using System.Collections.Generic;
using DotNetProjects.IndexedLinq.Tests.TestHelpers;
using NUnit.Framework;

namespace DotNetProjects.IndexedLinq.Tests
{
	[TestFixture(Description = "SortedList Tests")]
	public class SortedListTests
	{
		private readonly ProfilingComparer<int> _comparer = new ProfilingComparer<int>();

		[Test]
		public void TryFindIndex_should_return_correct_index()
		{
			var list = SetUpTestList(_comparer);
			_comparer.ResetCount();

			int index;
			bool result;
			result = list.TryFindIndex(0, out index);
			result.ShouldBeFalse();
			index.ShouldEqual(0);
			_comparer.Count.ShouldEqual(3);
			_comparer.ResetCount();

			result = list.TryFindIndex(1, out index);
			result.ShouldBeTrue();
			index.ShouldEqual(0);
			_comparer.Count.ShouldEqual(3);
			_comparer.ResetCount();

			result = list.TryFindIndex(2, out index);
			result.ShouldBeFalse();
			index.ShouldEqual(1);
			_comparer.Count.ShouldEqual(3);
			_comparer.ResetCount();

			result = list.TryFindIndex(3, out index);
			result.ShouldBeTrue();
			index.ShouldEqual(1);
			_comparer.Count.ShouldEqual(2);
			_comparer.ResetCount();

			result = list.TryFindIndex(4, out index);
			result.ShouldBeTrue();
			index.ShouldEqual(2);
			_comparer.Count.ShouldEqual(1);
			_comparer.ResetCount();

			result = list.TryFindIndex(5, out index);
			result.ShouldBeTrue();
			index.ShouldEqual(3);
			_comparer.Count.ShouldEqual(3);
			_comparer.ResetCount();

			result = list.TryFindIndex(6, out index);
			result.ShouldBeFalse();
			index.ShouldEqual(4);
			_comparer.Count.ShouldEqual(3);
			_comparer.ResetCount();

			result = list.TryFindIndex(7, out index);
			result.ShouldBeTrue();
			index.ShouldEqual(4);
			_comparer.Count.ShouldEqual(2);
			_comparer.ResetCount();

			result = list.TryFindIndex(8, out index);
			result.ShouldBeFalse();
			index.ShouldEqual(5);
			_comparer.Count.ShouldEqual(2);
			_comparer.ResetCount();
		}

		private SortedList<int, int> SetUpTestList(IComparer<int> comparer)
		{
			var list = new SortedList<int, int>(comparer);
			list.Add(1, 1);
			list.Add(3, 3);
			list.Add(4, 4);
			list.Add(5, 5);
			list.Add(7, 7);

			return list;
		}
	}
}