using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mindbox.Expressions
{
	internal sealed class ExpressionParameterSubstitutor : ExpressionVisitor
	{

		public static Expression SubstituteParameters(
			Expression expression,
			IDictionary<ParameterExpression, Expression> parameterSubstitutions)
		{
			if (parameterSubstitutions == null)
				throw new ArgumentNullException("parameterSubstitutions");

			return new ExpressionParameterSubstitutor(parameterSubstitutions).Visit(expression);
		}

		public static Expression SubstituteParameter(
			Expression expression,
			ParameterExpression oldParameter,
			Expression newParameter)
		{
			return SubstituteParameters(
				expression,
				new Dictionary<ParameterExpression, Expression>
				{
					{
						oldParameter, 
						newParameter
					}
				});
		}


		private readonly Dictionary<ParameterExpression, Expression> parameterSubstitutions;


		private ExpressionParameterSubstitutor(IDictionary<ParameterExpression, Expression> parameterSubstitutions)
		{
			if (parameterSubstitutions == null)
				throw new ArgumentNullException("parameterSubstitutions");

			this.parameterSubstitutions = new Dictionary<ParameterExpression, Expression>(parameterSubstitutions);
		}


		protected override Expression VisitParameter(ParameterExpression node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			Expression substitution;
			return parameterSubstitutions.TryGetValue(node, out substitution) ?
				SubstituteParameters(substitution, new Dictionary<ParameterExpression, Expression>()) :
				base.VisitParameter(node);
		}

#if NET40 || SL4 || CORE45 || WP8 || WINDOWS_PHONE_APP
		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			var newParameters = new List<ParameterExpression>(node.Parameters.Count);

			foreach (var oldParameter in node.Parameters)
			{
				var newParameter = Expression.Parameter(oldParameter.Type, oldParameter.Name);
				newParameters.Add(newParameter);
				parameterSubstitutions.Add(oldParameter, newParameter);
			}

			return node.Update(Visit(node.Body), newParameters);
		}
#else
		protected override Expression VisitLambda(LambdaExpression node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			var newParameters = new List<ParameterExpression>(node.Parameters.Count);

			foreach (var oldParameter in node.Parameters)
			{
				var newParameter = Expression.Parameter(oldParameter.Type, oldParameter.Name);
				newParameters.Add(newParameter);
				parameterSubstitutions.Add(oldParameter, newParameter);
			}

			return Expression.Lambda(node.Type, Visit(node.Body), newParameters);
		}
#endif
	}
}
