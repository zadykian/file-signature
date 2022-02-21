using FileSignature.App.Queues.Base;

namespace FileSignature.App.Queues;

/// <summary>
/// Blocking concurrent queue with limited capacity.
/// </summary>
/// <typeparam name="T">
/// Type of elements.
/// </typeparam>
internal class BoundedConcurrentQueue<T> : ISink<T>, ISource<T>
{
	// todo

	/// <inheritdoc />
	void ISink<T>.Push(T item)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	void ISink<T>.Complete()
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	bool ISource<T>.ShouldPullNext()
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	T ISource<T>.Pull()
	{
		throw new NotImplementedException();
	}
}