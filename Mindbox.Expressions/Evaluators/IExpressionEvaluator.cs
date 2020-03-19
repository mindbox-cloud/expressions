using System.Linq.Expressions;

namespace Mindbox.Expressions
{
	public interface IExpressionEvaluator
	{
		object Evaluate(Expression expression);
	}
}