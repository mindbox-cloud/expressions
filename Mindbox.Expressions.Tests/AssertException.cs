using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mindbox.Expressions.Tests
{
	public static class AssertException
	{
		public static void Throws<TException>(Action action, Action<TException> assertion)
			where TException : Exception
		{
			try
			{
				action();
			}
			catch (TException e)
			{
				assertion(e);
				return;
			}

			Assert.Fail($"Expected exception of type {typeof(TException)}, but is has never occured");
		}
	}
}