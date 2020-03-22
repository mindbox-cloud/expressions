using System;

namespace Mindbox.Expressions
{
	public static class ExpressionsConfiguration
	{
		public static Func<IExpressionEvaluator> ExpressionEvaluatorFactory { get; set; } =
			() => CachingCompilingExpressionEvaluator.Instance;
	}
}