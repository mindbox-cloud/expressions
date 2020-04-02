using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		}

		private readonly Holder holder = new Holder();


		[TestMethod]
		public void ConstantExpression()
		{
			AssertIdenticalResult(() => 1);
		}

		[TestMethod]
		public void PropertyExpression()
		{
			AssertIdenticalResult(() => holder.Property);
		}

		[TestMethod]
		public void StaticPropertyExpression()
		{
			AssertIdenticalResult(() => Holder.StaticProperty);
		}

		[TestMethod]
		public void FieldExpression()
		{
			AssertIdenticalResult(() => holder.Field);
		}

		[TestMethod]
		public void StaticFieldExpression()
		{
			AssertIdenticalResult(() => Holder.StaticField);
		}

		[TestMethod]
		public void MethodCallExpression()
		{
			AssertIdenticalResult(() => holder.Sum(3, 11));
		}

		[TestMethod]
		public void StaticMethodCallExpression()
		{
			AssertIdenticalResult(() => Holder.StaticSum(7, 13));
		}

		[TestMethod]
		public void IndexerMethodCallExpression()
		{
			var array = new [,] { { 1, 2 }, { 3, 4 } };

			AssertIdenticalResult(() => array[1, 0]);
		}

		[TestMethod]
		public void ArrayLengthExpression()
		{
			var array = new [] { 1, 2, 3 };

			AssertIdenticalResult(() => array.Length);
		}

		struct Convertible
		{
			public static implicit operator string(Convertible c)
			{
				return "-10";
			}

			public static explicit operator int(Convertible c)
			{
				return 10;
			}
		}

		[TestMethod]
		public void UnaryExpression_Convert_ImplicitMethod()
		{
			AssertIdenticalResult<string>(() => new Convertible());
		}

		[TestMethod]
		public void UnaryExpression_Convert_ExplicitMethod()
		{
			AssertIdenticalResult(() => (int) new Convertible());
		}

		[TestMethod]
		public void UnaryExpression_Convert_IntToLong()
		{
			AssertIdenticalResult(Expression.Lambda<Func<long>>(
				Expression.Convert(
					Expression.Constant(1, typeof(int)),
					typeof(long))));
		}

		[TestMethod]
		public void UnaryExpression_Convert_IntToNullableInt()
		{
			AssertIdenticalResult(Expression.Lambda<Func<int?>>(
				Expression.Convert(
					Expression.Constant(1, typeof(int)),
					typeof(int?))));
		}

		[TestMethod]
		public void UnaryExpression_Convert_IntToNullableLong()
		{
			AssertIdenticalResult(
				Expression.Lambda<Func<long?>>(
					Expression.Convert(
						Expression.Constant(1, typeof(int)),
						typeof(long?))));
		}

		[TestMethod]
		public void UnaryExpression_Convert_ValueTypeToObject()
		{
			AssertIdenticalResult(() => (object) 1);
		}

		[TestMethod]
		public void UnaryExpression_Convert_ObjectToValueType()
		{
			var boxedInt = (object) 1;
			AssertIdenticalResult(() => (int)boxedInt);
		}

		class Super { }

		class Sub : Super { }

		[TestMethod]
		public void UnaryExpression_Convert_ObjectToObject_CastSubclassToSuperclass()
		{
			var obj = new Sub();
			AssertIdenticalResult(() => (Super)obj);
		}

		[TestMethod]
		public void UnaryExpression_Convert_ObjectToObject_CastSuperclassToSubclass()
		{
			Super obj = new Sub();
			AssertIdenticalResult(() => (Sub)obj);
		}

		[TestMethod]
		public void UnaryExpression_Convert_EvaluateValueCastThrows()
		{
			var obj = (object)new KeyValuePair<int, int>();
			AssertIdenticalException<object, InvalidCastException>(() => (KeyValuePair<int, long>)obj);
		}

		struct ValueStruct
		{
			public int Value { get; set; }

			public static ValueStruct operator -(ValueStruct negated)
			{
				return new ValueStruct
				{
					Value = -negated.Value
				};
			}
		}

		[TestMethod]
		public void UnaryExpression_OperatorCall()
		{
			AssertIdenticalResult(() => -new ValueStruct { Value = 1 });
		}

		[TestMethod]
		public void UnaryExpression_OperatorCall_LiftedToNull_NonNullOperand()
		{
			AssertIdenticalResult<ValueStruct?>(() => -new ValueStruct { Value = 1 });
		}

		[TestMethod]
		public void UnaryExpression_OperatorCall_LiftedToNull_NullOperand()
		{
			AssertIdenticalResult<ValueStruct?>(() => -(ValueStruct?)null);
		}

		private static void AssertIdenticalResult<TResult>(Expression<Func<TResult>> expression)
		{
			var evaluatedResult = (TResult)EvaluationScope.Empty.TryEvaluate(expression.Body);

			var compiledExpressionResult = expression.Compile()();

			Assert.AreEqual(compiledExpressionResult, evaluatedResult);
		}

		private static void AssertIdenticalException<TResult, TException>(Expression<Func<TResult>> expression)
			where TException : Exception
		{
			Assert.ThrowsException<TException>(() => EvaluationScope.Empty.TryEvaluate(expression.Body));
			Assert.ThrowsException<TException>(() => expression.Compile()());

		}
	}
}
