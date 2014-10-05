using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mindbox.Expressions
{
	internal sealed class ExpressionParameterPresenceDetector : ExpressionVisitor
	{
		public static bool DoesExpressionHaveParameters(Expression expression)
		{
			var detector = new ExpressionParameterPresenceDetector();
			detector.Visit(expression);
			return detector.doesExpressionHaveParameters;
		}


		private ExpressionParameterPresenceDetector() { }


		private bool doesExpressionHaveParameters;
		private readonly List<ParameterExpression> allowedParameters = new List<ParameterExpression>();


		protected override Expression VisitParameter(ParameterExpression node)
		{
			if (!allowedParameters.Contains(node))
				doesExpressionHaveParameters = true;

			return base.VisitParameter(node);
		}

		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			allowedParameters.AddRange(node.Parameters);
			var result = base.VisitLambda(node);
			foreach (var parameter in node.Parameters)
				allowedParameters.Remove(parameter);
			return result;
		}
	}
}
