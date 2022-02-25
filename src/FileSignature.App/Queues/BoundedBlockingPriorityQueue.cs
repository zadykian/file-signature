namespace FileSignature.App.Queues;

/// <summary>
/// Blocking concurrent priority queue with limited capacity.
/// </summary>
/// <typeparam name="T">
/// Type of elements.
/// </typeparam>
internal class BoundedBlockingPriorityQueue<T> : IPriorityQueue<T>
	where T : notnull
{
	// todo

	private readonly int maxSize;

	public BoundedBlockingPriorityQueue(uint maxSize) => this.maxSize = (int)maxSize;

	/// <inheritdoc />
	void IPriorityQueue<T>.Push(T item, uint priority, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	T IPriorityQueue<T>.Pull(uint priority, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	void ICompletableCollection.Complete()
	{
		throw new NotImplementedException();
	}
}