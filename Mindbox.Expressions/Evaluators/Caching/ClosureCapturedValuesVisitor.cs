using System;
using System.Linq.Expressions;

namespace Mindbox.Expressions
{
	internal abstract class ClosureCapturedValuesVisitor : ExpressionVisitor
	{
		protected ClosureCapturedValuesVisitor()
		{

		}

		protected sealed override Expression VisitConstant(ConstantExpression node)
		{
			if (node == null)
				throw new ArgumentNullException(nameof(node));

			if (node.Value == null)
				return base.VisitConstant(node);

			var replacedQuery = TryProcessClosure(node);
			if (replacedQuery != null)
				return replacedQuery;

			return base.VisitConstant(node);
		}

		protected abstract Expression TryProcessClosure(ConstantExpression node);
	}
}