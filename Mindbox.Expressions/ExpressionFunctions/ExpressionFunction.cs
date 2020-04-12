using System;
using System.Linq.Expressions;

namespace Mindbox.Expressions
{
	internal class ExpressionFunction<TResult>
	{
		private readonly Expression expression;

		public ExpressionFunction(Expression expression)
		{
			this.expression = expression;
		}

		internal Delegate EvaluationFunction => (Func<TResult>) Evaluate;

		private TResult Evaluate()
		{
			return (TResult)EvaluationScope.Empty.TryEvaluate(expression);
		}
	}
}