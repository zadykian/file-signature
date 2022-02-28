namespace FileSignature.App.Collections.Interfaces;

/// <summary>
/// Queue of items of type <typeparamref name="T"/>.
/// </summary>
internal interface IQueue<T> : ICompletableCollection
{
	/// <summary>
	/// Add item to queue.
	/// </summary>
	void Push(T item);

	/// <summary>
	/// Pull all values from <see cref="IQueue{T}"/> as sequence of <typeparamref name="T"/>. 
	/// </summary>
	IEnumerable<T> ConsumeAsEnumerable();
}