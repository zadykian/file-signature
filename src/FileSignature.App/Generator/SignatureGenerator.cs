using System.Security.Cryptography;
using FileSignature.App.Reader;
using FileSignature.App.Scheduler;

// ReSharper disable ArgumentsStyleStringLiteral

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
	IEnumerable<IndexedSegment> ISignatureGenerator.Generate(GenerationContext context)
	{
		context.CancellationToken.ThrowIfCancellationRequested();

		// Consume file content as sequence of blocks
		// and push each block to GenerationContext.FileBlockInputQueue.
		RunFileConsumptionProcess(context);

		// Consume data from GenerationContext.FileBlockInputQueue, generate
		// hash codes and push values to GenerationContext.BlockHashOutputMap.
		RunHashGenerationProcess(context);

		// Consume hash values from GenerationContext.BlockHashOutputMap
		// in foreground.
		var blockIndexRange = Enumerable.Range(0, int.MaxValue).Select(blockIndex => (uint)blockIndex);
		return context.BlockHashOutputMap.GetAndRemoveAllByKeys(blockIndexRange, context.CancellationToken);
	}

	/// <summary>
	/// Run file consumption process in background.
	/// </summary>
	/// <param name="context">
	/// Hash codes generation context.
	/// </param>
	private void RunFileConsumptionProcess(GenerationContext context)
		=> workScheduler.RunInBackground(() =>
		{
			inputReader
				.Read(context.GenParameters, context.CancellationToken)
				.ForEach(context.FileBlockInputQueue.Push);

			context.FileBlockInputQueue.Complete();
		}, workerName: "File reader");

	/// <summary>
	/// Run hash generation process in background.
	/// </summary>
	/// <param name="context">
	/// Hash codes generation context.
	/// </param>
	private void RunHashGenerationProcess(GenerationContext context)
	{
		// Consume data from GenerationContext.FileBlockInputQueue, calculate hash codes
		// in parallel and push results to GenerationContext.BlockHashOutputMap.

		workScheduler.RunInBackground(
			() => HashGenerationWorkItem(context),
			workerName: "Hash generator",
			degreeOfParallelism: context.GenParameters.HashWorkersCount);

		// Wait for calculation completion in background and then set
		// GenerationContext.BlockHashOutputMap as completed.

		workScheduler.RunInBackground(() =>
		{
			context.CompleteBlockHashQueueEvent.Wait(context.CancellationToken);
			context.BlockHashOutputMap.Complete();
		}, workerName: "Hash completion waiter");
	}

	/// <summary>
	/// Calculate hash codes of file blocks from queue <see cref="GenerationContext.FileBlockInputQueue"/>
	/// and push results to <see cref="GenerationContext.FileBlockInputQueue"/>.
	/// </summary>
	/// <param name="context">
	/// Hash codes generation context.
	/// </param>
	private static void HashGenerationWorkItem(GenerationContext context)
	{
		context.CancellationToken.ThrowIfCancellationRequested();
		using var sha256 = SHA256.Create();

		foreach (var fileBlock in context.FileBlockInputQueue.ConsumeAsEnumerable())
		{
			try
			{
				context.CancellationToken.ThrowIfCancellationRequested();
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
}