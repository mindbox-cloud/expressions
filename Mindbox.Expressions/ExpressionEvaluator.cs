using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Text;

namespace Mindbox.Expressions
{
	/// <summary>
	/// Evaluates an <c>Expression</c> avoiding compilation when possible.
	/// Base class only handles the most frequent expression types without compilation.
	/// Derived class can be created when required and used via <c>Instance</c> property.
	/// </summary>
	public class ExpressionEvaluator
	{
		private static ExpressionEvaluator instance = new ExpressionEvaluator();


		/// <summary>
		/// Instance used by <c>Extensions.ExpandExpressions</c> method. Cannot be null.
		/// </summary>
		public static ExpressionEvaluator Instance
		{
			get { return instance; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");
				instance = value;
			}
		}


		/// <summary>
		/// Evaluates expression when possible. Avoids compilation when possible.
		/// Expressions containing parameters cannot be evaluated.
		/// </summary>
		/// <param name="expression">Expression to be evaluated. Can be null.</param>
		/// <param name="result">Expression value if successful (with or without compilation). Otherwise <c>null</c>.</param>
		/// <returns>Whether the evaluation was successful.</returns>
		public bool TryEvaluate(Expression expression, out object result)
		{
			if (TryEvaluateWithoutCompiling(expression, out result))
				return true;
			if (expression == null)
				throw new InvalidOperationException("TryEvaluateWithoutCompiling method failed for null expression.");

			if (ExpressionParameterPresenceDetector.DoesExpressionHaveParameters(expression))
				return false;

			result = Expression.Lambda(expression).Compile().DynamicInvoke();
			return true;
		}

		/// <summary>
		/// Evaluates expression. Avoids compilation when possible.
		/// </summary>
		/// <param name="expression">Expression to be evaluated. Can be null.</param>
		/// <exception cref="ArgumentException">Expression contains parameters.</exception>
		public object Evaluate(Expression expression)
		{
			object result;
			if (TryEvaluate(expression, out result))
				return result;

			throw new ArgumentException("Expression contains parameters.", "expression");
		}


		/// <summary>
		/// Tries to evaluate expression without compilation.
		/// Base implementation only handles the most frequent expression types without compilation.
		/// </summary>
		/// <param name="expression">Expression to be evaluated. Can be null.</param>
		/// <param name="result">Expression value if successful. Otherwise <c>null</c>.</param>
		/// <returns>Whether the evaluation was successful.</returns>
		protected virtual bool TryEvaluateWithoutCompiling(Expression expression, out object result)
		{
			if (expression == null)
			{
				result = null;
				return true;
			}

			switch (expression.NodeType)
			{
				case ExpressionType.Lambda:
					if (TryEvaluateLambda((LambdaExpression)expression, out result))
						return true;
					break;

				case ExpressionType.Quote:
					if (TryEvaluateQuote((UnaryExpression)expression, out result))
						return true;
					break;

				case ExpressionType.Constant:
					if (TryEvaluateConstant((ConstantExpression)expression, out result))
						return true;
					break;

				case ExpressionType.MemberAccess:
					if (TryEvaluateMemberAccess((MemberExpression)expression, out result))
						return true;
					break;

				case ExpressionType.Call:
					if (TryEvaluateCall((MethodCallExpression)expression, out result))
						return true;
					break;
			}

			result = default(object);
			return false;
		}

		/// <summary>
		/// Tries to evaluate member access expression without compilation.
		/// Base implementation only handles field access and property access.
		/// </summary>
		/// <param name="expression">Expression to be evaluated. Cannot be null.</param>
		/// <param name="result">Expression value if successful. Otherwise <c>null</c>.</param>
		/// <returns>Whether the evaluation was successful.</returns>
		protected virtual bool TryEvaluateMemberAccess(MemberExpression expression, out object result)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (expression.NodeType != ExpressionType.MemberAccess)
				throw new ArgumentException("expression.NodeType != ExpressionType.MemberAccess", "expression");

			try
			{
				object memberObject;
				if (TryEvaluateWithoutCompiling(expression.Expression, out memberObject))
				{
					var field = expression.Member as FieldInfo;
					if (field != null)
					{
						result = field.GetValue(memberObject);
						return true;
					}

					var property = expression.Member as PropertyInfo;
					if (property != null)
					{
#if NET45 || CORE45 || WINDOWS_PHONE_APP
						result = property.GetValue(memberObject);
#else
						result = property.GetValue(memberObject, new object[0]);
#endif
						return true;
					}
				}
			}
			catch (MemberAccessException)
			{
			}

			result = default(object);
			return false;
		}

		/// <summary>
		/// Tries to evaluate lambda expression without compilation.
		/// Base implementation only handles lambda of type expression and doesn't handle lambda delegates.
		/// </summary>
		/// <param name="expression">Expression to be evaluated. Cannot be null.</param>
		/// <param name="result">Expression value if successful. Otherwise <c>null</c>.</param>
		/// <returns>Whether the evaluation was successful.</returns>
		protected virtual bool TryEvaluateLambda(LambdaExpression expression, out object result)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (expression.NodeType != ExpressionType.Lambda)
				throw new ArgumentException("expression.NodeType != ExpressionType.Lambda", "expression");

#if NET35 || SL3 || WINDOWS_PHONE || PORTABLE36 || PORTABLE88 || PORTABLE328
			if (typeof(Expression).IsAssignableFrom(expression.Type))
#else
			if (typeof(Expression).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
#endif
			{
				result = expression;
				return true;
			}

			result = default(object);
			return false;
		}


		private bool TryEvaluateQuote(UnaryExpression expression, out object result)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (expression.NodeType != ExpressionType.Quote)
				throw new ArgumentException("expression.NodeType != ExpressionType.Quote", "expression");

			result = expression.Operand;
			return true;
		}

		private bool TryEvaluateConstant(ConstantExpression expression, out object result)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (expression.NodeType != ExpressionType.Constant)
				throw new ArgumentException("expression.NodeType != ExpressionType.Constant", "expression");

			result = expression.Value;
			return true;
		}

		private bool TryEvaluateCall(MethodCallExpression expression, out object result)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (expression.NodeType != ExpressionType.Call)
				throw new ArgumentException("expression.NodeType != ExpressionType.Call", "expression");

			object methodObject;
			if (!TryEvaluateWithoutCompiling(expression.Object, out methodObject))
			{
				result = default(object);
				return false;
			}

			var parameters = new object[expression.Arguments.Count];
			for (var argumentIndex = 0; argumentIndex < expression.Arguments.Count; argumentIndex++)
			{
				object parameter;
				if (!TryEvaluateWithoutCompiling(expression.Arguments[argumentIndex], out parameter))
				{
					result = default(object);
					return false;
				}
				parameters[argumentIndex] = parameter;
			}

			try
			{
				result = expression.Method.Invoke(methodObject, parameters);
				return true;
			}
			catch (MemberAccessException)
			{
				result = default(object);
				return false;
			}
		}
	}
}
