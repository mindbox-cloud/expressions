using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mindbox.Expressions
{
#if !NET40 && !SL4 && !CORE45 && !WP8 && !WINDOWS_PHONE_APP && !PORTABLE36 && !PORTABLE328
	/// <summary>
	/// Represents a visitor or rewriter for expression trees.
	/// Not present in .NET 4.0 and higher versions of the library
	/// (use System.Linq.Expressions.ExpressionVisitor from System.Core.dll).
	/// </summary>
	/// <remarks>
	/// From MSDN: http://msdn.microsoft.com/en-us/library/bb882521(v=vs.90).aspx
	/// </remarks>
	public abstract class ExpressionVisitor
	{
		/// <summary>
		/// Dispatches the expression to one of the more specialized visit methods in this class.
		/// </summary>
		/// <param name="exp">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression Visit(Expression exp)
		{
			if (exp == null)
				return null;

			switch (exp.NodeType)
			{
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
				case ExpressionType.Not:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.ArrayLength:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
					return VisitUnary((UnaryExpression)exp);

				case ExpressionType.Add:
				case ExpressionType.AddChecked:
				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.Divide:
				case ExpressionType.Modulo:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				case ExpressionType.Coalesce:
				case ExpressionType.ArrayIndex:
				case ExpressionType.RightShift:
				case ExpressionType.LeftShift:
				case ExpressionType.ExclusiveOr:
					return VisitBinary((BinaryExpression)exp);

				case ExpressionType.TypeIs:
					return VisitTypeIs((TypeBinaryExpression)exp);

				case ExpressionType.Conditional:
					return VisitConditional((ConditionalExpression)exp);

				case ExpressionType.Constant:
					return VisitConstant((ConstantExpression)exp);

				case ExpressionType.Parameter:
					return VisitParameter((ParameterExpression)exp);

				case ExpressionType.MemberAccess:
					return VisitMemberAccess((MemberExpression)exp);

				case ExpressionType.Call:
					return VisitMethodCall((MethodCallExpression)exp);

				case ExpressionType.Lambda:
					return VisitLambda((LambdaExpression)exp);

				case ExpressionType.New:
					return VisitNew((NewExpression)exp);

				case ExpressionType.NewArrayInit:
				case ExpressionType.NewArrayBounds:
					return VisitNewArray((NewArrayExpression)exp);

				case ExpressionType.Invoke:
					return VisitInvocation((InvocationExpression)exp);

				case ExpressionType.MemberInit:
					return VisitMemberInit((MemberInitExpression)exp);

				case ExpressionType.ListInit:
					return VisitListInit((ListInitExpression)exp);
				
				// Expression types can be extended by third-party libraries (for example, EntityFramework Core).
				// These extensions are assumed not expandable, but valid.
				case ExpressionType.Extension:
					return exp;

				default:
					throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
			}
		}

		/// <summary>
		/// Visits the children of the MemberBinding.
		/// </summary>
		/// <param name="binding">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual MemberBinding VisitBinding(MemberBinding binding)
		{
			if (binding == null)
				throw new ArgumentNullException("binding");

			switch (binding.BindingType)
			{
				case MemberBindingType.Assignment:
					return VisitMemberAssignment((MemberAssignment)binding);

				case MemberBindingType.MemberBinding:
					return VisitMemberMemberBinding((MemberMemberBinding)binding);

				case MemberBindingType.ListBinding:
					return VisitMemberListBinding((MemberListBinding)binding);

				default:
					throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
			}
		}

		/// <summary>
		/// Visits the children of the <c>ElementInit</c>.
		/// </summary>
		/// <param name="initializer">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
		{
			if (initializer == null)
				throw new ArgumentNullException("initializer");

			var arguments = VisitExpressionList(initializer.Arguments);
			return arguments == initializer.Arguments ? initializer : Expression.ElementInit(initializer.AddMethod, arguments);
		}

		/// <summary>
		/// Visits the children of the <c>UnaryExpression</c>.
		/// </summary>
		/// <param name="u">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitUnary(UnaryExpression u)
		{
			if (u == null)
				throw new ArgumentNullException("u");

			var operand = Visit(u.Operand);
			return operand == u.Operand ? u : Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
		}

		/// <summary>
		/// Visits the children of the <c>BinaryExpression</c>.
		/// </summary>
		/// <param name="b">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitBinary(BinaryExpression b)
		{
			if (b == null)
				throw new ArgumentNullException("b");

			var left = Visit(b.Left);
			var right = Visit(b.Right);
			var conversion = Visit(b.Conversion);
			if (left == b.Left && right == b.Right && conversion == b.Conversion)
				return b;
			if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
				return Expression.Coalesce(left, right, conversion as LambdaExpression);
			return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
		}

		/// <summary>
		/// Visits the children of the <c>TypeBinaryExpression</c>.
		/// </summary>
		/// <param name="b">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
		{
			if (b == null)
				throw new ArgumentNullException("b");

			var expr = Visit(b.Expression);
			return expr == b.Expression ? b : Expression.TypeIs(expr, b.TypeOperand);
		}

		/// <summary>
		/// Visits the <c>ConstantExpression</c>.
		/// </summary>
		/// <param name="c">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitConstant(ConstantExpression c)
		{
			if (c == null)
				throw new ArgumentNullException("c");

			return c;
		}

		/// <summary>
		/// Visits the children of the <c>ConditionalExpression</c>.
		/// </summary>
		/// <param name="c">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitConditional(ConditionalExpression c)
		{
			if (c == null)
				throw new ArgumentNullException("c");

			var test = Visit(c.Test);
			var ifTrue = Visit(c.IfTrue);
			var ifFalse = Visit(c.IfFalse);
			return test == c.Test && ifTrue == c.IfTrue && ifFalse == c.IfFalse ? c : Expression.Condition(test, ifTrue, ifFalse);
		}

		/// <summary>
		/// Visits the <c>ParameterExpression</c>.
		/// </summary>
		/// <param name="p">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitParameter(ParameterExpression p)
		{
			if (p == null)
				throw new ArgumentNullException("p");

			return p;
		}

		/// <summary>
		/// Visits the children of the <c>MemberExpression</c>.
		/// </summary>
		/// <param name="m">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitMemberAccess(MemberExpression m)
		{
			if (m == null)
				throw new ArgumentNullException("m");

			var exp = Visit(m.Expression);
			return exp == m.Expression ? m : Expression.MakeMemberAccess(exp, m.Member);
		}

		/// <summary>
		/// Visits the children of the <c>MethodCallExpression</c>.
		/// </summary>
		/// <param name="m">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitMethodCall(MethodCallExpression m)
		{
			if (m == null)
				throw new ArgumentNullException("m");

			var obj = Visit(m.Object);
			var args = VisitExpressionList(m.Arguments);
			return obj == m.Object && args == m.Arguments ? m : Expression.Call(obj, m.Method, args);
		}

		/// <summary>
		/// Dispatches the list of expressions to one of the more specialized visit methods in this class.
		/// </summary>
		/// <param name="original">The expressions to visit.</param>
		/// <returns>The modified expression list, if any one of the elements were modified; 
		/// otherwise, returns the original expression list.</returns>
		protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
		{
			if (original == null)
				throw new ArgumentNullException("original");

			List<Expression> list = null;
			for (int i = 0, n = original.Count; i < n; i++)
			{
				var p = Visit(original[i]);
				if (list != null)
				{
					list.Add(p);
				}
				else if (p != original[i])
				{
					list = new List<Expression>(n);
					for (var j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}
					list.Add(p);
				}
			}
			return list == null ? original : new ReadOnlyCollection<Expression>(list);
		}

		/// <summary>
		/// Visits the children of the <c>MemberAssignment</c>.
		/// </summary>
		/// <param name="assignment">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			if (assignment == null)
				throw new ArgumentNullException("assignment");

			var e = Visit(assignment.Expression);
			return e == assignment.Expression ? assignment : Expression.Bind(assignment.Member, e);
		}

		/// <summary>
		/// Visits the children of the <c>MemberMemberBinding</c>.
		/// </summary>
		/// <param name="binding">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
		{
			if (binding == null)
				throw new ArgumentNullException("binding");

			var bindings = VisitBindingList(binding.Bindings);
			return bindings == binding.Bindings ? binding : Expression.MemberBind(binding.Member, bindings);
		}

		/// <summary>
		/// Visits the children of the <c>MemberListBinding</c>.
		/// </summary>
		/// <param name="binding">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
		{
			if (binding == null)
				throw new ArgumentNullException("binding");

			var initializers = VisitElementInitializerList(binding.Initializers);
			return initializers == binding.Initializers ? binding : Expression.ListBind(binding.Member, initializers);
		}

		/// <summary>
		/// Visits all <c>MemberBinding</c> nodes in the collection using <c>VisitBinding</c> method.
		/// </summary>
		/// <param name="original">The nodes to visit.</param>
		/// <returns>The modified node list, if any of the elements were modified; 
		/// otherwise, returns the original node list.</returns>
		protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
		{
			if (original == null)
				throw new ArgumentNullException("original");

			List<MemberBinding> list = null;
			for (int i = 0, n = original.Count; i < n; i++)
			{
				var b = VisitBinding(original[i]);
				if (list != null)
				{
					list.Add(b);
				}
				else if (b != original[i])
				{
					list = new List<MemberBinding>(n);
					for (var j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}
					list.Add(b);
				}
			}
			if (list == null)
				return original;
			return list;
		}

		/// <summary>
		/// Visits all <c>ElementInit</c> nodes in the collection using <c>VisitElementInitializer</c> method.
		/// </summary>
		/// <param name="original">The nodes to visit.</param>
		/// <returns>The modified node list, if any of the elements were modified; 
		/// otherwise, returns the original node list.</returns>
		protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
		{
			if (original == null)
				throw new ArgumentNullException("original");

			List<ElementInit> list = null;
			for (int i = 0, n = original.Count; i < n; i++)
			{
				var init = VisitElementInitializer(original[i]);
				if (list != null)
				{
					list.Add(init);
				}
				else if (init != original[i])
				{
					list = new List<ElementInit>(n);
					for (var j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}
					list.Add(init);
				}
			}
			if (list != null)
				return list;
			return original;
		}

		/// <summary>
		/// Visits the children of the <c>LambdaExpression</c>.
		/// </summary>
		/// <param name="lambda">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitLambda(LambdaExpression lambda)
		{
			if (lambda == null)
				throw new ArgumentNullException("lambda");

			var body = Visit(lambda.Body);
			return body == lambda.Body ? lambda : Expression.Lambda(lambda.Type, body, lambda.Parameters);
		}

		/// <summary>
		/// Visits the children of the <c>NewExpression</c>.
		/// </summary>
		/// <param name="nex">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual NewExpression VisitNew(NewExpression nex)
		{
			if (nex == null)
				throw new ArgumentNullException("nex");

			IEnumerable<Expression> args = VisitExpressionList(nex.Arguments);
			if (args == nex.Arguments)
				return nex;
			return nex.Members == null ? 
				Expression.New(nex.Constructor, args) : 
				Expression.New(nex.Constructor, args, nex.Members);
		}

		/// <summary>
		/// Visits the children of the <c>MemberInitExpression</c>.
		/// </summary>
		/// <param name="init">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitMemberInit(MemberInitExpression init)
		{
			if (init == null)
				throw new ArgumentNullException("init");

			var n = VisitNew(init.NewExpression);
			var bindings = VisitBindingList(init.Bindings);
			if (n == init.NewExpression && bindings == init.Bindings)
				return init;
			return Expression.MemberInit(n, bindings);
		}

		/// <summary>
		/// Visits the children of the <c>ListInitExpression</c>.
		/// </summary>
		/// <param name="init">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitListInit(ListInitExpression init)
		{
			if (init == null)
				throw new ArgumentNullException("init");

			var n = VisitNew(init.NewExpression);
			var initializers = VisitElementInitializerList(init.Initializers);
			if (n == init.NewExpression && initializers == init.Initializers)
				return init;
			return Expression.ListInit(n, initializers);
		}

		/// <summary>
		/// Visits the children of the <c>NewArrayExpression</c>.
		/// </summary>
		/// <param name="na">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitNewArray(NewArrayExpression na)
		{
			if (na == null)
				throw new ArgumentNullException("na");

			var exprs = VisitExpressionList(na.Expressions);
			if (exprs == na.Expressions)
				return na;
			if (na.NodeType == ExpressionType.NewArrayInit)
				return Expression.NewArrayInit(na.Type.GetElementType(), exprs);
			return Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
		}

		/// <summary>
		/// Visits the children of the <c>InvocationExpression</c>.
		/// </summary>
		/// <param name="iv">The expression to visit.</param>
		/// <returns>The modified expression, if it or any subexpression was modified; 
		/// otherwise, returns the original expression.</returns>
		protected virtual Expression VisitInvocation(InvocationExpression iv)
		{
			if (iv == null)
				throw new ArgumentNullException("iv");

			var args = VisitExpressionList(iv.Arguments);
			var expr = Visit(iv.Expression);
			if (args == iv.Arguments && expr == iv.Expression)
				return iv;
			return Expression.Invoke(expr, args);
		}
	}
#endif
}
