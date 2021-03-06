using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileSignature.App.Scheduler;

/// <inheritdoc />
internal class ThreadWorkScheduler : IWorkScheduler
{
	private readonly IHostApplicationLifetime applicationLifetime;
	private readonly ILogger<ThreadWorkScheduler> logger;

	public ThreadWorkScheduler(IHostApplicationLifetime applicationLifetime, ILogger<ThreadWorkScheduler> logger)
	{
		this.applicationLifetime = applicationLifetime;
		this.logger = logger;
	}

	/// <inheritdoc />
	void IWorkScheduler.RunInBackground(Action workItem, string workerName, byte degreeOfParallelism)
	{
		if (degreeOfParallelism == default)
		{
			throw new ArgumentException("Degree of parallelism must be positive.", nameof(degreeOfParallelism));
		}

		void RunWorkOnThread() => CreateThread(workItem, workerName).Start();

		if (degreeOfParallelism == 1)
		{
			RunWorkOnThread();
			return;
		}

		Enumerable.Range(1, degreeOfParallelism).ForEach(_ => RunWorkOnThread());
	}

	/// <summary>
	/// Create <see cref="Thread"/> to execute <paramref name="workItem"/>. 
	/// </summary>
	private Thread CreateThread(Action workItem, string workerName)
	{
		void WrappedWorkItem(object? _)
		{
			var threadId = Environment.CurrentManagedThreadId;
			logger.LogTrace("Starting worker '{workerName}' with ThreadId={threadId}.", workerName, threadId);

			try
			{
				workItem();
			}
			catch (OperationCanceledException)
			{
				// If cancellation was requested, just terminate current worker thread.
				logger.LogTrace("Worker '{workerName}' with ThreadId={threadId} is cancelled.", workerName, threadId);
				return;
			}
			catch (Exception exception)
			{
				logger.LogError(exception,
					"Error occured in worker '{workerName}' with ThreadId={threadId}, requesting app termination.",
					workerName, threadId);

				// It's considered that there is no reason to continue app execution if one of workers failed.
				// So, instead of unhandled exception, graceful shutdown is initiated by
				// requesting cancellation via CancellationToken objects propagated to other workers.

				applicationLifetime.StopApplication();
			}

			logger.LogTrace("Worker '{workerName}' with ThreadId={threadId} is terminated.", workerName, threadId);
		}

		return new Thread(WrappedWorkItem) { IsBackground = true };
	}
}