using System;
using System.Collections.Generic;

namespace DotNetProjects.IndexedLinq
{
	public static class EnumerableExtensions
	{
		internal static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if (sequence == null) throw new ArgumentNullException("sequence");
			if (action == null) throw new ArgumentNullException("action");

			foreach (var item in sequence)
			{
				action(item);
			}
		}
	}
}