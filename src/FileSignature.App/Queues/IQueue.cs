namespace FileSignature.App.Queues;

/// <summary>
/// Queue of items of type <typeparamref name="T"/>.
/// </summary>
internal interface IQueueBase<out T>
{
	/// <summary>
	/// Set that queue won't be filled with items again.
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

/// <inheritdoc />
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