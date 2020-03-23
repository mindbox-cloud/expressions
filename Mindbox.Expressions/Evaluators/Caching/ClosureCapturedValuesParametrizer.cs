using System;
using System.Linq.Expressions;

namespace Mindbox.Expressions
{
	internal class ClosureCapturedValuesParametrizer : ClosureCapturedValuesVisitor
	{
		public static Expression GetParametrizedExpression(
			Expression expression,
			ParameterExpression arrayOfValues)
		{
			var visitor = new ClosureCapturedValuesParametrizer(arrayOfValues);
			return visitor.Visit(expression);
		}

		private readonly ParameterExpression arrayOfValues;

		private int visitedIndex = 0;

		private ClosureCapturedValuesParametrizer(
			ParameterExpression arrayOfValues)
		{
			this.arrayOfValues = arrayOfValues;
		}

		protected override Expression TryProcessClosure(ConstantExpression node)
		{
			return CreateIndexExpression(node);
		}

		private UnaryExpression CreateIndexExpression(ConstantExpression node)
		{
			var indexExpression = Expression.Convert(
				Expression.ArrayIndex(arrayOfValues, Expression.Constant(visitedIndex)),
				node.Type);
			visitedIndex++;
			return indexExpression;
		}
	}
}