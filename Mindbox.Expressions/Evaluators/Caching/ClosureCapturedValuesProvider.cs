using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mindbox.Expressions
{
	internal class ClosureCapturedValuesProvider : ClosureCapturedValuesVisitor
	{
		public static object[] GetCapturedValues(Expression expression)
		{
			var visitor = new ClosureCapturedValuesProvider();
			visitor.Visit(expression);

			return visitor.capturedValues.ToArray();
		}

		private ClosureCapturedValuesProvider()
		{

		}

		private readonly List<object> capturedValues = new List<object>();

		protected override Expression TryProcessClosure(ConstantExpression node)
		{
			if (node == null)
				throw new ArgumentNullException(nameof(node));

			capturedValues.Add(node.Value);

			return null;
		}
	}
}