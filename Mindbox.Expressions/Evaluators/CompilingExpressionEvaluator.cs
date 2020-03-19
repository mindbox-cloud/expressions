using System.Linq.Expressions;

namespace Mindbox.Expressions
{
	public class CompilingExpressionEvaluator : IExpressionEvaluator
	{
		public object Evaluate(Expression expression)
		{
			return Expression.Lambda(expression).Compile()
				.DynamicInvoke();
		}

		public static CompilingExpressionEvaluator Instance { get; } = new CompilingExpressionEvaluator();
	}
}