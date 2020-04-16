using System;
using System.Collections.Generic;

namespace Mindbox.Expressions
{
	public static class ParameterHolderFactory
	{
		public static ParameterHolder Create(IList<object> parameters)
		{
			if (parameters.Count == 0)
				return null;

			if (parameters.Count > 512)
				throw new NotSupportedException("Can process 512 parameters max");

			return CreateHolderRecursive(parameters);
		}

		private static ParameterHolder CreateHolderRecursive(IList<object> parameters)
		{
			var foldedList = parameters;
			do
			{
				var resultList = new List<object>();
				var currentHolder = new ParameterHolder();
				for (var i = 0; i < foldedList.Count; i++)
				{
					if (i % 8 == 0)
					{
						currentHolder = new ParameterHolder();
						resultList.Add(currentHolder);
					}

					currentHolder[i % 8] = foldedList[i];
				}

				foldedList = resultList;

			} while (foldedList.Count > 1);

			return (ParameterHolder)foldedList[0];
		}
	}
}