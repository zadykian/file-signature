using System.Buffers;
using FileSignature.App.Generator;
using Microsoft.Extensions.Logging;

namespace FileSignature.App.Reader;

/// <inheritdoc />
internal class InputReader : IInputReader
{
	private readonly ILogger<InputReader> logger;

	public InputReader(ILogger<InputReader> logger) => this.logger = logger;

	/// <inheritdoc />
	IEnumerable<FileBlock> IInputReader.Read(GenParameters genParameters, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		using var inputStream = CreateInputStream(genParameters);
		var consumed = false;
		uint currentIndex = 0;

		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var bytesCount = (int)genParameters.BlockSize.TotalBytes;
			var buffer = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(bytesCount), offset: 0, count: bytesCount);

			var bytesReadCount = inputStream.Read(buffer);

			if (!ShouldContinue(genParameters, bytesReadCount, ref consumed))
			{
				ArrayPool<byte>.Shared.Return(buffer.Array!);
				logger.LogInformation("End of file is reached.");
				yield break;
			}

			yield return new FileBlock(currentIndex, buffer[..bytesReadCount]);
			currentIndex++;
		}
	}

	/// <summary>
	/// Check if file was read to its' end.
	/// </summary>
	/// <returns>
	/// <c>true</c> if there are more bytes to read, otherwise - <c>false</c>.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// Raised in case when file <see cref="GenParameters.FilePath"/> is empty.
	/// </exception>
	private bool ShouldContinue(GenParameters genParameters, int bytesReadCount, ref bool consumed)
	{
		if (bytesReadCount == 0 && !consumed)
		{
			logger.LogWarning("File '{FilePath}' is empty.", genParameters.FilePath);
		}

		consumed = true;
		return bytesReadCount != 0;
	}

	/// <summary>
	/// Create stream to consume file's content.
	/// </summary>
	/// <param name="genParameters">
	/// Input parameters for file signature generation algorithm.
	/// </param>
	/// <exception cref="FileNotFoundException">
	/// Raised in case when file <see cref="GenParameters.FilePath"/> does not exist.
	/// </exception>
	private Stream CreateInputStream(GenParameters genParameters)
	{
		var (filePath, blockSize) = genParameters;

		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"File '{filePath}' does not exist.");
		}

		try
		{
			return new FileStream(filePath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				bufferSize: (int)blockSize.TotalBytes);
		}
		catch (UnauthorizedAccessException)
		{
			logger.LogError("Read permission are required for file '{filePath}'.", filePath);
			throw;
		}
	}
}