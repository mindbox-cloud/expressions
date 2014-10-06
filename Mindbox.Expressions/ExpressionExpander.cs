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

		private static readonly string CompileMethodName =
			ReflectionExpressions.GetMethodName<Expression<Func<object>>>(expression => expression.Compile());


		public static Expression ExpandExpression(Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");

			return new ExpressionExpander().Visit(expression);
		}


		private static LambdaExpression TryGetLambdaExpressionFromExpression(Expression expression)
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

		private static bool IsCompileMethod(MethodInfo method)
		{
			return (method.DeclaringType != null) &&
				method.DeclaringType.IsConstructedGenericType &&
				(method.DeclaringType.GetGenericTypeDefinition() == typeof(Expression<>)) &&
				(method.Name == CompileMethodName);
		}


		private ExpressionExpander() { }


		protected override Expression VisitInvocation(InvocationExpression node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			var baseResult = (InvocationExpression)base.VisitInvocation(node);

			if (baseResult.Expression.NodeType == ExpressionType.Call)
			{
				var methodCallExpression = (MethodCallExpression)baseResult.Expression;

				if (IsCompileMethod(methodCallExpression.Method))
				{
					Expression result;
					if (TrySubstituteExpression(methodCallExpression.Object, baseResult.Arguments, out result))
						return result;
				}
			}

			return baseResult;
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			var baseResult = (MethodCallExpression)base.VisitMethodCall(node);

			if (IsEvaluateMethod(baseResult.Method))
			{
				Expression result;
				if (TrySubstituteExpression(baseResult.Arguments[0], baseResult.Arguments.Skip(1).ToList(), out result))
					return result;
			}

			if ((baseResult.Method.DeclaringType != null) &&
				(baseResult.Method.DeclaringType.BaseType == typeof(MulticastDelegate)) && 
				(baseResult.Method.Name == InvokeMethodName) &&
				(baseResult.Object != null) &&
				(baseResult.Object.NodeType == ExpressionType.Call))
			{
				var methodCallExpression = (MethodCallExpression)baseResult.Object;

				if (IsCompileMethod(methodCallExpression.Method))
				{
					Expression result;
					if (TrySubstituteExpression(methodCallExpression.Object, baseResult.Arguments, out result))
						return result;
				}
			}

			if (baseResult.Method.Equals(CreateDelegateMethod) && (baseResult.Object.NodeType == ExpressionType.Constant))
			{
				var constantExpression = (ConstantExpression)baseResult.Object;
				if (IsEvaluateMethod((MethodInfo)constantExpression.Value))
				{
					var innerExpression = TryGetLambdaExpressionFromExpression(baseResult.Arguments[1]);
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
			if (node == null)
				throw new ArgumentNullException("node");

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


		private bool TrySubstituteExpression(
			Expression expressionExpression, 
			IReadOnlyList<Expression> arguments, 
			out Expression result)
		{
			if (expressionExpression == null)
				throw new ArgumentNullException("expressionExpression");
			if (arguments == null)
				throw new ArgumentNullException("arguments");

			var lambdaExpression = TryGetLambdaExpressionFromExpression(expressionExpression);
			if (lambdaExpression != null)
			{
				if (lambdaExpression.Parameters.Count != arguments.Count)
					throw new ArgumentException("Argument count doesn't match parameter count.");

				var visitedLambdaExpression = (LambdaExpression)Visit(lambdaExpression);

				var parameterSubstitutions = new Dictionary<ParameterExpression, Expression>();
				for (var parameterIndex = 0;
					parameterIndex < visitedLambdaExpression.Parameters.Count;
					parameterIndex++)
				{
					var originalParameter = visitedLambdaExpression.Parameters[parameterIndex];
					var replacedParameter = arguments[parameterIndex];
					parameterSubstitutions.Add(originalParameter, replacedParameter);
				}

				result = Visit(ExpressionParameterSubstitutor.SubstituteParameters(
					visitedLambdaExpression.Body,
					parameterSubstitutions));
				return true;
			}

			result = default(Expression);
			return false;
		}
	}
}
