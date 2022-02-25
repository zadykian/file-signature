using System.Diagnostics;
using FileSignature.App.Generator;
using Microsoft.Extensions.Logging;

namespace FileSignature.App;

// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable UnusedMember.Global

/// <summary>
/// Available application's commands.
/// </summary>
internal class AppCommands : ConsoleAppBase
{
	private readonly ISignatureGenerator signatureGenerator;
	private readonly ILogger<AppCommands> logger;

	public AppCommands(ISignatureGenerator signatureGenerator, ILogger<AppCommands> logger)
	{
		this.signatureGenerator = signatureGenerator;
		this.logger = logger;
	}

	/// <summary>
	/// Generate signature of file <paramref name="filePath"/>
	/// using <paramref name="blockSize"/> as size of single block. 
	/// </summary>
	[Command(commandName: "generate", description: "Generate signature of file using specified block size.")]
	public void GenerateSignature(
		[Option(shortName: "f", description: "Path to file.")]                       string filePath,
		[Option(shortName: "b", description: "Size of single block [4kB .. 64MB].")] string blockSize = "1MB")
		=> signatureGenerator
			.Generate(ParseInput(filePath, blockSize), Context.CancellationToken)
			.ForEach(block =>
			{
				logger.LogTrace(block.ToString());
				block.Dispose();
			});

	/// <summary>
	/// Parse and validate CLI parameters (<paramref name="filePath"/>, <paramref name="blockSize"/>).
	/// </summary>
	/// <param name="filePath">
	/// Path to file.
	/// </param>
	/// <param name="blockSize">
	/// Size of single block to generate hash code of.
	/// </param>
	/// <returns>
	/// Input data for file signature generation algorithm.
	/// </returns>
	/// <exception cref="ArgumentException">
	/// Either <paramref name="filePath"/> or <paramref name="blockSize"/> has invalid format or value.
	/// </exception>
	private static GenParameters ParseInput(string filePath, string blockSize)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			throw new ArgumentException("File path value cannot be empty.", nameof(filePath));
		}

		if (!Memory.TryParse(blockSize, out var memory))
		{
			throw new ArgumentException($"Value '{blockSize}' is not a valid memory volume.", nameof(blockSize));
		}

		if (!memory.Value.InRangeBetween(4 * Memory.Kilobyte, 64 * Memory.Megabyte))
		{
			throw new ArgumentException("Block size value must belong to range [4kB .. 64MB].");
		}

		return new GenParameters(filePath, memory.Value);
	}
}

/// <summary>
/// Command filter which measures elapsed time.
/// </summary>
internal class ElapsedTimeFilter : ConsoleAppFilter
{
	private ElapsedTimeFilter()
	{
	}

	/// <summary>
	/// Instance of <see cref="ElapsedTimeFilter"/>.
	/// </summary>
	public static ConsoleAppFilter Instance { get; } = new ElapsedTimeFilter();

	/// <inheritdoc />
	public override async ValueTask Invoke(ConsoleAppContext context, Func<ConsoleAppContext, ValueTask> next)
	{
		var stopwatch = Stopwatch.StartNew();
		try
		{
			await next(context);
		}
		finally
		{
			context.Logger.LogInformation(@"Elapsed time: {Elapsed:hh\:mm\:ss\.fff}", stopwatch.Elapsed);
		}
	}
}