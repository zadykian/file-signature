namespace FileSignature.App.Queues.Base;

/// <summary>
/// Destination for items of type <typeparamref name="T"/>.
/// </summary>
public interface ISink<in T>
{
	/// <summary>
	/// Add item into sink.
	/// </summary>
	void Push(T item);

	/// <summary>
	/// Set that <see cref="Push"/> method won't be called again on this object.
	/// </summary>
	void Complete();
}