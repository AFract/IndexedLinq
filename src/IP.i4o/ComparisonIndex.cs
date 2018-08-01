using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNetProjects.IndexedLinq
{
	public class ComparisonIndex<TChild, TProperty> : IIndex<TChild>
			where TProperty : IComparable
	{
		private readonly SortedList<TProperty, List<TChild>> _index = new SortedList<TProperty, List<TChild>>();
		private readonly PropertyReader<TChild> _propertyReader;

		public ComparisonIndex(IEnumerable<TChild> collectionToIndex, PropertyInfo property)
		{
			_propertyReader = new PropertyReader<TChild>(property.Name);

			collectionToIndex.ForEach(Add);
		}

		public IEnumerator<TChild> GetEnumerator()
		{
			return _index.Values.SelectMany(list => list).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(TChild item)
		{
			var propValue = (TProperty)_propertyReader.ReadValue(item);

			int index = _index.IndexOfKey(propValue);
			if (index >= 0)
			{
				_index[propValue].Add(item);
			}
			else
			{
				_index.Add(propValue, new List<TChild> { item });
			}
		}

		public void Clear()
		{
			_index.Clear();
		}

		public bool Contains(TChild item)
		{
			var propValue = (TProperty)_propertyReader.ReadValue(item);
			return _index.ContainsKey(propValue);
		}

		public void CopyTo(TChild[] array, int arrayIndex)
		{
			var listOfAll = this.ToList();
			listOfAll.CopyTo(array, arrayIndex);
		}

		public bool Remove(TChild item)
		{
			var propValue = (TProperty)_propertyReader.ReadValue(item);

			int index = _index.IndexOfKey(propValue);
			if (index >= 0)
			{
				return _index[propValue].Remove(item);
			}

			return false;
		}

		public int Count
		{
			get { return this.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public IEnumerable<TChild> WhereThroughIndex(Expression<Func<TChild, bool>> predicate)
		{
			if (!(predicate.Body is BinaryExpression))
				throw new NotSupportedException();
			var equalityExpression = predicate.Body as BinaryExpression;
			if (equalityExpression == null) throw new NullReferenceException();
			var rightSide = Expression.Lambda(equalityExpression.Right);
			var valueToCheck = (TProperty)rightSide.Compile().DynamicInvoke(null);
			switch (equalityExpression.NodeType)
			{
				case ExpressionType.Equal:
					return WhereEqualTo(valueToCheck);

				case ExpressionType.LessThan:
					return _index.WhereLessThan(valueToCheck).SelectMany(prop => _index[prop]);

				case ExpressionType.GreaterThan:
					return _index.WhereGreaterThan(valueToCheck).SelectMany(prop => _index[prop]);

				case ExpressionType.LessThanOrEqual:
					return _index.WhereLessThanOrEqualTo(valueToCheck).SelectMany(prop => _index[prop]);

				case ExpressionType.GreaterThanOrEqual:
					return _index.WhereGreaterThanOrEqualTo(valueToCheck).SelectMany(prop => _index[prop]);

				default:
					throw new NotImplementedException("Unsupported operation");
			}
		}

		public void Reset(TChild changedObject)
		{
			Remove(changedObject);
			Add(changedObject);
		}

		private IEnumerable<TChild> WhereEqualTo(TProperty valueToCheck)
		{
			List<TChild> result;
			if (_index.TryGetValue(valueToCheck, out result))
			{
				return result;
			}

			return Enumerable.Empty<TChild>();
		}
	}
}