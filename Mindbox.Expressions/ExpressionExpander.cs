using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mindbox.Expressions
{
	internal sealed class ExpressionExpander : ExpressionVisitor
	{
		private static readonly MethodInfo CreateDelegateMethod = ReflectionExpressions.GetMethodInfo<MethodInfo>(methodInfo =>
			methodInfo.CreateDelegate(default(Type), default(object)));

		private static readonly string EvaluateMethodName = 
			ReflectionExpressions.GetMethodName<Expression<Func<object>>>(expression => expression.Evaluate());

		private static readonly string InvokeMethodName =
			ReflectionExpressions.GetMethodName<Action>(action => action.Invoke());


		public static Expression ExpandExpression(Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");

			return new ExpressionExpander().Visit(expression);
		}


		private static LambdaExpression TryGetCompiledExpression(Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");

			object value;
			return ExpressionEvaluator.Instance.TryEvaluate(expression, out value) ? (LambdaExpression)value : null;
		}

		private static bool IsEvaluateMethod(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			return (method.DeclaringType == typeof(Extensions)) && (method.Name == EvaluateMethodName);
		}


		private ExpressionExpander() { }


		protected override Expression VisitInvocation(InvocationExpression node)
		{
			var baseResult = (InvocationExpression)base.VisitInvocation(node);

			if (baseResult.Expression.NodeType == ExpressionType.Call)
			{
				var methodCallExpression = (MethodCallExpression)baseResult.Expression;

				if ((methodCallExpression.Method.DeclaringType != null) &&
					methodCallExpression.Method.DeclaringType.IsGenericType &&
					(methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(Expression<>)) &&
					(methodCallExpression.Method.Name == "Compile"))
				{
					var innerExpression = TryGetCompiledExpression(methodCallExpression.Object);
					if (innerExpression != null)
					{
						var visitedInnerExpression = (LambdaExpression)Visit(innerExpression);

						var parameterSubstitutions = new Dictionary<ParameterExpression, Expression>();
						for (var parameterIndex = 0; 
							parameterIndex < visitedInnerExpression.Parameters.Count;
							parameterIndex++)
						{
							var originalParameter = visitedInnerExpression.Parameters[parameterIndex];
							var replacedParameter = baseResult.Arguments[parameterIndex];
							parameterSubstitutions.Add(originalParameter, replacedParameter);
						}

						return Visit(ExpressionParameterSubstitutor.SubstituteParameters(
							visitedInnerExpression.Body,
							parameterSubstitutions));
					}
				}
			}

			return baseResult;
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			var baseResult = (MethodCallExpression)base.VisitMethodCall(node);

			if (IsEvaluateMethod(baseResult.Method))
			{
				var innerExpression = TryGetCompiledExpression(baseResult.Arguments[0]);
				if (innerExpression != null)
				{
					var parameterSubstitutions = new Dictionary<ParameterExpression, Expression>();
					for (
						var parameterIndex = 0;
						parameterIndex < innerExpression.Parameters.Count;
						parameterIndex++)
					{
						var originalParameter = innerExpression.Parameters[parameterIndex];
						var replacedParameter = baseResult.Arguments[parameterIndex + 1];
						parameterSubstitutions.Add(originalParameter, replacedParameter);
					}

					return Visit(ExpressionParameterSubstitutor.SubstituteParameters(
						innerExpression.Body,
						parameterSubstitutions));
				}
			}

			if ((baseResult.Method.DeclaringType != null) &&
				(baseResult.Method.DeclaringType.BaseType == typeof(MulticastDelegate)) && 
				(baseResult.Method.Name == InvokeMethodName) &&
				(baseResult.Object != null) &&
				(baseResult.Object.NodeType == ExpressionType.Call))
			{
				var methodCallExpression = (MethodCallExpression)baseResult.Object;

				if ((methodCallExpression.Method.DeclaringType != null) &&
					methodCallExpression.Method.DeclaringType.IsGenericType &&
					(methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(Expression<>)) &&
					(methodCallExpression.Method.Name == "Compile"))
				{
					var innerExpression = TryGetCompiledExpression(methodCallExpression.Object);
					if (innerExpression != null)
					{
						var visitedInnerExpression = (LambdaExpression)Visit(innerExpression);

						var parameterSubstitutions = new Dictionary<ParameterExpression, Expression>();
						for (var parameterIndex = 0; parameterIndex < visitedInnerExpression.Parameters.Count;
							parameterIndex++)
						{
							var originalParameter = visitedInnerExpression.Parameters[parameterIndex];
							var replacedParameter = baseResult.Arguments[parameterIndex];
							parameterSubstitutions.Add(originalParameter, replacedParameter);
						}

						return Visit(ExpressionParameterSubstitutor.SubstituteParameters(
							visitedInnerExpression.Body,
							parameterSubstitutions));
					}
				}
			}

			if (baseResult.Method.Equals(CreateDelegateMethod) && (baseResult.Object.NodeType == ExpressionType.Constant))
			{
				var constantExpression = (ConstantExpression)baseResult.Object;
				if (IsEvaluateMethod((MethodInfo)constantExpression.Value))
				{
					var innerExpression = TryGetCompiledExpression(baseResult.Arguments[1]);
					if (innerExpression != null)
					{
						var parameterSubstitutions = new Dictionary<ParameterExpression, Expression>();
						var replacedParameters = new List<ParameterExpression>(innerExpression.Parameters.Count);
						foreach (var originalParameter in innerExpression.Parameters)
						{
							var replacedParameter = Expression.Parameter(originalParameter.Type, originalParameter.Name);
							parameterSubstitutions.Add(originalParameter, replacedParameter);
							replacedParameters.Add(replacedParameter);
						}

						return Expression.Lambda(
							innerExpression.Type,
							Visit(ExpressionParameterSubstitutor.SubstituteParameters(
								innerExpression.Body,
								parameterSubstitutions)),
							replacedParameters);
					}
				}
			}

			return baseResult;
		}

		protected override Expression VisitUnary(UnaryExpression node)
		{
			var baseResult = base.VisitUnary(node);
			if (baseResult.NodeType == ExpressionType.Convert)
			{
				var baseResultUnary = (UnaryExpression)baseResult;
				if ((baseResultUnary.Type == baseResultUnary.Operand.Type) &&
					(baseResultUnary.Method == null) &&
					!baseResultUnary.IsLifted &&
					!baseResultUnary.IsLiftedToNull)
					return baseResultUnary.Operand;
			}

			return baseResult;
		}
	}
}
