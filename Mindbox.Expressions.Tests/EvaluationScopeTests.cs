using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mindbox.Expressions.Tests
{
	[TestClass]
	public class EvaluationScopeTests
	{
		class Holder
		{
			public byte Property => 2;

			public static short StaticProperty => 4;

			public int Field = 8;

			public static long StaticField = 16;

			public long Sum(int a, short b) => a + b;

			public static long StaticSum(int a, short b) => a + b;

			public static Holder operator-(Holder other)
			{
				return new Holder
				{
					Field = -other.Field
				};
			}
		}

		private readonly Holder holder = new Holder();


		[TestMethod]
		public void ConstantExpression()
		{
			var constantExpression = Expression.Constant(1);

			var result = EvaluationScope.Empty.TryEvaluate(constantExpression);

			Assert.AreEqual(1, result.Value);
		}

		[TestMethod]
		public void PropertyExpression()
		{
			Expression<Func<byte>> wrappingLambda = () => holder.Property;

			var result = EvaluationScope.Empty.TryEvaluate(wrappingLambda.Body);

			Assert.AreEqual((byte)2, result.Value);
		}

		[TestMethod]
		public void StaticPropertyExpression()
		{
			Expression<Func<short>> wrappingLambda = () => Holder.StaticProperty;

			var result = EvaluationScope.Empty.TryEvaluate(wrappingLambda.Body);

			Assert.AreEqual((short)4, result.Value);
		}

		[TestMethod]
		public void FieldExpression()
		{
			Expression<Func<int>> wrappingLambda = () => holder.Field;

			var result = EvaluationScope.Empty.TryEvaluate(wrappingLambda.Body);

			Assert.AreEqual(8, result.Value);
		}

		[TestMethod]
		public void StaticFieldExpression()
		{
			Expression<Func<long>> wrappingLambda = () => Holder.StaticField;

			var result = EvaluationScope.Empty.TryEvaluate(wrappingLambda.Body);

			Assert.AreEqual(16L, result.Value);
		}

		[TestMethod]
		public void MethodCallExpression()
		{
			Expression<Func<long>> wrappingLambda = () => holder.Sum(3, 11);

			var result = EvaluationScope.Empty.TryEvaluate(wrappingLambda.Body);

			Assert.AreEqual(14L, result.Value);
		}

		[TestMethod]
		public void StaticMethodCallExpression()
		{
			Expression<Func<long>> wrappingLambda = () => Holder.StaticSum(7, 13);

			var result = EvaluationScope.Empty.TryEvaluate(wrappingLambda.Body);

			Assert.AreEqual(20L, result.Value);
		}

		[TestMethod]
		public void IndexerMethodCallExpression()
		{
			var array = new [,] { { 1, 2 }, { 3, 4 } };

			Expression<Func<int>> wrappingLambda = () => array[1, 0];

			var result = EvaluationScope.Empty.TryEvaluate(wrappingLambda.Body);

			Assert.AreEqual(3, result.Value);
		}
	}
}
