using System.Diagnostics.CodeAnalysis;
using FileSignature.App.Collections.Interfaces;

// ReSharper disable InconsistentNaming

namespace FileSignature.App.Collections;

/// <summary>
/// Blocking concurrent queue with limited capacity.
/// </summary>
/// <typeparam name="T">
/// Type of elements.
/// </typeparam>
internal class BoundedBlockingQueue<T> : IQueue<T>
{
	private readonly object enqueueLock = new();
	private readonly object dequeueLock = new();

	private readonly int maxSize;
	private readonly CancellationToken cancellationToken;

	private int currentSize;
	private Node head;
	private Node tail;

	// We use integer as logical value here for Interlocked operations support.
	private int completionState;
	private const int NotCompleted = 0;
	private const int Completed = 1;

	public BoundedBlockingQueue(uint maxSize, CancellationToken cancellationToken = default)
	{
		this.maxSize = (int)maxSize;
		head = new Node();
		tail = head;

		// In case of cancellation we must complete current queue
		// to unlock writers or/and readers locked by Monitor.Wait.
		cancellationToken.Register(Complete);
		this.cancellationToken = cancellationToken;
	}

	/// <inheritdoc />
	void IQueue<T>.Push(T item)
	{
		ThrowIfCompleted();
		var queueWasEmpty = false;
		var newNode = new Node(item);

		lock (enqueueLock)
		{
			while (currentSize == maxSize)
			{
				cancellationToken.ThrowIfCancellationRequested();
				Monitor.Wait(enqueueLock);
			}

			tail.Next = newNode;
			tail = newNode;

			// Queue was empty, maybe some readers are blocked.
			if (Interlocked.Increment(ref currentSize) == 1)
			{
				queueWasEmpty = true;
			}
		}

		if (!queueWasEmpty) return;

		lock (dequeueLock)
		{
			Monitor.PulseAll(dequeueLock);
		}
	}

	/// <inheritdoc />
	public void Complete()
	{
		if (Interlocked.Exchange(ref completionState, Completed) != NotCompleted)
		{
			return;
		}

		// At this moment some writers or/and readers may already be blocked by Monitor.Wait,
		// so we are notifying them to either cancel or complete their operations.

		lock (enqueueLock)
		{
			Monitor.PulseAll(enqueueLock);
		}

		lock (dequeueLock)
		{
			Monitor.PulseAll(dequeueLock);
		}
	}

	/// <inheritdoc />
	IEnumerable<T> IQueue<T>.ConsumeAsEnumerable()
	{
		while (TryPull(out var pulledValue))
		{
			yield return pulledValue;
		}
	}

	/// <summary>
	/// Try pull value from queue.
	/// </summary>
	/// <param name="pulledValue">
	/// Value pulled from queue.
	/// </param>
	/// <returns>
	/// <c>true</c> if item was pulled successfully, otherwise - false.
	/// </returns>
	/// <remarks>
	/// This method blocks current thread if there is a contention between several reader threads
	/// or if queue is reached its' capacity limit.
	/// Method returns <c>false</c> only if current queue was completed concurrently by other thread.
	/// </remarks>
	private bool TryPull([NotNullWhen(returnValue: true)] out T? pulledValue)
	{
		var queueWasFull = false;

		lock (dequeueLock)
		{
			while (head.Next == null)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (completionState == Completed)
				{
					pulledValue = default;
					return false;
				}

				Monitor.Wait(dequeueLock);
			}

			pulledValue = head.Next.Value!;
			head = head.Next;

			// Queue was full, maybe some writers are blocked.
			if (Interlocked.Decrement(ref currentSize) == maxSize - 1)
			{
				queueWasFull = true;
			}
		}

		if (!queueWasFull) return true;

		lock (enqueueLock)
		{
			Monitor.PulseAll(enqueueLock);
		}

		return true;
	}

	/// <summary>
	/// Throw <see cref="InvalidOperationException"/> if queue was already completed.
	/// </summary>
	private void ThrowIfCompleted()
	{
		if (completionState != Completed)
		{
			return;
		}

		throw new InvalidOperationException("Queue is already completed!");
	}

	/// <summary>
	/// Linked list node.
	/// </summary>
	private sealed class Node
	{
		public Node(T? value = default) => Value = value;

		/// <summary>
		/// Node's value.
		/// </summary>
		public readonly T? Value;

		/// <summary>
		/// Next node in list.
		/// </summary>
		public Node? Next;
	}
}