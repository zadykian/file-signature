using System.Diagnostics.CodeAnalysis;

namespace FileSignature.App.Queues;

/// <summary>
/// Blocking concurrent priority queue with limited capacity.
/// </summary>
/// <typeparam name="T">
/// Type of elements.
/// </typeparam>
internal class BoundedConcurrentPriorityQueue<T> : IPriorityQueue<T>
{
	// todo

	/// <inheritdoc />
	bool IQueueBase<T>.TryPull([NotNullWhen(returnValue: true)] out T? item)
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
	void IPriorityQueue<T>.Push(T item, uint priority)
	{
		throw new NotImplementedException();
	}
}