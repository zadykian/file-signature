using System.Diagnostics.CodeAnalysis;

namespace FileSignature.App.Queues;

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

	private int currentSize;
	private Node head;
	private Node tail;

	private bool isCompleted;

	public BoundedBlockingQueue(uint maxSize)
	{
		this.maxSize = (int)maxSize;
		head = new Node();
		tail = head;
	}

	/// <inheritdoc />
	void IQueue<T>.Push(T item, CancellationToken cancellationToken)
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
	void ICompletableCollection.Complete()
	{
		ThrowIfCompleted();
		isCompleted = true;

		// At this moment some readers may already be blocked by Monitor.Wait,
		// so we are notifying them to complete ConsumeAsEnumerable operation.

		lock (dequeueLock)
		{
			Monitor.PulseAll(dequeueLock);
		}
	}

	/// <inheritdoc />
	IEnumerable<T> IQueue<T>.ConsumeAsEnumerable(CancellationToken cancellationToken)
	{
		while (TryPull(cancellationToken, out var pulledValue))
		{
			yield return pulledValue;
		}
	}

	/// <summary>
	/// Try pull value from queue.
	/// </summary>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	/// <param name="pulledValue">
	/// Value pulled from queue.
	/// </param>
	/// <returns>
	/// <c>true</c> if item was pulled successfully, otherwise - false.
	/// </returns>
	/// <remarks>
	/// This method blocks current thread if there is a contention between several reader threads.
	/// Method returns <c>false</c> only if current queue was completed concurrently by other thread.
	/// </remarks>
	private bool TryPull(CancellationToken cancellationToken, [NotNullWhen(returnValue: true)] out T? pulledValue)
	{
		var queueWasFull = false;

		lock (dequeueLock)
		{
			while (head.Next == null)
			{
				if (isCompleted)
				{
					pulledValue = default;
					return false;
				}

				cancellationToken.ThrowIfCancellationRequested();
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
		if (!isCompleted)
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