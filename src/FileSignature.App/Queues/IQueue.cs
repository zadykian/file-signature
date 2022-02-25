using System.Diagnostics.CodeAnalysis;

namespace FileSignature.App.Queues;

/// <summary>
/// Completable collection.
/// </summary>
internal interface ICompletableCollection
{
	/// <summary>
	/// Set that queue won't be filled with new items again.
	/// </summary>
	void Complete();
}

/// <summary>
/// Queue of items of type <typeparamref name="T"/>.
/// </summary>
internal interface IQueue<T> : ICompletableCollection
{
	/// <summary>
	/// Add item into sink.
	/// </summary>
	void Push(T item, CancellationToken cancellationToken = default);

	/// <summary>
	/// Represent <see cref="IQueue{T}"/> as sequence of <typeparamref name="T"/>. 
	/// </summary>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	IEnumerable<T> ConsumeAsEnumerable(CancellationToken cancellationToken = default);
}

/// <summary>
/// Priority queue.
/// </summary>
internal interface IPriorityQueue<T> : ICompletableCollection
{
	/// <summary>
	/// Add <paramref name="item"/> into queue with <paramref name="priority"/>.
	/// </summary>
	void Push(T item, uint priority);

	/// <summary>
	/// Pull sequence of values from queue by priorities.
	/// </summary>
	/// <param name="priorities">
	/// Sequence of priorities associated with values of type <typeparamref name="T"/>.
	/// </param>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	/// <remarks>
	/// This method blocks current thread until either all items are found by priority
	/// or current queue is completed by <see cref="ICompletableCollection.Complete"/> method.
	/// </remarks>
	IEnumerable<T> PullAllByPriorities(IEnumerable<uint> priorities, CancellationToken cancellationToken = default);
}