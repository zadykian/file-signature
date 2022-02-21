namespace FileSignature.App.Queues.Base;

/// <summary>
/// Object with priority represented as unsigned integer.
/// </summary>
internal interface IHasPriority
{
	/// <summary>
	/// Object's priority.
	/// </summary>
	uint Priority { get; }
}