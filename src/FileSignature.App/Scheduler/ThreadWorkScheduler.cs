using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileSignature.App.Scheduler;

/// <inheritdoc />
internal class ThreadWorkScheduler : IWorkScheduler
{
	private readonly IHostApplicationLifetime applicationLifetime;
	private readonly ILogger<ThreadWorkScheduler> logger;

	public ThreadWorkScheduler(
		IHostApplicationLifetime applicationLifetime,
		ILogger<ThreadWorkScheduler> logger)
	{
		this.applicationLifetime = applicationLifetime;
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
			catch (Exception e)
			{
				logger.LogError(e, "Error occured in background thread.");
				// Gracefully shutdown application instead of unhandled exception.
				applicationLifetime.StopApplication();
			}
		}

		return new Thread(WrappedWorkItem) { IsBackground = true };
	}
}