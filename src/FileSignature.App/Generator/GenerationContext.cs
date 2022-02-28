using System.Buffers;
using FileSignature.App.Collections;
using FileSignature.App.Collections.Interfaces;
using FileSignature.App.Reader;

namespace FileSignature.App.Generator;

/// <summary>
/// Hash codes generation context.
/// </summary>
internal readonly struct GenerationContext : IDisposable
{
	/// <summary>
	/// Calculate queue size limit based on size of single block <paramref name="blockSize"/>.
	/// </summary>
	/// <returns>
	/// Max allowable number of elements in queue <see cref="FileBlockInputQueue"/>.
	/// </returns>
	private static uint GetInputQueueSize(Memory blockSize)
	{
		const uint minSize = 4u;
		var basedOnBlockSize = (uint)(32 * Memory.Megabyte / blockSize);
		return Math.Max(minSize, basedOnBlockSize);
	}

	public GenerationContext(GenParameters genParameters, CancellationToken cancellationToken = default)
	{
		GenParameters = genParameters;
		CancellationToken = cancellationToken;

		// Get queue size limit based on single block size.
		var fileQueueSize = GetInputQueueSize(genParameters.BlockSize);
		FileBlockInputQueue = new BoundedBlockingQueue<IndexedSegment>(fileQueueSize, cancellationToken);

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
	/// Token representing generation cancellation.
	/// </summary>
	public CancellationToken CancellationToken { get; }

	/// <summary>
	/// Input file blocks queue which is consumed in parallel by hash calculation workers.
	/// </summary>
	public IQueue<IndexedSegment> FileBlockInputQueue { get; }

	/// <summary>
	/// Output hash codes map in which hash workers add generated results.
	/// </summary>
	/// <remarks>
	/// Keys:   Blocks' indices <see cref="IndexedSegment.Index"/>.
	/// Values: Generated hash codes.
	/// </remarks>
	public IBlockingMap<uint, IndexedSegment> BlockHashOutputMap { get; }

	/// <summary>
	/// Event which represents completion of multithreaded hash calculation process.
	/// </summary>
	public CountdownEvent CompleteBlockHashQueueEvent { get; }

	/// <inheritdoc />
	void IDisposable.Dispose() => CompleteBlockHashQueueEvent.Dispose();
}