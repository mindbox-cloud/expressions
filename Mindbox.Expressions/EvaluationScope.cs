using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Mindbox.Expressions
{
	public class EvaluationScope
	{
		public static EvaluationScope Empty { get; } = new EvaluationScope();

		public object TryEvaluate(Expression expression)
		{
			switch (expression)
			{
				case MethodCallExpression methodCallExpression:
					return EvaluateMethod(methodCallExpression);
				case MemberExpression memberExpression:
					switch (memberExpression.Member)
					{
						case FieldInfo fieldInfo when fieldInfo.IsStatic:
							return fieldInfo.GetValue(null);
						case FieldInfo fieldInfo:
							return fieldInfo.GetValue(TryEvaluate(memberExpression.Expression));
						case PropertyInfo propertyInfo when propertyInfo.GetMethod.IsStatic:
							return propertyInfo.GetValue(null);
						case PropertyInfo propertyInfo:
							return propertyInfo.GetValue(TryEvaluate(memberExpression.Expression));
					}

					break;
				case ConstantExpression constantExpression:
					return constantExpression.Value;
			}

			return CachingCompilingExpressionEvaluator.Instance.Evaluate(expression);
		}

		private object EvaluateMethod(MethodCallExpression methodCallExpression)
		{
			var argumentsCount = methodCallExpression.Arguments.Count;
			var arguments = argumentsCount == 0 ? Array.Empty<object>() : new object[argumentsCount];

			for (var index = 0; index < methodCallExpression.Arguments.Count; index++)
			{
				var argument = methodCallExpression.Arguments[index];
				var argumentValue = TryEvaluate(argument);

				arguments[index] = argumentValue;
			}

			if (methodCallExpression.Method.IsStatic)
				return methodCallExpression.Method.Invoke(null, arguments);

			var target = TryEvaluate(methodCallExpression.Object);
			return methodCallExpression.Method.Invoke(target, arguments);
		}
	}
}