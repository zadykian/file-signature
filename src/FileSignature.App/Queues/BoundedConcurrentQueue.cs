using System.Diagnostics.CodeAnalysis;

namespace FileSignature.App.Queues;

/// <summary>
/// Blocking concurrent queue with limited capacity.
/// </summary>
/// <typeparam name="T">
/// Type of elements.
/// </typeparam>
internal class BoundedConcurrentQueue<T> : IQueue<T>
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
	void IQueue<T>.Push(T item)
	{
		throw new NotImplementedException();
	}
}