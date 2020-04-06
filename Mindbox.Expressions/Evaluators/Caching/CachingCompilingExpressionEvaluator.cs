using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Resources;

namespace Mindbox.Expressions
{
	public class CachingCompilingExpressionEvaluator : IExpressionEvaluator
	{
		private CachingCompilingExpressionEvaluator()
		{
			
		}

		private readonly ConcurrentDictionary<string, Func<object[], object>> compiledExpressionsCache = 
			new ConcurrentDictionary<string, Func<object[], object>>();

		public object Evaluate(Expression expression)
		{
			var cacheKey = MindboxExpressionStringBuilder.ExpressionToString(expression);
			var cachedDelegate = compiledExpressionsCache.GetOrAdd(cacheKey, key =>
			{
				var arrayOfValuesParameter = Expression.Parameter(typeof(object[]));
				var parametrizedBody = Parametrize(expression, arrayOfValuesParameter);
				if (parametrizedBody.Type.IsValueType)
				{
					parametrizedBody = Expression.Convert(parametrizedBody, typeof(object));
				}

				var effectiveQuery = Expression.Lambda<Func<object[], object>>(
					parametrizedBody,
					arrayOfValuesParameter
				);

				return effectiveQuery.Compile();
			});

			var capturedValues = GetCapturedValues(expression);
			return cachedDelegate(capturedValues);
		}

		private static Expression Parametrize(
			Expression expression,
			ParameterExpression arrayOfValuesParameter)
		{
			return ClosureCapturedValuesParametrizer.GetParametrizedExpression(
				expression,
				arrayOfValuesParameter);
		}

		private static object[] GetCapturedValues(Expression expression)
		{
			return ClosureCapturedValuesProvider.GetCapturedValues(expression);
		}

		public static CachingCompilingExpressionEvaluator Instance { get; } = new CachingCompilingExpressionEvaluator();
	}
}