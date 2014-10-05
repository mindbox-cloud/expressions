using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mindbox.Expressions.Tests
{
	[TestClass]
	public class EvaluateTests
	{
		[TestMethod]
		public void Evaluate0ArgumentsTest()
		{
			Expression<Func<int>> f1 = () => 5;

			Assert.AreEqual(5, f1.Evaluate());
		}

		[TestMethod]
		public void Evaluate1ArgumentsTest()
		{
			Expression<Func<int, int>> f1 = x => x + 1;

			Assert.AreEqual(6, f1.Evaluate(5));
		}

		[TestMethod]
		public void Evaluate2ArgumentsTest()
		{
			Expression<Func<int, int, int>> f1 = (x1, x2) => x1 + x2 * 2;

			Assert.AreEqual(5, f1.Evaluate(1, 2));
		}

		[TestMethod]
		public void Evaluate3ArgumentsTest()
		{
			Expression<Func<int, int, int, int>> f1 = (x1, x2, x3) => x1 + x2 * 2 + x3 * 3;

			Assert.AreEqual(14, f1.Evaluate(1, 2, 3));
		}

		[TestMethod]
		public void Evaluate4ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int>> f1 = (x1, x2, x3, x4) => x1 + x2 * 2 + x3 * 3 + x4 * 4;

			Assert.AreEqual(30, f1.Evaluate(1, 2, 3, 4));
		}

		[TestMethod]
		public void Evaluate5ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int>> f1 = (x1, x2, x3, x4, x5) => x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5;

			Assert.AreEqual(35, f1.Evaluate(1, 2, 3, 4, 5));
		}

		[TestMethod]
		public void Evaluate6ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int, int>> f1 = 
				(x1, x2, x3, x4, x5, x6) => x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5 + x6;

			Assert.AreEqual(41, f1.Evaluate(1, 2, 3, 4, 5, 6));
		}

		[TestMethod]
		public void Evaluate7ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int, int, int>> f1 =
				(x1, x2, x3, x4, x5, x6, x7) => x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5 + x6 + x7;

			Assert.AreEqual(48, f1.Evaluate(1, 2, 3, 4, 5, 6, 7));
		}

		[TestMethod]
		public void Evaluate8ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int, int, int, int>> f1 =
				(x1, x2, x3, x4, x5, x6, x7, x8) => x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5 + x6 + x7 + x8;

			Assert.AreEqual(56, f1.Evaluate(1, 2, 3, 4, 5, 6, 7, 8));
		}

		[TestMethod]
		public void Evaluate9ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int, int, int, int, int>> f1 =
				(x1, x2, x3, x4, x5, x6, x7, x8, x9) => x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5 + x6 + x7 + x8 + x9;

			Assert.AreEqual(65, f1.Evaluate(1, 2, 3, 4, 5, 6, 7, 8, 9));
		}

		[TestMethod]
		public void Evaluate10ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int, int, int, int, int, int>> f1 =
				(x1, x2, x3, x4, x5, x6, x7, x8, x9, x10) => x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5 + x6 + x7 + x8 + x9 + x10;

			Assert.AreEqual(75, f1.Evaluate(1, 2, 3, 4, 5, 6, 7, 8, 9, 10));
		}

		[TestMethod]
		public void Evaluate11ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int, int, int, int, int, int, int>> f1 =
				(x1, x2, x3, x4, x5, x6, x7, x8, x9, x10, x11) => 
					x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5 + x6 + x7 + x8 + x9 + x10 + x11;

			Assert.AreEqual(86, f1.Evaluate(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11));
		}

		[TestMethod]
		public void Evaluate12ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int, int, int, int, int, int, int, int>> f1 =
				(x1, x2, x3, x4, x5, x6, x7, x8, x9, x10, x11, x12) =>
					x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5 + x6 + x7 + x8 + x9 + x10 + x11 + x12;

			Assert.AreEqual(98, f1.Evaluate(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12));
		}

		[TestMethod]
		public void Evaluate13ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int>> f1 =
				(x1, x2, x3, x4, x5, x6, x7, x8, x9, x10, x11, x12, x13) =>
					x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5 + x6 + x7 + x8 + x9 + x10 + x11 + x12 - x13;

			Assert.AreEqual(85, f1.Evaluate(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13));
		}

		[TestMethod]
		public void Evaluate14ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>> f1 =
				(x1, x2, x3, x4, x5, x6, x7, x8, x9, x10, x11, x12, x13, x14) =>
					x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5 + x6 + x7 + x8 + x9 + x10 + x11 + x12 - x13 - x14;

			Assert.AreEqual(71, f1.Evaluate(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14));
		}

		[TestMethod]
		public void Evaluate15ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>> f1 =
				(x1, x2, x3, x4, x5, x6, x7, x8, x9, x10, x11, x12, x13, x14, x15) =>
					x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5 + x6 + x7 + x8 + x9 + x10 + x11 + x12 - x13 - x14 - x15;

			Assert.AreEqual(56, f1.Evaluate(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15));
		}

		[TestMethod]
		public void Evaluate16ArgumentsTest()
		{
			Expression<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>> f1 =
				(x1, x2, x3, x4, x5, x6, x7, x8, x9, x10, x11, x12, x13, x14, x15, x16) =>
					x1 + x2 * 2 + x3 * 3 + x4 * 4 + x5 + x6 + x7 + x8 + x9 + x10 + x11 + x12 - x13 - x14 - x15 - x16;

			Assert.AreEqual(40, f1.Evaluate(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16));
		}
	}
}
