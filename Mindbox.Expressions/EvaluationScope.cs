using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mindbox.Expressions
{
	public class EvaluationScope
	{
		private readonly IDictionary<string, object> variables;
		private readonly EvaluationScope parent;

		public static EvaluationScope Empty { get; } = new EvaluationScope(new Dictionary<string, object>(), null);

		public EvaluationScope(IDictionary<string, object> variables, EvaluationScope parent)
		{
			this.variables = variables;
			this.parent = parent;
		}

		public EvaluationResult TryEvaluate(Expression expression)
		{
			switch (expression)
			{
				case MethodCallExpression methodCallExpression:
				{
					return EvaluateMethod(methodCallExpression);
				}
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
				case UnaryExpression unaryExpression when unaryExpression.NodeType == ExpressionType.Convert:
				{
					return TryEvaluate(unaryExpression.Operand);
				}
				case UnaryExpression unaryExpression:
				{
					if (unaryExpression.Method == null) return EvaluationResult.Impossible;
					if (!unaryExpression.Method.IsStatic) return EvaluationResult.Impossible;

					var operand = TryEvaluate(unaryExpression.Operand);
					if (operand.IsImpossible) return EvaluationResult.Impossible;

					return EvaluationResult.Of(unaryExpression.Method.Invoke(null, new [] { operand.Value }));
				}
				case InvocationExpression invocationExpression:
				{
					return EvaluateInvocation(invocationExpression);
				}
				case LambdaExpression lambdaExpression:
				{
					return EvaluateLambda(lambdaExpression);
				}
				case BinaryExpression binaryExpression:
				{
					if (binaryExpression.Method == null) return EvaluationResult.Impossible;
					if (!binaryExpression.Method.IsStatic) return EvaluationResult.Impossible;

					var left = TryEvaluate(binaryExpression.Left);
					if (left.IsImpossible) return EvaluationResult.Impossible;

					var right = TryEvaluate(binaryExpression.Right);
					if (right.IsImpossible) return EvaluationResult.Impossible;

					return EvaluationResult.Of(
						binaryExpression.Method.Invoke(null, new [] { left.Value, right.Value }));
				}
				case ParameterExpression parameterExpression:
				{
					if (!LookupParameter(parameterExpression.Name, out var parameterValue))
						throw new InvalidOperationException($"Parameter {parameterExpression.Name} not found");

					return EvaluationResult.Of(parameterValue);
				}
				case NewArrayExpression newArrayExpression:
				{
					var elementType = newArrayExpression.Type.GetElementType();
					if (elementType == null)
						return EvaluationResult.Impossible;


					var array = Array.CreateInstance(elementType, newArrayExpression.Expressions.Count);
					for (var i = 0; i < array.Length; i++)
					{
						var element = TryEvaluate(newArrayExpression.Expressions[i]);
						if (element.IsImpossible)
							return EvaluationResult.Impossible;
						
						array.SetValue(element.Value, i);
					}

					return EvaluationResult.Of(array);
				}
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

		private EvaluationResult EvaluateInvocation(InvocationExpression invocationExpression)
		{
			var argumentsCount = invocationExpression.Arguments.Count;
			var arguments = argumentsCount == 0 ? Array.Empty<object>() : new object[argumentsCount];

			for (var index = 0; index < invocationExpression.Arguments.Count; index++)
			{
				var argument = invocationExpression.Arguments[index];
				var argumentValue = TryEvaluate(argument);
				if (argumentValue.IsImpossible)
					return EvaluationResult.Impossible;

				arguments[index] = argumentValue.Value;
			}

			var expressionEvaluationResult = TryEvaluate(invocationExpression.Expression);
			if (expressionEvaluationResult.IsImpossible)
				return EvaluationResult.Impossible;

			var result = ((Delegate) expressionEvaluationResult.Value).DynamicInvoke(arguments);

			return EvaluationResult.Of(result);
		}

		private EvaluationResult EvaluateLambda(LambdaExpression lambdaExpression)
		{
			if (lambdaExpression.Parameters.Count == 0)
			{
				object Func()
				{
					var interpretationResult = TryEvaluate(lambdaExpression.Body);

					return interpretationResult.IsImpossible
						? lambdaExpression.Compile().DynamicInvoke()
						: interpretationResult.Value;
				}

				return EvaluationResult.Of((Func<object>)Func);
			}

			var types = new Type[lambdaExpression.Parameters.Count + 1];
			for (var i = 0; i < lambdaExpression.Parameters.Count; i++)
			{
				types[i] = lambdaExpression.Parameters[i].Type;
			}

			types[lambdaExpression.Parameters.Count] = lambdaExpression.ReturnType;

			switch (lambdaExpression.Parameters.Count)
			{
				case 1:
				{
					var carrierType = typeof(FunctionWrapper<,>).MakeGenericType(types.ToArray());

					object Fn(object p1)
					{
						return ExecuteWithChildScope(lambdaExpression, new[] { p1 });
					}

					var carrier = (FunctionWrapper) Activator.CreateInstance(carrierType, (Func<object, object>) Fn);

					return EvaluationResult.Of(carrier.EvaluationFunction);
				}
				case 2:
				{
					var carrierType = typeof(FunctionWrapper<,,>).MakeGenericType(types.ToArray());

					object Fn(object p1, object p2)
					{
						return ExecuteWithChildScope(lambdaExpression, new[] { p1, p2 });
					}

					var carrier = (FunctionWrapper) Activator.CreateInstance(carrierType, (Func<object, object, object>) Fn);

					return EvaluationResult.Of(carrier.EvaluationFunction);
				}
			}

			return EvaluationResult.Of(lambdaExpression.Compile());
		}

		private object ExecuteWithChildScope(LambdaExpression lambdaExpression, object[] parameters)
		{
			var scope = CreateNewScope(lambdaExpression.Parameters, parameters);
			var interpretationResult = scope.TryEvaluate(lambdaExpression.Body);

			return interpretationResult.IsImpossible
				? lambdaExpression.Compile().DynamicInvoke(parameters)
				: interpretationResult.Value;
		}

		private EvaluationScope CreateNewScope(
			IReadOnlyList<ParameterExpression> parameterExpressions,
			params object[] parameters)
		{
			var dictionary = new Dictionary<string, object>();
			for (var i = 0; i < parameterExpressions.Count; i++)
			{
				dictionary[parameterExpressions[i].Name] = parameters[i];
			}
			return new EvaluationScope(dictionary, this);
		}

		protected bool LookupParameter(string name, out object value)
		{
			if (variables.TryGetValue(name, out value) ||
				(parent != null && parent.LookupParameter(name, out value))) return true;

			value = null;
			return false;
		}

		abstract class FunctionWrapper
		{
			public abstract object EvaluationFunction { get; }
		}

		class FunctionWrapper<T1, TResult> : FunctionWrapper
		{
			private readonly Func<object, object> fn;

			public FunctionWrapper(Func<object, object> fn)
			{
				this.fn = fn;
			}

			private TResult Evaluate(T1 p1)
			{
				return (TResult) fn(p1);
			}

			public override object EvaluationFunction => (Func<T1, TResult>)Evaluate;
		}

		class FunctionWrapper<T1, T2, TResult> : FunctionWrapper
		{
			private readonly Func<object, object, object> fn;

			public FunctionWrapper(Func<object, object, object> fn)
			{
				this.fn = fn;
			}

			private TResult Evaluate(T1 p1, T2 p2)
			{
				return (TResult) fn(p1, p2);
			}

			public override object EvaluationFunction => (Func<T1, T2, TResult>)Evaluate;
		}
	}

	public struct EvaluationResult
	{
		private static readonly object ImpossibleValue = new object();

		public object Value { get; private set; }

		public static EvaluationResult Impossible { get; } = new EvaluationResult { Value = ImpossibleValue };

		public static EvaluationResult Of(object result) => new EvaluationResult { Value = result };

		public bool IsImpossible => Value == Impossible.Value;
	}
}