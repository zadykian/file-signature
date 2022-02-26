using FileSignature.App.Collections;
using FileSignature.App.Collections.Interfaces;
using NUnit.Framework;

#pragma warning disable CS4014

namespace FileSignature.Test;

/// <summary>
/// Tests of <see cref="BlockingMap{TKey, TValue}"/> component.
/// </summary>
public class BlockingMapTests : TestBase
{
	/// <summary>
	/// Create new <see cref="IBlockingMap{TKey,TValue}"/> instance.
	/// </summary>
	private static IBlockingMap<TKey, TValue> Map<TKey, TValue>()
		where TKey : IEquatable<TKey>
		=> new BlockingMap<TKey, TValue>(initialCapacity: 64u);

	/// <summary>
	/// Generate test items to and and retrieve from map. 
	/// </summary>
	private static IReadOnlyCollection<(uint Key, Guid Value)> TestItems()
		=> Enumerable
			.Range(0, 32768)
			.Select(key => ((uint) key, Guid.NewGuid()))
			.ToArray();

	/// <summary>
	/// Attempt to add new item to blockingMap after its' completion leads
	/// to <see cref="InvalidOperationException"/>.
	/// </summary>
	[Test]
	public void PushingAfterCompletion()
	{
		var blockingMap = Map<int, int>();
		blockingMap.Add(default, 1);
		blockingMap.Complete();
		Assert.Throws<InvalidOperationException>(() => blockingMap.Add(default, 0));
	}

	/// <summary>
	/// Single publisher and single consumer working concurrently.
	/// </summary>
	[Test]
	[Timeout(1000)]
	public async Task SinglePublisherSingleConsumer()
	{
		var blockingMap = Map<uint, Guid>();
		var items = TestItems();

		Task.Run(() =>
		{
			foreach (var (key, value) in items) blockingMap.Add(key, value);
			blockingMap.Complete();
		});

		await RunConsumerTask<uint, Guid>(blockingMap, items);
	}

	/// <summary>
	/// Multiple publishers and single consumer working concurrently.
	/// </summary>
	[Test]
	[Timeout(1000)]
	public async Task MultiplePublishersSingleConsumer()
	{
		var blockingMap = Map<uint, Guid>();
		var items = TestItems();

		Task.Run(() =>
		{
			items
				.AsParallel()
				.WithDegreeOfParallelism(8)
				.ForAll(item => blockingMap.Add(item.Key, item.Value));

			blockingMap.Complete();
		});

		await RunConsumerTask<uint, Guid>(blockingMap, items);
	}

	/// <summary>
	/// Run <see cref="Task"/> which reads items from <paramref name="blockingMap"/>
	/// and then compared retrieved items with <paramref name="items"/>.
	/// </summary>
	private static Task RunConsumerTask<TKey, TValue>(
		IBlockingMap<uint, TValue> blockingMap,
		IReadOnlyCollection<(uint Key, TValue Value)> items)
		where TKey : IEquatable<TKey>
		=> Task.Run(() =>
		{
			var result = blockingMap
				.GetAndRemoveAllByKeys(items.Select(item => item.Key))
				.ToArray();

			Assert.IsTrue(
				result.SequenceEqual(items.Select(item => item.Value)),
				$"actual (Length: {result.Length}): [{string.Join(",",result)}]");
		});
}