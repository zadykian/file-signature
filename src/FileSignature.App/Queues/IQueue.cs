using System.Diagnostics.CodeAnalysis;

namespace FileSignature.App.Queues;

/// <summary>
/// Source of items of type <typeparamref name="T"/>.
/// </summary>
internal interface IQueueBase<T>
{
	/// <summary>
	/// Try pull element from source.
	/// </summary>
	/// <param name="item">
	/// Pulled item.
	/// </param>
	/// <returns>
	/// <c>true</c> if item is pulled, otherwise - <c>false</c>. 
	/// </returns>
	bool TryPull([NotNullWhen(returnValue: true)] out T? item);

	/// <summary>
	/// Set that queue won't be filled again.
	/// </summary>
	void Complete();

	/// <summary>
	/// Represent <see cref="IQueueBase{T}"/> as sequence of <typeparamref name="T"/>. 
	/// </summary>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	IEnumerable<T> ConsumeAsEnumerable(CancellationToken cancellationToken);
}

/// <summary>
/// Queue.
/// </summary>
internal interface IQueue<T> : IQueueBase<T>
{
	/// <summary>
	/// Add item into sink.
	/// </summary>
	void Push(T item);
}

/// <summary>
/// Priority queue.
/// </summary>
internal interface IPriorityQueue<T> : IQueueBase<T>
{
	/// <summary>
	/// Add item into queue with <paramref name="priority"/>.
	/// </summary>
	void Push(T item, uint priority);
}