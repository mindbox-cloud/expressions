namespace Mindbox.Expressions
{
	public struct EvaluationResult
	{
		private static readonly object ImpossibleValue = new object();

		public object Value { get; private set; }

		public static EvaluationResult Impossible { get; } = new EvaluationResult { Value = ImpossibleValue };

		public static EvaluationResult Of(object result) => new EvaluationResult { Value = result };

		public bool IsImpossible => Value == Impossible.Value;
	}
}