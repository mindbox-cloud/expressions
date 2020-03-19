using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Mindbox.Expressions
{
	public class EvaluationScope
	{
		public static EvaluationScope Empty { get; } = new EvaluationScope();

		public EvaluationResult TryEvaluate(Expression expression)
		{
			switch (expression)
			{
				case MethodCallExpression methodCallExpression:
					return EvaluateMethod(methodCallExpression);
				case MemberExpression memberExpression:
					switch (memberExpression.Member)
					{
						case FieldInfo fieldInfo when fieldInfo.IsStatic:
							return EvaluationResult.Of(fieldInfo.GetValue(null));
						case FieldInfo fieldInfo:
						{
							var target = TryEvaluate(memberExpression.Expression);
							return target.IsImpossible
								? EvaluationResult.Impossible
								: EvaluationResult.Of(fieldInfo.GetValue(target.Value));
						}
						case PropertyInfo propertyInfo when propertyInfo.GetMethod.IsStatic:
							return EvaluationResult.Of(propertyInfo.GetValue(null));
						case PropertyInfo propertyInfo:
						{
							var target = TryEvaluate(memberExpression.Expression);
							return target.IsImpossible
								? EvaluationResult.Impossible
								: EvaluationResult.Of(propertyInfo.GetValue(target.Value));
						}
					}

					break;
				case ConstantExpression constantExpression:
					return EvaluationResult.Of(constantExpression.Value);
			}

			return EvaluationResult.Impossible;
		}

		private EvaluationResult EvaluateMethod(MethodCallExpression methodCallExpression)
		{
			var argumentsCount = methodCallExpression.Arguments.Count;
			var arguments = argumentsCount == 0 ? Array.Empty<object>() : new object[argumentsCount];

			for (var index = 0; index < methodCallExpression.Arguments.Count; index++)
			{
				var argument = methodCallExpression.Arguments[index];
				var argumentValue = TryEvaluate(argument);
				if (argumentValue.IsImpossible)
					return EvaluationResult.Impossible;

				arguments[index] = argumentValue.Value;
			}

			if (methodCallExpression.Method.IsStatic)
				return EvaluationResult.Of(methodCallExpression.Method.Invoke(null, arguments));

			var target = TryEvaluate(methodCallExpression.Object);
			return target.IsImpossible
				? EvaluationResult.Impossible
				: EvaluationResult.Of(methodCallExpression.Method.Invoke(target.Value, arguments));
		}
	}
}