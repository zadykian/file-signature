namespace FileSignature.App.Workers.Base;

/// <summary>
/// Background worker.
/// </summary>
internal interface IWorker
{
	/// <summary>
	/// Number of workers available to run in parallel.
	/// </summary>
	byte DegreeOfParallelism { get; }

	/// <summary>
	/// Run worker.
	/// </summary>
	void Run();
}