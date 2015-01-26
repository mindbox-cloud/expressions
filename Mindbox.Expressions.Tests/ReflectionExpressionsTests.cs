using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
#if NETFX_CORE || WINDOWS_PHONE 
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Mindbox.Expressions.Tests
{
	[TestClass]
	public class ReflectionExpressionsTests
	{
		[TestMethod]
		public void GetMethodInfoInstanceFunctionTest()
		{
			var result = ReflectionExpressions.GetMethodInfo<Test8>(methodObject => methodObject.Method1());
			Assert.AreEqual("Method1", result.Name);
		}

		[TestMethod]
		public void GetMethodInfoInstanceFunctionVariableTest()
		{
			Expression<Func<Test8, int>> expression = methodObject => methodObject.Method1();
			var result = ReflectionExpressions.GetMethodInfo(expression);
			Assert.AreEqual("Method1", result.Name);
		}

		[TestMethod]
		public void GetMethodInfoInstanceFunctionWithArgumentsTest()
		{
			var result = ReflectionExpressions.GetMethodInfo<Test8>(methodObject => 
				methodObject.Method2(default(string), default(int)));
			Assert.AreEqual("Method2", result.Name);
		}

		[TestMethod]
		public void GetMethodInfoInstanceVoidTest()
		{
			var result = ReflectionExpressions.GetMethodInfo<Test8>(methodObject => methodObject.Method3());
			Assert.AreEqual("Method3", result.Name);
		}

		[TestMethod]
		public void GetMethodInfoStaticVoidTest()
		{
			var result = ReflectionExpressions.GetMethodInfo(() => Test8.Method4());
			Assert.AreEqual("Method4", result.Name);
		}

		[TestMethod]
		public void GetMethodInfoStaticFunctionTest()
		{
			var result = ReflectionExpressions.GetMethodInfo(() => Test8.Method5());
			Assert.AreEqual("Method5", result.Name);
		}

		[TestMethod]
		public void GetMethodInfoStaticFunctionVariableTest()
		{
			Expression<Func<int>> expression = () => Test8.Method5();
			var result = ReflectionExpressions.GetMethodInfo(expression);
			Assert.AreEqual("Method5", result.Name);
		}

		[TestMethod]
		public void TryGetPropertyNameTest()
		{
			Expression<Func<Test2, object>> expression = test => test.X;
			Assert.AreEqual("X", ReflectionExpressions.TryGetPropertyName(expression));
		}

		[TestMethod]
		public void TryGetPropertyNameGenericInterfaceConstraintTest()
		{
			TryGetPropertyNameGeneric<Test3>();
		}

		[TestMethod]
		public void TryGetIndexedPropertyNameTest()
		{
			Expression<Func<Test2, object>> expression = test => test[default(string)];
			Assert.AreEqual("Item", ReflectionExpressions.TryGetPropertyName(expression));
		}

		[TestMethod]
		public void TryGetFieldNameTest()
		{
			Expression<Func<Test2, object>> expression = test => test.y;
			Assert.AreEqual("y", ReflectionExpressions.TryGetFieldName(expression));
		}

		[TestMethod]
		public void TryGetMethodNameNullTest1()
		{
			Assert.IsNull(ReflectionExpressions.TryGetMethodName((Expression<Func<object, object>>)null));
		}

		[TestMethod]
		public void TryGetMethodNameNullTest2()
		{
			Assert.IsNull(ReflectionExpressions.TryGetMethodName((Expression<Action<object>>)null));
		}

		[TestMethod]
		public void TryGetMethodNameNullTest3()
		{
			Assert.IsNull(ReflectionExpressions.TryGetMethodName((Expression<Func<object>>)null));
		}

		[TestMethod]
		public void TryGetMethodNameNullTest4()
		{
			Assert.IsNull(ReflectionExpressions.TryGetMethodName((Expression<Action>)null));
		}

		[TestMethod]
		public void TryGetMethodNameNullTest5()
		{
			Assert.IsNull(ReflectionExpressions.TryGetMethodName((LambdaExpression)null));
		}

		[TestMethod]
		public void TryGetPropertyNameNullTest1()
		{
			Assert.IsNull(ReflectionExpressions.TryGetPropertyName((Expression<Func<object, object>>)null));
		}

		[TestMethod]
		public void TryGetPropertyNameNullTest2()
		{
			Assert.IsNull(ReflectionExpressions.TryGetPropertyName((Expression<Func<object>>)null));
		}

		[TestMethod]
		public void TryGetPropertyNameNullTest3()
		{
			Assert.IsNull(ReflectionExpressions.TryGetPropertyName((LambdaExpression)null));
		}

		[TestMethod]
		public void TryGetFieldNameNullTest1()
		{
			Assert.IsNull(ReflectionExpressions.TryGetFieldName((Expression<Func<object, object>>)null));
		}

		[TestMethod]
		public void TryGetFieldNameNullTest2()
		{
			Assert.IsNull(ReflectionExpressions.TryGetFieldName((Expression<Func<object>>)null));
		}

		[TestMethod]
		public void TryGetFieldNameNullTest3()
		{
			Assert.IsNull(ReflectionExpressions.TryGetFieldName((LambdaExpression)null));
		}


		// When using interface and new() generic constraints, expression contains Convert to the interface.
		private void TryGetPropertyNameGeneric<T>()
			where T : ITest1, new()
		{
			Expression<Func<T, object>> expression = test => test.Y;
			Assert.AreEqual("Y", ReflectionExpressions.TryGetPropertyName(expression));
		}


		private interface ITest1
		{
			int Y { get; }
			Test2 Z { get; }
		}

		private class Test2
		{
			public int y = 1;

			public int X { get; set; }

			public int this[string key]
			{
				get { return key == null ? 0 : key.Length; }
			}
		}

		private class Test3 : Test2, ITest1
		{
			int ITest1.Y
			{
				get
				{
					return 0;
				}
			}

			Test2 ITest1.Z
			{
				get
				{
					return null;
				}
			}
		}

		private abstract class Test8
		{
			public static void Method4()
			{
			}

			public static int Method5()
			{
				return 0;
			}


			public abstract int Method1();
			public abstract int Method2(string x, int y);
			public abstract void Method3();
		}
	}
}
