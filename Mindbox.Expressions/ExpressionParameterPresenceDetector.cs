using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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
			if (node == null)
				throw new ArgumentNullException("node");

			if (!allowedParameters.Contains(node))
				doesExpressionHaveParameters = true;

			return base.VisitParameter(node);
		}

#if NET40 || SL4 || CORE45 || WP8 || WINDOWS_PHONE_APP || PORTABLE36 || PORTABLE328
		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			allowedParameters.AddRange(node.Parameters);
			var result = base.VisitLambda(node);
			foreach (var parameter in node.Parameters)
				allowedParameters.Remove(parameter);
			return result;
		}
#else
		protected override Expression VisitLambda(LambdaExpression node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			allowedParameters.AddRange(node.Parameters);
			var result = base.VisitLambda(node);
			foreach (var parameter in node.Parameters)
				allowedParameters.Remove(parameter);
			return result;
		}
#endif
	}
}
