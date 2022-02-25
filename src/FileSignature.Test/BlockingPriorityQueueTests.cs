using FileSignature.App.Queues;
using NUnit.Framework;

#pragma warning disable CS4014

namespace FileSignature.Test;

/// <summary>
/// Tests of <see cref="BlockingPriorityQueue{T}"/> component.
/// </summary>
public class BlockingPriorityQueueTests : TestBase
{
	/// <summary>
	/// Create new <see cref="IPriorityQueue{T}"/> instance.
	/// </summary>
	private static IPriorityQueue<T> Queue<T>() => new BlockingPriorityQueue<T>();

	/// <summary>
	/// Generate test items to push and pull from queue. 
	/// </summary>
	private static IReadOnlyCollection<(Guid Value, uint Priority)> TestItems()
		=> Enumerable
			.Range(0, 32768)
			.Select(priority => (Guid.NewGuid(), (uint) priority))
			.ToArray();

	/// <summary>
	/// Attempt to push new item to queue after its' completion leads
	/// to <see cref="InvalidOperationException"/>.
	/// </summary>
	[Test]
	public void PushingAfterCompletion()
	{
		var queue = Queue<int>();
		queue.Push(default, 1);
		queue.Complete();
		Assert.Throws<InvalidOperationException>(() => queue.Push(default, 0));
	}

	/// <summary>
	/// Single publisher and single consumer working concurrently.
	/// </summary>
	[Test]
	[Timeout(1000)]
	public async Task SinglePublisherSingleSubscriber()
	{
		var queue = Queue<Guid>();
		var items = TestItems();

		Task.Run(() =>
		{
			foreach (var (item, priority) in items) queue.Push(item, priority);
			queue.Complete();
		});

		await Task.Run(() =>
		{
			var result = queue
				.PullAllByPriorities(items.Select(item => item.Priority))
				.ToArray();

			Assert.IsTrue(
				result.SequenceEqual(items.Select(item => item.Value)),
				$"actual (Length: {result.Length}): [{string.Join(",",result)}]");
		});
	}
}