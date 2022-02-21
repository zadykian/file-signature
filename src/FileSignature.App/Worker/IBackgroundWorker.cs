namespace FileSignature.App.Worker;

/// <summary>
/// Background worker.
/// </summary>
internal interface IBackgroundWorker
{
	/// <summary>
	/// Enqueue <paramref name="workUnit"/> to worker.
	/// </summary>
	/// <param name="workUnit">
	/// Work unit.
	/// It runs in parallel depending on <paramref name="degreeOfParallelism"/>.
	/// </param>
	/// <param name="degreeOfParallelism">
	/// Degree of parallelism.
	/// </param>
	void Enqueue(Action workUnit, byte degreeOfParallelism = 1);
}