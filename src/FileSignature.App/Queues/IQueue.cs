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
	/// Add item into queue with <paramref name="priority"/>.
	/// </summary>
	void Push(T item, uint priority, CancellationToken cancellationToken = default);

	/// <summary>
	/// Add item into queue with <paramref name="priority"/>.
	/// </summary>
	/// <remarks>
	/// This method blocks current thread until either item with <paramref name="priority"/>
	/// is pushed to queue or <see cref="ICompletableCollection.Complete"/> is called.
	/// </remarks>
	T Pull(uint priority, CancellationToken cancellationToken = default);
}