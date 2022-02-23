namespace FileSignature.App.Queues;

/// <summary>
/// Blocking concurrent priority queue with limited capacity.
/// </summary>
/// <typeparam name="T">
/// Type of elements.
/// </typeparam>
internal class BoundedConcurrentPriorityQueue<T> : IPriorityQueue<T>, IDisposable
	where T : notnull
{
	// todo

	/// <inheritdoc />
	void IPriorityQueue<T>.Push(T item, uint priority, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	void IQueueBase<T>.Complete()
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	IEnumerable<T> IQueueBase<T>.ConsumeAsEnumerable(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	void IDisposable.Dispose()
	{
		throw new NotImplementedException();
	}
}