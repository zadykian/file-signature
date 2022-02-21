namespace FileSignature.App.Scheduler;

/// <summary>
/// Background work scheduler.
/// </summary>
internal interface IWorkScheduler
{
	/// <summary>
	/// Run <paramref name="workItem"/> in background.
	/// </summary>
	/// <param name="workItem">
	/// Work item delegate.
	/// Runs in parallel depending on <paramref name="degreeOfParallelism"/>.
	/// </param>
	/// <param name="degreeOfParallelism">
	/// Degree of parallelism.
	/// </param>
	void RunInBackground(Action workItem, byte degreeOfParallelism = 1);
}