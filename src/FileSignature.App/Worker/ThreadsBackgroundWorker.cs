namespace FileSignature.App.Worker;

/// <inheritdoc />
internal class ThreadsBackgroundWorker : IBackgroundWorker
{
	/// <inheritdoc />
	void IBackgroundWorker.Enqueue(Action workUnit, byte degreeOfParallelism)
	{
		if (degreeOfParallelism == default)
		{
			throw new ArgumentException("Degree of parallelism must be positive.", nameof(degreeOfParallelism));
		}

		void RunWorkOnThread() => new Thread(_ => workUnit()).Start();

		if (degreeOfParallelism == 1)
		{
			RunWorkOnThread();
			return;
		}

		Enumerable.Range(1, degreeOfParallelism).ForEach(_ => RunWorkOnThread());
	}
}