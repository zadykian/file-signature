using System.Security.Cryptography;
using FileSignature.App.Collections;
using FileSignature.App.Collections.Interfaces;
using FileSignature.App.Reader;
using FileSignature.App.Scheduler;

namespace FileSignature.App.Generator;

/// <inheritdoc cref="ISignatureGenerator" />
internal class SignatureGenerator : ISignatureGenerator
{
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

		var context = new CalculationContext(genParameters);

		// Consume file content as sequence of blocks
		// and push each block to CalculationContext.FileBlockInputQueue.
		RunFileConsumptionProcess(context, cancellationToken);

		// Consume data from CalculationContext.FileBlockInputQueue, calculate
		// hash codes and push values to CalculationContext.BlockHashOutputMap.
		RunHashCalculationProcess(context, cancellationToken);

		// Consume hash values from CalculationContext.BlockHashOutputMap
		// in foreground.
		var blockIndexRange = Enumerable.Range(0, int.MaxValue).Select(blockIndex => (uint)blockIndex);
		return context.BlockHashOutputMap.GetAndRemoveAllByKeys(blockIndexRange, cancellationToken);
	}

	/// <summary>
	/// Run file consumption process in background.
	/// </summary>
	/// <param name="context">
	/// Hash codes generation context.
	/// </param>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	private void RunFileConsumptionProcess(CalculationContext context, CancellationToken cancellationToken)
		=> workScheduler.RunInBackground(() =>
		{
			inputReader
				.Read(context.GenParameters, cancellationToken)
				.ForEach(item => context.FileBlockInputQueue.Push(item, cancellationToken));

			context.FileBlockInputQueue.Complete();
		});

	/// <summary>
	/// Run hash calculation process in background.
	/// </summary>
	/// <param name="context">
	/// Hash codes generation context.
	/// </param>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	private void RunHashCalculationProcess(CalculationContext context, CancellationToken cancellationToken)
	{
		// Consume data from CalculationContext.FileBlockInputQueue, calculate hash codes
		// in parallel and push results to CalculationContext.BlockHashOutputMap.

		workScheduler.RunInBackground(
			() => HashGenerationWorkItem(context, cancellationToken),
			context.GenParameters.HashWorkersCount);

		// Wait for calculation completion in background and then set
		// blockHashOutputMap as completed.

		workScheduler.RunInBackground(() =>
		{
			context.CompleteBlockHashQueueEvent.Wait(cancellationToken);
			context.BlockHashOutputMap.Complete();
		});
	}

	/// <summary>
	/// Calculate hash codes of file blocks from queue <see cref="CalculationContext.FileBlockInputQueue"/>
	/// and push results to <see cref="CalculationContext.FileBlockInputQueue"/>.
	/// </summary>
	/// <param name="context">
	/// Hash codes generation context.
	/// </param>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	private static void HashGenerationWorkItem(CalculationContext context, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using var sha256 = SHA256.Create();

		foreach (var fileBlock in context.FileBlockInputQueue.ConsumeAsEnumerable(cancellationToken))
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				var hashCodeBlock = new IndexedSegment(fileBlock.Index, sha256.HashSize / 8 * Memory.Byte);
				sha256.TryComputeHash(fileBlock.Content, hashCodeBlock.Content, out _);
				context.BlockHashOutputMap.Add(hashCodeBlock.Index, hashCodeBlock);
			}
			finally
			{
				fileBlock.Dispose();
			}
		}

		context.CompleteBlockHashQueueEvent.Signal();
	}

	/// <summary>
	/// Hash codes generation context.
	/// </summary>
	private readonly struct CalculationContext
	{
		public CalculationContext(GenParameters genParameters)
		{
			GenParameters = genParameters;

			// 1GB - limit for intermediate queue.
			var fileQueueSize = (uint)(Memory.Gigabyte / genParameters.BlockSize);
			FileBlockInputQueue = new BoundedBlockingQueue<IndexedSegment>(fileQueueSize);

			// Set initial size for output map based on number of workers.
			var outputMapInitialSize = 8u * genParameters.HashWorkersCount;
			BlockHashOutputMap = new BlockingMap<uint, IndexedSegment>(outputMapInitialSize);

			CompleteBlockHashQueueEvent = new CountdownEvent(genParameters.HashWorkersCount);
		}

		/// <summary>
		/// Input parameters for file signature generation algorithm.
		/// </summary>
		public GenParameters GenParameters { get; }

		/// <summary>
		/// Input file blocks queue which is consumed in parallel by hash calculation workers.
		/// </summary>
		public IQueue<IndexedSegment> FileBlockInputQueue { get; }

		/// <summary>
		/// Output hash codes map in which hash workers add calculated results.
		/// </summary>
		public IBlockingMap<uint, IndexedSegment> BlockHashOutputMap { get; }

		/// <summary>
		/// Event which represents completion of multithreaded hash calculation process.
		/// </summary>
		public CountdownEvent CompleteBlockHashQueueEvent { get; }
	}
}