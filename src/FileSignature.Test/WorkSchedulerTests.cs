using FileSignature.App.Scheduler;
using NUnit.Framework;

namespace FileSignature.Test;

/// <summary>
/// Tests of <see cref="IWorkScheduler"/> component.
/// </summary>
public class WorkSchedulerTests : TestBase
{
	/// <summary>
	/// Run several delegates in parallel and wait for completion.
	/// </summary>
	[Test]
	[Timeout(1000)]
	public async Task EnqueuedDelegatesRunsSuccessfully()
	{
		IWorkScheduler workScheduler = new ThreadWorkScheduler(new TokenAppLifetime(), Logger<ThreadWorkScheduler>());
		const int workersCount = 4;

		var completionSource = new TaskCompletionSource();
		var counter = 0;

		workScheduler.RunInBackground(() =>
		{
			Thread.Sleep(10);
			Interlocked.Increment(ref counter);
			if (counter == workersCount) completionSource.SetResult();
		}, "test worker", degreeOfParallelism: workersCount);

		await completionSource.Task;
		Assert.Pass();
	}

	/// <summary>
	/// Unhandled exception in background worker initiates application's graceful shutdown.
	/// </summary>
	[Test]
	[Timeout(1000)]
	public void ExceptionInWorkerLeadsToCancellation()
	{
		var lifetimeManager = new TokenAppLifetime();
		IWorkScheduler workScheduler = new ThreadWorkScheduler(lifetimeManager, Logger<ThreadWorkScheduler>());

		workScheduler.RunInBackground(() =>
		{
			Thread.Sleep(10);
			throw new ApplicationException("unhandled exception in background worker!");
		}, "test worker");

		SpinWait.SpinUntil(() => lifetimeManager.TokenSource.IsCancellationRequested);
		Assert.Pass();
	}
}