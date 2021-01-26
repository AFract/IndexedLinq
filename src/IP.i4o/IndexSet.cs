using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNetProjects.IndexedLinq
{
	public class IndexSet<T> : IEnumerable<T>
	{
		protected readonly IndexSpecification<T> IndexSpecification;

		protected readonly Dictionary<string, IIndex<T>> IndexDictionary
				= new Dictionary<string, IIndex<T>>();

		public IndexSet(IndexSpecification<T> indexSpecification)
			: this(new List<T>(), indexSpecification)
		{
		}

		public IndexSet(IEnumerable<T> source, IndexSpecification<T> indexSpecification)
		{
			IndexSpecification = indexSpecification;
			SetupIndices(source);
		}		

		protected void SetupIndices(IEnumerable<T> source)
		{
			IndexSpecification.IndexedProperties.ForEach(
					propName =>
						IndexDictionary.Add(propName, IndexBuilder.GetIndexFor(source, typeof(T).GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)))
			);
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (IndexSpecification.IndexedProperties.Count > 0)
				return IndexDictionary[IndexSpecification.IndexedProperties[0]].GetEnumerator();
			throw new InvalidOperationException("Can't enumerate without at least one index");
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal IEnumerable<T> WhereUsingIndex(Expression<Func<T, bool>> predicate)
		{
			if (
					BodyIsBinary(predicate) &&
					BodyTypeIsEqual(predicate) &&
					LeftSideIsMemberExpression(predicate) &&
					LeftSideMemberIsIndexed(predicate)
				 )
				return IndexDictionary[LeftSide(predicate).Member.Name].WhereThroughIndex(predicate);

			// Raise event about missing or unusable index due to non supported predicate
			// TODO : verify in which case the "return IndexDictionary.First().Value.Where(predicate.Compile());" is actually invoked !
			OnUnableToUseIndex(new IndexEventArgs<T>(predicate));
			return IndexDictionary.First().Value.Where(predicate.Compile());
		}

		private static MemberExpression LeftSide(Expression<Func<T, bool>> predicate)
		{
			return ((MemberExpression)((BinaryExpression)predicate.Body).Left);
		}

		private bool LeftSideMemberIsIndexed(Expression<Func<T, bool>> predicate)
		{
			return (IndexSpecification.IndexedProperties.Contains(
					((MemberExpression)((BinaryExpression)predicate.Body).Left
					).Member.Name));
		}

		private static bool LeftSideIsMemberExpression(Expression<Func<T, bool>> predicate)
		{
			return ((((BinaryExpression)predicate.Body)).Left is MemberExpression);
		}

		private static bool BodyTypeIsEqual(Expression<Func<T, bool>> predicate)
		{
			return (predicate.Body.NodeType == ExpressionType.Equal);
		}

		private static bool BodyIsBinary(Expression<Func<T, bool>> predicate)
		{
			return (predicate.Body is BinaryExpression);
		}


		public event EventHandler<IndexEventArgs<T>> UnableToUseIndex;
		protected internal virtual void OnUnableToUseIndex(IndexEventArgs<T> e)
		{
			var handler = UnableToUseIndex;
			handler?.Invoke(this, e);
		}
	}

	public class IndexEventArgs<T> :EventArgs
    {
		public Expression<Func<T, bool>> Predicate { get; }
		public string Message { get; }
        public IndexEventArgs(Expression<Func<T, bool>> predicate)
        {
			Predicate = predicate;
			Message = "Unable to use index for predicate or missing index ! Predicate : " + Predicate?.ToString();
		}

	}
}