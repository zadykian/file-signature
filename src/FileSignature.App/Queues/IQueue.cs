namespace FileSignature.App.Queues;

/// <summary>
/// Queue of items of type <typeparamref name="T"/>.
/// </summary>
internal interface IQueueBase<out T> where T : notnull
{
	/// <summary>
	/// Set that queue won't be filled with new items which leads to
	/// termination of <see cref="IEnumerable{T}"/> returned by <see cref="ConsumeAsEnumerable"/> method. 
	/// </summary>
	void Complete();

	/// <summary>
	/// Represent <see cref="IQueueBase{T}"/> as sequence of <typeparamref name="T"/>. 
	/// </summary>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	IEnumerable<T> ConsumeAsEnumerable(CancellationToken cancellationToken = default);
}

/// <inheritdoc />
internal interface IQueue<T> : IQueueBase<T> where T : notnull
{
	/// <summary>
	/// Add item into sink.
	/// </summary>
	void Push(T item, CancellationToken cancellationToken = default);
}

/// <summary>
/// Priority queue.
/// </summary>
internal interface IPriorityQueue<T> : IQueueBase<T> where T : notnull
{
	/// <summary>
	/// Add item into queue with <paramref name="priority"/>.
	/// </summary>
	/// <remarks>
	/// The lower the <paramref name="priority"/>, the sooner <paramref name="item"/> will
	/// be yielded by <see cref="IQueueBase{T}.ConsumeAsEnumerable"/> method.
	/// </remarks>
	void Push(T item, uint priority, CancellationToken cancellationToken = default);
}