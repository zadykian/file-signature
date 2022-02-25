using System.Diagnostics.CodeAnalysis;

namespace FileSignature.App.Queues;

/// <summary>
/// Blocking concurrent priority queue without capacity limit.
/// </summary>
/// <typeparam name="T">
/// Type of elements.
/// </typeparam>
internal class BlockingPriorityQueue<T> : IPriorityQueue<T>
{
	private readonly object dequeueLock = new();

	// Head never logically contains a value, it's a sentinel node.
	private readonly Node head;
	private Node tail;

	private bool isCompleted;
	private bool waitingForNewItems;

	public BlockingPriorityQueue()
	{
		head = new Node(default, default);
		tail = head;
	}

	/// <inheritdoc />
	void IPriorityQueue<T>.Push(T item, uint priority)
	{
		ThrowIfCompleted();
		var newNode = new Node(item, priority);

		Node oldTail;
		Node newTail;
		do
		{
			oldTail = tail;
			newTail = new Node(tail.Value, tail.Priority) { Next = newNode };
		} while (Interlocked.CompareExchange(ref tail, value: newTail, comparand: oldTail) != oldTail);

		if (!waitingForNewItems) return;

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
	IEnumerable<T> IPriorityQueue<T>.PullAllByPriorities(
		IEnumerable<uint> priorities,
		CancellationToken cancellationToken)
	{
		foreach (var priority in priorities)
		{
			if (TryPull(priority, out var value, cancellationToken))
			{
				yield return value;
			}
			else
			{
				yield break;
			}
		}
	}

	/// <summary>
	/// Try pull value from queue by <paramref name="priority"/>.
	/// </summary>
	/// <returns>
	/// <c>true</c> if item was pulled successfully, otherwise - false.
	/// </returns>
	/// <remarks>
	/// This method blocks current thread until either item with <paramref name="priority"/>
	/// is pushed to queue or <see cref="ICompletableCollection.Complete"/> is called.
	/// </remarks>
	private bool TryPull(
		uint priority, [NotNullWhen(returnValue: true)] out T? value, CancellationToken cancellationToken)
	{
		lock (dequeueLock)
		{
			var previousNode = head;
			var currentNode = head.Next;

			// While queue is empty or node with required priority is not found.
			while (currentNode is null || currentNode.Priority != priority)
			{
				cancellationToken.ThrowIfCancellationRequested();

				// We are at the end of linked list now, so we have 
				// to wait for new nodes to be enqueued.
				while (currentNode?.Next == null)
				{
					if (isCompleted)
					{
						value = default;
						return false;
					}

					cancellationToken.ThrowIfCancellationRequested();

					waitingForNewItems = true;
					Monitor.Wait(dequeueLock);
					waitingForNewItems = false;
				}

				previousNode = currentNode;
				currentNode = currentNode.Next;
			}

			// Value is found by priority, removing node from list.
			value = currentNode.Value!;
			previousNode.Next = currentNode.Next;
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
		public Node(T? value, uint? priority)
		{
			Value = value;
			Priority = priority;
		}

		/// <summary>
		/// Node's value.
		/// </summary>
		public readonly T? Value;

		/// <summary>
		/// Priority associated with <see cref="Value"/>;
		/// </summary>
		public readonly uint? Priority;

		/// <summary>
		/// Next node in list.
		/// </summary>
		public Node? Next;
	}
}