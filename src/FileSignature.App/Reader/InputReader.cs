using System.Buffers;
using Microsoft.Extensions.Logging;

namespace FileSignature.App.Reader;

/// <inheritdoc />
internal class InputReader : IInputReader
{
	private readonly ILogger<InputReader> logger;

	public InputReader(ILogger<InputReader> logger) => this.logger = logger;

	/// <inheritdoc />
	IEnumerable<FileBlock> IInputReader.Read(GenParameters genParameters)
	{
		using var inputStream = CreateInputStream(genParameters);
		var consumed = false;
		uint currentIndex = 0;

		while (true)
		{
			var buffer = ArrayPool<byte>.Shared.Rent((int)genParameters.BlockSize.TotalBytes);
			var bytesReadCount = inputStream.Read(buffer);

			if (!ShouldContinue(genParameters, bytesReadCount, ref consumed))
			{
				ArrayPool<byte>.Shared.Return(buffer);
				logger.LogInformation("End of file is reached.");
				yield break;
			}

			var content = new ArraySegment<byte>(buffer, 0, bytesReadCount);
			yield return new FileBlock(currentIndex, content);
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
	private static bool ShouldContinue(GenParameters genParameters, int bytesReadCount, ref bool consumed)
	{
		if (bytesReadCount == 0 && !consumed)
		{
			throw new InvalidOperationException($"File '{genParameters.FilePath}' is empty.");
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
			throw new FileNotFoundException($"File {filePath} does not exist.");
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