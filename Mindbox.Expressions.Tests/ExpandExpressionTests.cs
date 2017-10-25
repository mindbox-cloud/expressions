using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
#if NETFX_CORE || WINDOWS_PHONE 
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Mindbox.Expressions.Tests
{
	[TestClass]
	public class ExpandExpressionTests
	{
		[TestMethod]
		public void ExpandExpressionsSimpleTest()
		{
			Expression<Func<int, int>> f1 = x => x + 2;
			var result = f1.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(3, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpand1Test()
		{
			Expression<Func<int, int>> f1 = x => x + 2;
			Expression<Func<int, int>> f2 = x => f1.Compile()(x) * 3;
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(9, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpand1QuoteTest()
		{
			Expression<Func<int, int>> f1 = y => y + 2;

			var x = Expression.Parameter(typeof(int), "x");
			Expression<Func<int, int>> f2 = Expression.Lambda<Func<int, int>>(
				Expression.Multiply(
					Expression.Call(
						ReflectionExpressions.GetMethodInfo<Expression<Func<int, int>>>(expression => 
							expression.Evaluate(default(int))),
						Expression.Quote(f1),
						x), 
					Expression.Constant(3)),
				new[]
				{
					x
				});
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(9, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpand1MethodCallTest()
		{
			Expression<Func<int, int>> f2 = x => Getter().Evaluate(x) * 3;
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(6, result.Evaluate(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpand1QuoteParameterizedTest()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => y => x * 2 + y;
			Expression<Func<int, int>> f2 = z => f1.Evaluate(z).Evaluate(z * 3) + 1;
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(6, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpand1InvokeTest()
		{
			Expression<Func<int, int>> f1 = x => x + 2;
			Expression<Func<int, int>> f2 = x => f1.Compile().Invoke(x) * 3;
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(9, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpand1EvaluateTest()
		{
			Expression<Func<int, int>> f1 = x => x + 2;
			Expression<Func<int, int>> f2 = x => f1.Evaluate(x) * 3;
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(9, result.Evaluate(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpand1EvaluateMethodGroupTest()
		{
			Expression<Func<int, bool>> f1 = x => x == 2;
			Expression<Func<IEnumerable<int>, IEnumerable<int>>> f2 = x => x.Where(f1.Evaluate);
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(3, result.Evaluate(new[] { 1, 2, 2, 2, 3 }).Count());
			NoEvaluationsAssertion.AssertNoEvaluations(result);
			NoConvertAssertion.AssertNoConverts(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpand2EvaluateMethodGroupTest()
		{
			Expression<Func<int, bool>> f1 = x => x == 2;
			Expression<Func<IEnumerable<int>, IEnumerable<int>>> f2 = x => x.Where(f1.Evaluate).Concat(x.Where(f1.Evaluate));
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(6, result.Evaluate(new[] { 1, 2, 2, 2, 3 }).Count());
			NoEvaluationsAssertion.AssertNoEvaluations(result);
			NoConvertAssertion.AssertNoConverts(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpandComplexTest()
		{
			Expression<Func<int, int>> f1 = x => x + 2;
			Expression<Func<int, int>> f2 = y => f1.Compile()(y + 4) * 3;
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(21, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpandComplexEvaluateTest()
		{
			Expression<Func<int, int>> f1 = x => x + 2;
			Expression<Func<int, int>> f2 = y => f1.Evaluate(y + 4) * 3;
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(21, result.Evaluate(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpandNestedTest()
		{
			Expression<Func<int, int>> f1 = x => x + 2;
			Expression<Func<int, int>> f2 = y => f1.Compile()(f1.Compile()(y) + f1.Compile()(4)) * 3;
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(33, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsExpandNestedEvaluateTest()
		{
			Expression<Func<int, int>> f1 = x => x + 2;
			Expression<Func<int, int>> f2 = y => f1.Evaluate(f1.Evaluate(y) + f1.Evaluate(4)) * 3;
			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(33, result.Evaluate(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressions_EvaluateOnNull_Exception()
		{
			Expression<Func<int, int>> f1 = null;
			Expression<Func<int, int>> f2 = z => f1.Evaluate(z);

			AssertException.Throws<InvalidOperationException>(
				() => f2.ExpandExpressions(),
				ex =>
				{
					Assert.AreEqual(
						$"Usage of {nameof(Extensions.Evaluate)} on null expression is invalid", 
						ex.Message);
				});
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexTest()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				z => f1.Compile()(f1.Compile()(1).Compile()(z)).Compile()(f1.Compile()(5).Compile()(z));

			Assert.AreEqual(8, f2.Compile()(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(8, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexEvaluateTest()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				z => f1.Evaluate(f1.Evaluate(1).Evaluate(z)).Evaluate(f1.Evaluate(5).Evaluate(z));

			Assert.AreEqual(8, f2.Evaluate(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(8, result.Evaluate(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexTest2()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				z => f1.Compile()(f1.Compile()(z).Compile()(1)).Compile()(f1.Compile()(z).Compile()(5));

			Assert.AreEqual(8, f2.Compile()(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(8, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexEvaluateTest2()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				z => f1.Evaluate(f1.Evaluate(z).Evaluate(1)).Evaluate(f1.Evaluate(z).Evaluate(5));

			Assert.AreEqual(8, f2.Evaluate(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(8, result.Evaluate(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexTest3()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 = z => f1.Compile()(z).Compile()(5);

			Assert.AreEqual(6, f2.Compile()(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(6, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexEvaluateTest3()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 = z => f1.Evaluate(z).Evaluate(5);

			Assert.AreEqual(6, f2.Evaluate(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(6, result.Evaluate(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexTest4()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				z => f1.Compile()(f1.Compile()(z).Compile()(1)).Compile()(6);

			Assert.AreEqual(8, f2.Compile()(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(8, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexEvaluateTest4()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				z => f1.Evaluate(f1.Evaluate(z).Evaluate(1)).Evaluate(6);

			Assert.AreEqual(8, f2.Evaluate(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(8, result.Evaluate(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexSameNamesTest1()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				x => f1.Compile()(f1.Compile()(x).Compile()(1)).Compile()(f1.Compile()(x).Compile()(5));

			Assert.AreEqual(8, f2.Compile()(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(8, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexSameNamesEvaluateTest1()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				x => f1.Evaluate(f1.Evaluate(x).Evaluate(1)).Evaluate(f1.Evaluate(x).Evaluate(5));

			Assert.AreEqual(8, f2.Evaluate(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(8, result.Evaluate(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexSameNamesTest2()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				y => f1.Compile()(f1.Compile()(y).Compile()(1)).Compile()(f1.Compile()(y).Compile()(5));

			Assert.AreEqual(8, f2.Compile()(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(8, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsVeryComplexSameNamesEvaluateTest2()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				y => f1.Evaluate(f1.Evaluate(y).Evaluate(1)).Evaluate(f1.Evaluate(y).Evaluate(5));

			Assert.AreEqual(8, f2.Evaluate(1));

			var result = f2.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(8, result.Evaluate(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsDirtyTest()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				z => f1.Compile()(DirtyGetter(z).Compile()(1)).Compile()(f1.Compile()(5).Compile()(z));

			Assert.AreEqual(8, f2.Compile()(1));

			AssertException.Throws<InvalidOperationException>(
				() => f2.ExpandExpressions(),
				ex =>
				{
					Assert.AreEqual(
						"Expression isn't expandable due to usage of " +
						$"{nameof(Extensions.Evaluate)} or {nameof(LambdaExpression.Compile)} on expression, " +
						"that can't be obtained because it depends on outer lambda expression parameter.",
						ex.Message);
				});
		}

		[TestMethod]
		public void ExpandExpressionsDirtyEvaluateTest()
		{
			Expression<Func<int, Expression<Func<int, int>>>> f1 = x => (y => y + x);
			Expression<Func<int, int>> f2 =
				z => f1.Evaluate(DirtyGetter(z).Evaluate(1)).Evaluate(f1.Evaluate(5).Evaluate(z));

			Assert.AreEqual(8, f2.Evaluate(1));

			AssertException.Throws<InvalidOperationException>(
				() => f2.ExpandExpressions(),
				ex =>
				{
					Assert.AreEqual(
						"Expression isn't expandable due to usage of " +
						$"{nameof(Extensions.Evaluate)} or {nameof(LambdaExpression.Compile)} on expression, " +
						"that can't be obtained because it depends on outer lambda expression parameter.",
						ex.Message);
				});
		}

		[TestMethod]
		public void ExpandExpressionsNonCompileInvocationTest()
		{
			Expression<Func<int, int>> f1 = x => GetGetter()() + x;

			Assert.AreEqual(2, f1.Compile()(1));

			var result = f1.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
			Assert.AreEqual(2, result.Compile()(1));
			NoEvaluationsAssertion.AssertNoEvaluations(result);
		}

		[TestMethod]
		public void ExpandExpressionsFalseCompileTest()
		{
			Expression<Func<int, int>> f1 = x => x + 2;
			Expression<Func<int, Func<int, int>>> f2 = x => f1.Compile();
			var result = f2.ExpandExpressions();

			Assert.AreEqual(3, result.Evaluate(4)(1));
		}

		[TestMethod]
		public void ExpandExpressionsDuplicateParameterComlexTest()
		{
			Expression<Func<int, int>> f1 = x => new[] { x + 1 }.Single(y => y != x);
			Expression<Func<int, int>> f2 = x => x * x;
			Expression<Func<int, int>> f3 = x => f2.Evaluate(f1.Evaluate(x));

			var result = f3.ExpandExpressions();

			NoDuplicateParameterAssertion.AssertNoDuplicateParameters(result);
		}


		private static Expression<Func<int, int>> DirtyGetter(int argument)
		{
			return x => x + argument;
		}

		private static Func<int> GetGetter()
		{
			return () => 1;
		}

		private static Expression<Func<int, int>> Getter()
		{
			return x => x * 2;
		}


		private sealed class NoEvaluationsAssertion : ExpressionVisitor
		{
			private static readonly string CompileMethodName =
				ReflectionExpressions.GetMethodName<Expression<Func<object>>>(expression => expression.Compile());


			public static void AssertNoEvaluations(Expression expression)
			{
				new NoEvaluationsAssertion().Visit(expression);
			}


			private NoEvaluationsAssertion() { }


			protected override Expression VisitMethodCall(MethodCallExpression node)
			{
				if (node == null)
					throw new ArgumentNullException("node");

				ValidateMethod(node, node.Method);

				return base.VisitMethodCall(node);
			}

			protected override Expression VisitInvocation(InvocationExpression node)
			{
				if (node == null)
					throw new ArgumentNullException("node");

				if (node.Expression.NodeType == ExpressionType.Call)
				{
					var methodCallExpression = (MethodCallExpression)node.Expression;
					if ((methodCallExpression.Method.DeclaringType != null) &&
#if NET45 || CORE45 || WINDOWS_PHONE_APP
							methodCallExpression.Method.DeclaringType.IsConstructedGenericType &&
#else
							methodCallExpression.Method.DeclaringType.IsGenericType &&
							!methodCallExpression.Method.DeclaringType.IsGenericTypeDefinition &&
#endif
							(methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(Expression<>)) &&
							(methodCallExpression.Method.Name == CompileMethodName))
						Assert.Fail("The expression body has evaluation: \"{0}\".", node);
				}

				return base.VisitInvocation(node);
			}

			protected override Expression VisitConstant(ConstantExpression node)
			{
				if (node == null)
					throw new ArgumentNullException("node");

				if (node.Type == typeof(MethodInfo))
					ValidateMethod(node, (MethodInfo)node.Value);

				return base.VisitConstant(node);
			}


			private static void ValidateMethod(Expression node, MethodInfo method)
			{
				if (node == null)
					throw new ArgumentNullException("node");
				if (method == null)
					throw new ArgumentNullException("method");

				if ((method.DeclaringType == typeof(Extensions)) && 
						(method.Name == ReflectionExpressions.GetMethodName<Expression<Func<object>>>(
							expression => expression.Evaluate())))
					Assert.Fail("The expression body has evaluation: \"{0}\".", node);
				if ((method.DeclaringType != null) && 
#if NET35 || SL3 || WINDOWS_PHONE || PORTABLE36 || PORTABLE88 || PORTABLE328
						(method.DeclaringType.BaseType == 
#else
						(method.DeclaringType.GetTypeInfo().BaseType ==
#endif
							typeof(MulticastDelegate)) && 
						(method.Name == ReflectionExpressions.GetMethodName<Action>(action => action.Invoke())))
					Assert.Fail("The expression body has invokation: \"{0}\".", node);
			}
		}


		private sealed class NoConvertAssertion : ExpressionVisitor
		{
			public static void AssertNoConverts(Expression expression)
			{
				new NoConvertAssertion().Visit(expression);
			}


			private NoConvertAssertion() { }


			protected override Expression VisitUnary(UnaryExpression node)
			{
				if (node == null)
					throw new ArgumentNullException("node");

				if (node.NodeType == ExpressionType.Convert)
					Assert.Fail("The expression body has convert: \"{0}\".", node);

				return base.VisitUnary(node);
			}
		}


		private sealed class NoDuplicateParameterAssertion : ExpressionVisitor
		{

			public static void AssertNoDuplicateParameters(Expression expression)
			{
				new NoDuplicateParameterAssertion().Visit(expression);
			}


			private NoDuplicateParameterAssertion() { }


			private readonly List<ParameterExpression> parameters = new List<ParameterExpression>();


#if NET40 || SL4 || CORE45 || WP8 || WINDOWS_PHONE_APP || PORTABLE36 || PORTABLE328
			protected override Expression VisitLambda<T>(Expression<T> node)
			{
				if (node == null)
					throw new ArgumentNullException("node");

				foreach (var parameter in node.Parameters)
				{
					if (parameters.Contains(parameter))
						Assert.Fail("Duplicate parameter detected: \"{0}\".", parameter);
					parameters.Add(parameter);
				}

				return base.VisitLambda(node);
			}
		}
#else
			protected override Expression VisitLambda(LambdaExpression node)
			{
				if (node == null)
					throw new ArgumentNullException("node");

				foreach (var parameter in node.Parameters)
				{
					if (parameters.Contains(parameter))
						Assert.Fail("Duplicate parameter detected: \"{0}\".", parameter);
					parameters.Add(parameter);
				}

				return base.VisitLambda(node);
			}
		}
#endif
	}
}
