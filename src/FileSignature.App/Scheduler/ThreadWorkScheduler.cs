using Microsoft.Extensions.Logging;

namespace FileSignature.App.Scheduler;

/// <inheritdoc />
internal class ThreadWorkScheduler : IWorkScheduler
{
	private readonly ILifetimeManager lifetimeManager;
	private readonly ILogger<ThreadWorkScheduler> logger;

	public ThreadWorkScheduler(ILifetimeManager lifetimeManager, ILogger<ThreadWorkScheduler> logger)
	{
		this.lifetimeManager = lifetimeManager;
		this.logger = logger;
	}

	/// <inheritdoc />
	void IWorkScheduler.RunInBackground(Action workItem, byte degreeOfParallelism)
	{
		if (degreeOfParallelism == default)
		{
			throw new ArgumentException("Degree of parallelism must be positive.", nameof(degreeOfParallelism));
		}

		void RunWorkOnThread() => CreateThread(workItem).Start();

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
	private Thread CreateThread(Action workItem)
	{
		void WrappedWorkItem(object? _)
		{
			try
			{
				workItem();
			}
			catch (OperationCanceledException)
			{
				// If cancellation was requested, just terminate current worker thread.
			}
			catch (Exception e)
			{
				logger.LogError(e, "Error occured in background thread, requesting app termination.");

				// It's considered that there is no reason to continue app execution if one of workers failed.
				// So, instead of unhandled exception, graceful shutdown is initiated by
				// requesting cancellation via CancellationToken objects propagated to other workers.

				lifetimeManager.RequestAppCancellation();
			}
		}

		return new Thread(WrappedWorkItem) { IsBackground = true };
	}
}