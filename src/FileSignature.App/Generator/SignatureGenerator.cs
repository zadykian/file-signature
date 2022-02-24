using System.Security.Cryptography;
using FileSignature.App.Queues;
using FileSignature.App.Reader;
using FileSignature.App.Scheduler;

namespace FileSignature.App.Generator;

/// <inheritdoc cref="ISignatureGenerator" />
internal class SignatureGenerator : ISignatureGenerator, IDisposable
{
	/// <summary>
	/// Number of workers to perform hashcode calculations.
	/// </summary>
	private static readonly byte hashWorkersCount = (byte) Environment.ProcessorCount;

	/// <summary>
	/// Max size for queues input and output queues.
	/// </summary>
	private static readonly ushort maxQueuesSize = (ushort) (4 * hashWorkersCount);

	/// <summary>
	/// Event which represents completion of multithreading hash calculation process.
	/// </summary>
	private readonly CountdownEvent completeBlockHashQueueEvent = new(hashWorkersCount);

	private readonly IQueue<IndexedSegment> fileBlockQueue
		= new BoundedConcurrentQueue<IndexedSegment>(maxQueuesSize);

	private readonly IPriorityQueue<IndexedSegment> blockHashQueue
		= new BoundedConcurrentPriorityQueue<IndexedSegment>(maxQueuesSize);

	private readonly IInputReader inputReader;
	private readonly IWorkScheduler workScheduler;

	public SignatureGenerator(IInputReader inputReader, IWorkScheduler workScheduler)
	{
		this.inputReader = inputReader;
		this.workScheduler = workScheduler;
	}

	/// <inheritdoc />
	IEnumerable<IndexedSegment> ISignatureGenerator.Generate(
		GenParameters genParameters, 
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		// Consume file content as sequence of blocks and push each block to fileBlockQueue.
		RunFileConsumptionProcess(genParameters, cancellationToken);

		// Consume data from fileBlockQueue, calculate hash codes and push values to blockHashQueue.
		RunHashCalculationProcess(cancellationToken);

		// Consume hash values from blockHashQueue in foreground.
		return blockHashQueue.ConsumeAsEnumerable(cancellationToken);
	}

	/// <summary>
	/// Run file consumption process in background.
	/// </summary>
	/// <param name="genParameters">
	/// Input parameters for file signature generation algorithm.
	/// </param>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	private void RunFileConsumptionProcess(GenParameters genParameters, CancellationToken cancellationToken)
		=> workScheduler.RunInBackground(() =>
		{
			inputReader
				.Read(genParameters, cancellationToken)
				.ForEach(item => fileBlockQueue.Push(item, cancellationToken));

			fileBlockQueue.Complete();
		});

	/// <summary>
	/// Run hash calculation process in background.
	/// </summary>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	private void RunHashCalculationProcess(CancellationToken cancellationToken)
	{
		// Consume data from fileBlockQueue, calculate hash codes in parallel and push results to blockHashQueue.

		workScheduler.RunInBackground(
			() => HashGenerationWorkItem(cancellationToken),
			hashWorkersCount);

		// Wait for calculation completion in background and then set blockHashQueue as completed.

		workScheduler.RunInBackground(() =>
		{
			completeBlockHashQueueEvent.Wait(cancellationToken);
			blockHashQueue.Complete();
		});
	}

	/// <summary>
	/// Calculate hash codes of file blocks from queue <see cref="fileBlockQueue"/>
	/// and push results to <see cref="blockHashQueue"/>.
	/// </summary>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	private void HashGenerationWorkItem(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using var sha256 = SHA256.Create();

		foreach (var fileBlock in fileBlockQueue.ConsumeAsEnumerable(cancellationToken))
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				var hashCodeBlock = new IndexedSegment(fileBlock.Index, sha256.HashSize * Memory.Byte);
				sha256.TryComputeHash(fileBlock.Content, hashCodeBlock.Content, out _);
				blockHashQueue.Push(hashCodeBlock, hashCodeBlock.Index, cancellationToken);
			}
			finally
			{
				fileBlock.Dispose();
			}
		}

		completeBlockHashQueueEvent.Signal();
	}

	/// <inheritdoc />
	void IDisposable.Dispose() => completeBlockHashQueueEvent.Dispose();
}