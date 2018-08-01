using System;
using System.Linq.Expressions;

namespace DotNetProjects.IndexedLinq
{
	internal static class ExpressionExtensions
	{
		internal static string GetMemberName<T, TProperty>(this Expression<Func<T, TProperty>> propertyExpression)
		{
			return ((MemberExpression)(propertyExpression.Body)).Member.Name;
		}
	}
}