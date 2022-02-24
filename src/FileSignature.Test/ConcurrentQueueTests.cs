using System.Collections.Concurrent;
using FileSignature.App.Queues;
using NUnit.Framework;
using TypeDecorators.Lib.Extensions;

#pragma warning disable CS4014

namespace FileSignature.Test;

/// <summary>
/// Tests of <see cref="BoundedConcurrentQueue{T}"/> component.
/// </summary>
public class ConcurrentQueueTests : TestBase
{
	private static IQueue<T> Queue<T>() where T : notnull => new BoundedConcurrentQueue<T>(64u);

	/// <summary>
	/// Attempt to push new item to queue after its' completion leads
	/// to <see cref="InvalidOperationException"/>.
	/// </summary>
	[Test]
	public void PushingAfterCompletion()
	{
		var queue = Queue<int>();
		queue.Push(default);
		queue.Complete();
		Assert.Throws<InvalidOperationException>(() => queue.Push(default));
	}

	/// <summary>
	/// Single publisher and single consumer working concurrently.
	/// </summary>
	[Test]
	[Timeout(1000)]
	public async Task SinglePublisherSingleConsumer()
	{
		var queue = Queue<Guid>();
		var items = TestItems();
		RunPublisherTask(queue, items);

		await Task.Run(() =>
		{
			var result = queue.ConsumeAsEnumerable().ToArray();
			Assert.IsTrue(
				result.SequenceEqual(items),
				$"actual (Length: {result.Length}): [{string.Join(",",result)}]");
		});
	}

	/// <summary>
	/// Single publisher and multiple consumers working concurrently.
	/// </summary>
	[Test]
	[Timeout(1000)]
	public async Task SinglePublisherMultipleConsumers()
	{
		var queue = Queue<Guid>();
		var items = TestItems();
		RunPublisherTask(queue, items);

		var allConsumed = new ConcurrentBag<Guid>();

		var tasks = Enumerable
			.Range(1, 8)
			.Select(_ => new Task(() => queue.ConsumeAsEnumerable().ForEach(allConsumed.Add)))
			.YieldForEach(task => task.Start());

		await Task.WhenAll(tasks);

		Assert.IsTrue(
			allConsumed.OrderBy(x => x).SequenceEqual(items.OrderBy(x => x)));
	}

	/// <summary>
	/// Generate test items to push and pull from queue. 
	/// </summary>
	private static IReadOnlyCollection<Guid> TestItems()
		=> Enumerable
			.Range(0, 32768)
			.Select(_ => Guid.NewGuid())
			.ToArray();

	/// <summary>
	/// Run <see cref="Task"/> witch pushes <paramref name="itemsToPush"/> to <paramref name="queue"/>
	/// and that completes <paramref name="queue"/>.
	/// </summary>
	private static void RunPublisherTask<T>(IQueue<T> queue, IEnumerable<T> itemsToPush)
		where T : notnull
		=> Task.Run(() =>
		{
			foreach (var item in itemsToPush) queue.Push(item);
			queue.Complete();
		});
}