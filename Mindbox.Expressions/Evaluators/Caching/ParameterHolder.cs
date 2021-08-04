using System;

namespace Mindbox.Expressions
{
	public class ParameterHolder
	{
		public object P0 { get; private set; }
		public object P1 { get; private set; }
		public object P2 { get; private set; }
		public object P3 { get; private set; }
		public object P4 { get; private set; }
		public object P5 { get; private set; }
		public object P6 { get; private set; }
		public object P7 { get; private set; }

		public object this[int idx]
		{
			set
			{
				switch (idx)
				{
					case 0: P0 = value; break;
					case 1: P1 = value; break;
					case 2: P2 = value; break;
					case 3: P3 = value; break;
					case 4: P4 = value; break;
					case 5: P5 = value; break;
					case 6: P6 = value; break;
					case 7: P7 = value; break;
					default:
						throw new IndexOutOfRangeException();
				}
			}
		}
	}
}