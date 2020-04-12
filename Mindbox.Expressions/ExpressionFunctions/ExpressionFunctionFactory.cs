using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace Mindbox.Expressions
{
	public static class ExpressionFunctionFactory
	{
		private static readonly ConcurrentDictionary<Type, Func<Expression, Delegate>> FunctionFactories =
			new ConcurrentDictionary<Type, Func<Expression, Delegate>>();

		public static Delegate Create(Expression expression)
		{
			var type = expression.Type;
			return FunctionFactories.GetOrAdd(type, CreateExpressionFunctionFactory).Invoke(expression);
		}

		private static Func<Expression, Delegate> CreateExpressionFunctionFactory(Type expressionType)
		{
			var evaluator = typeof(ExpressionFunction<>).MakeGenericType(expressionType);
			var constructor = evaluator.GetConstructors().Single();

			var parameter = Expression.Parameter(typeof(Expression), "expression");

			var newEvaluatorExpression = Expression.New(constructor, parameter);

			var delegateGetter = Expression.Property(newEvaluatorExpression,
				nameof(ExpressionFunction<int>.EvaluationFunction));
			return (Func<Expression, Delegate>)Expression.Lambda(delegateGetter, parameter)
				.Compile();
		}
	}
}