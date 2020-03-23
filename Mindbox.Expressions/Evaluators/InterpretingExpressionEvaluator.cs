using System.Linq.Expressions;

namespace Mindbox.Expressions
{
	public class InterpretingExpressionEvaluator : IExpressionEvaluator
	{
		public object Evaluate(Expression expression)
		{
			return EvaluationScope.Empty.TryEvaluate(expression);
		}

		public static InterpretingExpressionEvaluator Instance { get; } = new InterpretingExpressionEvaluator();
	}
}