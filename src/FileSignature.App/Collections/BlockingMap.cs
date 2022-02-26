using System.Diagnostics.CodeAnalysis;
using FileSignature.App.Collections.Interfaces;

// ReSharper disable InconsistentlySynchronizedField

namespace FileSignature.App.Collections;

/// <inheritdoc />
internal class BlockingMap<TKey, TValue> : IBlockingMap<TKey, TValue>
	where TKey : IEquatable<TKey>
{
	private readonly object writerLock = new();
	private readonly Dictionary<TKey, TValue> dictionary;

	private bool isCompleted;

	public BlockingMap(uint initialCapacity)
		=> dictionary = new Dictionary<TKey, TValue>((int)initialCapacity);

	/// <inheritdoc />
	void IBlockingMap<TKey, TValue>.Add(TKey key, TValue value)
	{
		ThrowIfCompleted();

		lock (writerLock)
		{
			dictionary.Add(key, value);
		}
	}

	/// <inheritdoc />
	void ICompletableCollection.Complete()
	{
		ThrowIfCompleted();
		isCompleted = true;
	}

	/// <inheritdoc />
	IEnumerable<TValue> IBlockingMap<TKey, TValue>.GetAndRemoveAllByKeys(
		IEnumerable<TKey> keys,
		CancellationToken cancellationToken)
	{
		foreach (var key in keys)
		{
			if (TryGetAndRemove(key, out var value, cancellationToken)) yield return value;
			else yield break;
		}
	}

	/// <summary>
	/// Try get and remove <paramref name="value"/> from map by <paramref name="key"/>.
	/// </summary>
	/// <returns>
	/// <c>true</c> if item was retrieved successfully, otherwise - false.
	/// </returns>
	/// <remarks>
	/// This method blocks current thread until either item with <paramref name="key"/>
	/// is added to map or <see cref="ICompletableCollection.Complete"/> is called.
	/// </remarks>
	private bool TryGetAndRemove(
		TKey key, [NotNullWhen(returnValue: true)] out TValue? value, CancellationToken cancellationToken)
	{
		SpinWait.SpinUntil(() =>
			dictionary.ContainsKey(key)
			|| isCompleted
			|| cancellationToken.IsCancellationRequested);

		cancellationToken.ThrowIfCancellationRequested();

		if (isCompleted && !dictionary.ContainsKey(key))
		{
			value = default;
			return false;
		}

		lock (writerLock)
		{
			value = dictionary[key]!;
			dictionary.Remove(key);
			return true;
		}
	}

	/// <summary>
	/// Throw <see cref="InvalidOperationException"/> if queue was already completed.
	/// </summary>
	private void ThrowIfCompleted()
	{
		if (!isCompleted)
		{
			return;
		}

		throw new InvalidOperationException("Queue is already completed!");
	}
}