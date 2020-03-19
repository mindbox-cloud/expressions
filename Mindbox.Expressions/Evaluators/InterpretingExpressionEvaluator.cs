using System.Linq.Expressions;

namespace Mindbox.Expressions
{
	public class InterpretingExpressionEvaluator : IExpressionEvaluator
	{
		public object Evaluate(Expression expression)
		{
			var interpretationResult = EvaluationScope.Empty.TryEvaluate(expression);
			return interpretationResult.IsImpossible
				? CompilingExpressionEvaluator.Instance.Evaluate(expression)
				: interpretationResult.Value;
		}

		public static InterpretingExpressionEvaluator Instance { get; } = new InterpretingExpressionEvaluator();
	}
}