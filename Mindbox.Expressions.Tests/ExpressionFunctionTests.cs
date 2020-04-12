using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mindbox.Expressions.Tests
{
	[TestClass]
	public class ExpressionFunctionTests
	{
		private static object EvaluateViaExpressionFunction(Expression expression)
		{
			var evaluator = ExpressionFunctionFactory.Create(expression);

			return evaluator.DynamicInvoke();
		}

		[TestMethod]
		public void TestEvaluateObject()
		{
			var obj = new object();

			var result = EvaluateViaExpressionFunction(Expression.Constant(obj));

			Assert.AreEqual(obj, result);
		}

		[TestMethod]
		public void TestEvaluateInt()
		{
			var result = EvaluateViaExpressionFunction(Expression.Constant(1));

			Assert.AreEqual(1, result);
		}

		[TestMethod]
		public void TestEvaluateNullableInt()
		{
			var result = EvaluateViaExpressionFunction(Expression.Constant((int?)1, typeof(int?)));

			Assert.IsInstanceOfType(result, typeof(int?));
			Assert.AreEqual((int?)1, result);
		}

		[TestMethod]
		public void TestEvaluateNullableIntWithNull()
		{
			var result = EvaluateViaExpressionFunction(Expression.Constant(null, typeof(int?)));

			Assert.IsNull(result);
		}

		[TestMethod]
		public void TestEvaluateConvertIntToNullableInt()
		{
			var result = EvaluateViaExpressionFunction(
				Expression.Convert(
					Expression.Constant(1),
					typeof(int?)
				)
			);

			Assert.IsInstanceOfType(result, typeof(int?));
			Assert.AreEqual((int?)1, result);
		}

		[TestMethod]
		public void TestEvaluateConvertNullToNullableInt()
		{
			var result = EvaluateViaExpressionFunction(
				Expression.Convert(
					Expression.Constant(null),
					typeof(int?)
				)
			);

			Assert.IsNull(result);
		}
	}
}