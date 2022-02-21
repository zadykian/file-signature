using System.Security.Cryptography;
using FileSignature.App.Queues;
using FileSignature.App.Reader;
using FileSignature.App.Worker;

namespace FileSignature.App.Generator;

/// <inheritdoc />
internal class SignatureGenerator : ISignatureGenerator
{
	private readonly IInputReader inputReader;
	private readonly IBackgroundWorker backgroundWorker;

	private readonly IQueue<IndexedSegment> fileBlockQueue;
	private readonly IPriorityQueue<IndexedSegment> blockHashQueue;

	public SignatureGenerator(
		IInputReader inputReader,
		IBackgroundWorker backgroundWorker,
		IQueue<IndexedSegment> fileBlockQueue,
		IPriorityQueue<IndexedSegment> blockHashQueue)
	{
		this.inputReader = inputReader;
		this.backgroundWorker = backgroundWorker;
		this.fileBlockQueue = fileBlockQueue;
		this.blockHashQueue = blockHashQueue;
	}

	/// <inheritdoc />
	IEnumerable<IndexedSegment> ISignatureGenerator.Generate(
		GenParameters genParameters,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		// Consume file content as sequence of blocks and push each block to fileBlockQueue.

		backgroundWorker.Enqueue(() =>
		{
			inputReader
				.Read(genParameters, cancellationToken)
				.ForEach(fileBlockQueue.Push);

			fileBlockQueue.Complete();
		});

		// Consume data from fileBlockQueue, calculate hash codes in parallel and push results to blockHashQueue.

		backgroundWorker.Enqueue(
			() => HashGenerationWorkItem(cancellationToken),
			(byte)Environment.ProcessorCount);

		// Consume hash values from queue on current thread.

		return blockHashQueue.ConsumeAsEnumerable(cancellationToken);
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
				var hashCodeBlock = new IndexedSegment(fileBlock.Index, Memory.Bytes(sha256.HashSize));
				sha256.TryComputeHash(fileBlock.Content, hashCodeBlock.Content, out _);
				blockHashQueue.Push(hashCodeBlock, hashCodeBlock.Index);
			}
			finally
			{
				fileBlock.Dispose();
			}
		}

		// todo: call blockHashQueue.Complete()
	}
}