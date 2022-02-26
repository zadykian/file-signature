using System.Security.Cryptography;
using FileSignature.App.Collections;
using FileSignature.App.Collections.Interfaces;
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
	/// Size of intermediate buffers to store produced values.
	/// </summary>
	private static readonly ushort buffersSize = (ushort) (4 * hashWorkersCount);

	/// <summary>
	/// Event which represents completion of multithreading hash calculation process.
	/// </summary>
	private readonly CountdownEvent completeBlockHashQueueEvent = new(hashWorkersCount);

	/// <summary>
	/// Input file blocks queue which is consumed in parallel by hash calculation workers.
	/// </summary>
	private readonly IQueue<IndexedSegment> fileBlockInputQueue
		= new BoundedBlockingQueue<IndexedSegment>(buffersSize);

	/// <summary>
	/// Output hash codes map in which workers add results in parallel.
	/// </summary>
	private readonly IBlockingMap<uint, IndexedSegment> blockHashOutputMap
		= new BlockingMap<uint, IndexedSegment>(buffersSize);

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

		// Consume file content as sequence of blocks and push each block to fileBlockInputQueue.
		RunFileConsumptionProcess(genParameters, cancellationToken);

		// Consume data from fileBlockInputQueue, calculate hash codes and push values to blockHashOutputMap.
		RunHashCalculationProcess(cancellationToken);

		// Consume hash values from blockHashOutputMap in foreground.
		var blockIndexRange = Enumerable.Range(0, int.MaxValue).Select(blockIndex => (uint)blockIndex);
		return blockHashOutputMap.GetAndRemoveAllByKeys(blockIndexRange, cancellationToken);
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
				.ForEach(item => fileBlockInputQueue.Push(item, cancellationToken));

			fileBlockInputQueue.Complete();
		});

	/// <summary>
	/// Run hash calculation process in background.
	/// </summary>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	private void RunHashCalculationProcess(CancellationToken cancellationToken)
	{
		// Consume data from fileBlockInputQueue, calculate hash codes
		// in parallel and push results to blockHashOutputMap.

		workScheduler.RunInBackground(
			() => HashGenerationWorkItem(cancellationToken),
			hashWorkersCount);

		// Wait for calculation completion in background and then set
		// blockHashOutputMap as completed.

		workScheduler.RunInBackground(() =>
		{
			completeBlockHashQueueEvent.Wait(cancellationToken);
			blockHashOutputMap.Complete();
		});
	}

	/// <summary>
	/// Calculate hash codes of file blocks from queue <see cref="fileBlockInputQueue"/>
	/// and push results to <see cref="blockHashOutputMap"/>.
	/// </summary>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	private void HashGenerationWorkItem(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using var sha256 = SHA256.Create();

		foreach (var fileBlock in fileBlockInputQueue.ConsumeAsEnumerable(cancellationToken))
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				var hashCodeBlock = new IndexedSegment(fileBlock.Index, sha256.HashSize / 8 * Memory.Byte);
				sha256.TryComputeHash(fileBlock.Content, hashCodeBlock.Content, out _);
				blockHashOutputMap.Add(hashCodeBlock.Index, hashCodeBlock);
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