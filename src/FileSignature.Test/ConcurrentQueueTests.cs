using FileSignature.App.Queues;
using NUnit.Framework;

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
	[Timeout(10000)]
	public async Task SinglePublisherSingleConsumer()
	{
		var queue = Queue<Guid>();

		var items = Enumerable
			.Range(0, 128)
			.Select(_ => Guid.NewGuid())
			.ToArray();

		Task.Run(() =>
		{
			foreach (var item in items) queue.Push(item);
			queue.Complete();
		});

		await Task.Run(() =>
		{
			var result = queue.ConsumeAsEnumerable().ToArray();
			Assert.IsTrue(result.SequenceEqual(items));
		});
	}
}