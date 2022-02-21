namespace FileSignature.App.Workers.Base;

/// <inheritdoc />
internal abstract class WorkerBase : IWorker
{
	/// <inheritdoc />
	public abstract byte DegreeOfParallelism { get; }

	/// <inheritdoc />
	void IWorker.Run()
		=> Enumerable
			.Range(1, DegreeOfParallelism)
			.Select(_ => new Thread(_ => DoJob()))
			.ForEach(thread => thread.Start());

	/// <summary>
	/// Perform job.
	/// </summary>
	/// <remarks>
	/// This methods can runs on different threads in parallel based on <see cref="DegreeOfParallelism"/>. 
	/// </remarks>
	protected abstract void DoJob();
}