namespace FileSignature.App.Collections.Interfaces;

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
	/// Pull all values from <see cref="IQueue{T}"/> as sequence of <typeparamref name="T"/>. 
	/// </summary>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	IEnumerable<T> ConsumeAsEnumerable(CancellationToken cancellationToken = default);
}