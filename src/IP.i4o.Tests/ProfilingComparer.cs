using System.Collections.Generic;

namespace DotNetProjects.IndexedLinq.Tests
{
	internal class ProfilingComparer<T> : IComparer<T>
	{
		private readonly IComparer<T> _comparer = Comparer<T>.Default;

		public int Count { get; private set; }

		public void ResetCount()
		{
			Count = 0;
		}

		public int Compare(T x, T y)
		{
			Count++;
			return _comparer.Compare(x, y);
		}
	}
}