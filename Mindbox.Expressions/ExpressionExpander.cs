using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Mindbox.Expressions
{
	internal sealed class ExpressionExpander : ExpressionVisitor
	{
		private static readonly MethodInfo MethodInfoCreateDelegateMethod = 
#if NET45 || SL5 || CORE45 || WP8 || WINDOWS_PHONE_APP
			ReflectionExpressions.GetMethodInfo<MethodInfo>(methodInfo => 
				methodInfo.CreateDelegate(default(Type), default(object)));
#else
			typeof(MethodInfo).GetMethod(
				"CreateDelegate",
				new[]
				{
					typeof(Type),
					typeof(object)
				});
#endif

		private static readonly MethodInfo DelegateCreateDelegateMethod =
#if NET35 || SL3 || WINDOWS_PHONE || PORTABLE36 || PORTABLE88 || PORTABLE328
			ReflectionExpressions.GetMethodInfo(() => 
				Delegate.CreateDelegate(default(Type), default(object), default(MethodInfo)));
#else
			typeof(Delegate)
				.GetTypeInfo()
				.GetDeclaredMethods("CreateDelegate")
				.SingleOrDefault(method => method
					.GetParameters()
					.Select(parameter => parameter.ParameterType)
					.SequenceEqual(new[]
					{
						typeof(Type),
						typeof(object),
						typeof(MethodInfo)
					}));
#endif

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


		private static LambdaExpression GetLambdaExpressionFromExpression(Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");

			if (expression.NodeType == ExpressionType.Quote)
				return (LambdaExpression)((UnaryExpression)expression).Operand;

			if (ExpressionParameterPresenceDetector.DoesExpressionHaveParameters(expression))
				throw new InvalidOperationException(
					"Expression isn't expandable due to usage of " +
					$"{nameof(Extensions.Evaluate)} or {nameof(LambdaExpression.Compile)} on expression, " +
					"that can't be obtained because it depends on outer lambda expression parameter.");

			// Testing showed that evaluation via compilation works faster and the result is GCed.
			var result = (LambdaExpression) Expression.Lambda(expression).Compile().DynamicInvoke();
			if (result == null)
				throw new InvalidOperationException($"Usage of {nameof(Extensions.Evaluate)} on null expression is invalid");

			return result;
		}

		private static bool IsEvaluateMethod(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			return (method.DeclaringType == typeof(Extensions)) && (method.Name == EvaluateMethodName);
		}

		private static bool IsCompileMethod(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			return (method.DeclaringType != null) &&
#if NET45 || CORE45 || WINDOWS_PHONE_APP
				method.DeclaringType.IsConstructedGenericType &&
#else
				method.DeclaringType.IsGenericType &&
				!method.DeclaringType.IsGenericTypeDefinition &&
#endif
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
					return SubstituteExpression(methodCallExpression.Object, baseResult.Arguments);
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
				return SubstituteExpression(baseResult.Arguments[0], baseResult.Arguments.Skip(1).ToList());
			}

			if ((baseResult.Method.DeclaringType != null) &&
#if NET35 || SL3 || WINDOWS_PHONE || PORTABLE36 || PORTABLE88 || PORTABLE328
				(baseResult.Method.DeclaringType.BaseType ==
#else
				(baseResult.Method.DeclaringType.GetTypeInfo().BaseType ==
#endif
					typeof(MulticastDelegate)) &&
				(baseResult.Method.Name == InvokeMethodName) &&
				(baseResult.Object != null) &&
				(baseResult.Object.NodeType == ExpressionType.Call))
			{
				var methodCallExpression = (MethodCallExpression)baseResult.Object;

				if (IsCompileMethod(methodCallExpression.Method))
				{
					return SubstituteExpression(methodCallExpression.Object, baseResult.Arguments);
				}
			}

			if ((baseResult.Method == MethodInfoCreateDelegateMethod) && (baseResult.Object.NodeType == ExpressionType.Constant))
			{
				var constantExpression = (ConstantExpression)baseResult.Object;
				if (IsEvaluateMethod((MethodInfo)constantExpression.Value))
				{
					var innerExpression = GetLambdaExpressionFromExpression(baseResult.Arguments[1]);
					if (innerExpression == null)
						throw new InvalidOperationException("innerExpression == null");

					return Visit(ExpressionParameterSubstitutor.SubstituteParameters(
						innerExpression,
						new Dictionary<ParameterExpression, Expression>()));
				}
			}

			if ((baseResult.Method == DelegateCreateDelegateMethod) && 
				(baseResult.Arguments[2].NodeType == ExpressionType.Constant))
			{
				var constantExpression = (ConstantExpression)baseResult.Arguments[2];
				if (IsEvaluateMethod((MethodInfo)constantExpression.Value))
				{
					var innerExpression = GetLambdaExpressionFromExpression(baseResult.Arguments[1]);
					if (innerExpression == null)
						throw new InvalidOperationException("innerExpression == null");

					return Visit(ExpressionParameterSubstitutor.SubstituteParameters(
						innerExpression,
						new Dictionary<ParameterExpression, Expression>()));
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

		private Expression SubstituteExpression(
			Expression expressionExpression, 
#if NET45 || CORE45 || WINDOWS_PHONE_APP
			IReadOnlyList<Expression> arguments
#else
			IList<Expression> arguments
#endif
			)
		{
			if (expressionExpression == null)
				throw new ArgumentNullException("expressionExpression");
			if (arguments == null)
				throw new ArgumentNullException("arguments");

			var lambdaExpression = GetLambdaExpressionFromExpression(expressionExpression);
			if (lambdaExpression == null)
				throw new InvalidOperationException("lambdaExpression == null");

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

			return Visit(ExpressionParameterSubstitutor.SubstituteParameters(
				visitedLambdaExpression.Body,
				parameterSubstitutions));
		}
	}
}
