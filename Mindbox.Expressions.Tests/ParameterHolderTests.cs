using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mindbox.Expressions.Tests
{
	[TestClass]
	public class ParameterHolderTests
	{
		[TestMethod]
		public void CheckLevel0Translation()
		{
			var parameters = ParameterHolderFactory.Create(Enumerable.Range(0, 0).Cast<object>().ToList());

			Assert.IsNull(parameters);
		}

		[TestMethod]
		public void CheckLevel1Translation()
		{
			var parameters = ParameterHolderFactory.Create(Enumerable.Range(0, 8 - 1).Cast<object>().ToList());

			Assert.AreEqual(0, parameters.P0);

			Assert.AreEqual(6, parameters.P6);
		}

		[TestMethod]
		public void CheckLevel2Translation()
		{
			var parameters = ParameterHolderFactory.Create(Enumerable.Range(0, 64 - 1).Cast<object>().ToList());

			Assert.AreEqual(0, ((ParameterHolder)parameters.P0).P0);

			Assert.AreEqual(54, ((ParameterHolder)parameters.P6).P6);
		}

		[TestMethod]
		public void CheckLevel3Translation()
		{
			var parameters = ParameterHolderFactory.Create(Enumerable.Range(0, 512 - 64 - 1).Cast<object>().ToList());

			Assert.AreEqual(0, ((ParameterHolder)((ParameterHolder)parameters.P0).P0).P0);

			Assert.AreEqual(438, ((ParameterHolder)((ParameterHolder)parameters.P6).P6).P6);
		}
	}
}