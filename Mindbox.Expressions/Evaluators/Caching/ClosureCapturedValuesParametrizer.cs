using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Mindbox.Expressions
{
	internal class ClosureCapturedValuesParametrizer : ClosureCapturedValuesVisitor
	{
		public static Expression GetParametrizedExpression(
			Expression expression,
			ParameterExpression arrayOfValues)
		{
			var visitor = new ClosureCapturedValuesParametrizer(arrayOfValues);
			var parametrizedBody = visitor.Visit(expression);
			return parametrizedBody.Type.IsPrimitive
				? Expression.Convert(parametrizedBody, typeof(object))
				: parametrizedBody;
		}

		private static readonly MethodInfo[] properties;

		static ClosureCapturedValuesParametrizer()
		{
			var type = typeof(ParameterHolder);

			properties = new[]
			{
				type.GetProperty("P0").GetMethod,
				type.GetProperty("P1").GetMethod,
				type.GetProperty("P2").GetMethod,
				type.GetProperty("P3").GetMethod,
				type.GetProperty("P4").GetMethod,
				type.GetProperty("P5").GetMethod,
				type.GetProperty("P6").GetMethod,
				type.GetProperty("P7").GetMethod
			};
		}

		private readonly ParameterExpression parametersExpression;

		private int visitedIndex;

		private ClosureCapturedValuesParametrizer(
			ParameterExpression parametersExpression)
		{
			this.parametersExpression = parametersExpression;
		}

		protected override Expression TryProcessClosure(ConstantExpression node)
		{
			return CreateIndexExpression(node);
		}

		private UnaryExpression CreateIndexExpression(ConstantExpression node)
		{
			var propertyAccess = CreatePropertyAccess(parametersExpression, visitedIndex);

			var indexExpression = Expression.Convert(
				propertyAccess,
				node.Type);
			visitedIndex++;
			return indexExpression;
		}

		private static MemberExpression CreatePropertyAccess(Expression rootExpression, int index)
		{
			while (true)
			{
				if (index < 8)
				{
					return Expression.Property(
						Expression.Convert(rootExpression, typeof(ParameterHolder)),
						properties[index]);
				}

				var indexPart = index & 0x7;
				var restPart = index >> 3;
				rootExpression = CreatePropertyAccess(rootExpression, restPart);
				index = indexPart;
			}
		}
	}
}