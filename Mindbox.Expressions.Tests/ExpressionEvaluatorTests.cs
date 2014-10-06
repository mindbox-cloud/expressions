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
	public class ExpressionEvaluatorTests
	{
		[TestMethod]
		public void EvaluateLambdaAsFunc()
		{
			var values = new[]
			{
				1,
				2,
				3
			};
			Expression<Func<int>> expression = () => values.Where(value => value != 2).Sum();
			var result = ExpressionEvaluator.Instance.Evaluate(expression.Body);
			Assert.AreEqual(4, result);
		}
	}
}
