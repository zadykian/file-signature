namespace FileSignature.App.Collections.Interfaces;

/// <summary>
/// Blocking key-value collection.
/// </summary>
internal interface IBlockingMap<in TKey, TValue> : ICompletableCollection
	where TKey : IEquatable<TKey>
{
	/// <summary>
	/// Add new key-value pair to map.
	/// </summary>
	void Add(TKey key, TValue value);

	/// <summary>
	/// Read and remove sequence of values from map by keys.
	/// </summary>
	/// <param name="keys">
	/// Sequence of keys.
	/// </param>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	/// <remarks>
	/// This method blocks current thread until either
	/// all items are found by keys
	/// or current map is completed by <see cref="ICompletableCollection.Complete"/> method
	/// or cancellation is requested.
	/// </remarks>
	IEnumerable<TValue> GetAndRemoveAllByKeys(IEnumerable<TKey> keys, CancellationToken cancellationToken = default);
}