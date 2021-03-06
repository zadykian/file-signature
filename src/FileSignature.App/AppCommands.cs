using System.ComponentModel.DataAnnotations;
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
	/// <summary>
	/// Default number of workers to perform hashcode calculations.
	/// </summary>
	private static readonly byte defaultWorkersCount = (byte)Environment.ProcessorCount;

	private readonly ISignatureGenerator signatureGenerator;

	public AppCommands(ISignatureGenerator signatureGenerator) => this.signatureGenerator = signatureGenerator;

	/// <summary>
	/// Generate signature of file <paramref name="filePath"/>
	/// using <paramref name="blockSize"/> as size of single block. 
	/// </summary>
	[Command(commandName: "generate", description: "Generate signature of file using specified block size.")]
	public void GenerateSignature(
		[Option(shortName: "f", "Path to file.")]
		string filePath,
		[Option(shortName: "b", "Size of single block [4KB .. 64MB].")]
		string blockSize = "1MB",
		[Option(shortName: "w", "Number of hash workers [1 .. 32].", DefaultValue = "Number of processors")]
		string? workersCount = null)
	{
		using var context = new GenerationContext(
			ParseInput(filePath, blockSize, workersCount),
			Context.CancellationToken);

		signatureGenerator
			.Generate(context)
			.ForEach(block =>
			{
				Console.WriteLine(block);
				block.Dispose();
			});
	}

	/// <summary>
	/// Parse and validate CLI parameters.
	/// </summary>
	/// <param name="filePath">
	/// Path to file.
	/// </param>
	/// <param name="blockSize">
	/// Size of single block to generate hash code of.
	/// </param>
	/// <param name="workersCount">
	/// Number of workers to perform hashcode calculations.
	/// </param>
	/// <returns>
	/// Input data for file signature generation algorithm.
	/// </returns>
	/// <exception cref="ValidationException">
	/// One of arguments has invalid value.
	/// </exception>
	private static GenParameters ParseInput(string filePath, string blockSize, string? workersCount)
	{
		var validators = new (Func<bool> Validator, string ErrorMessage)[]
		{
			(() => !string.IsNullOrWhiteSpace(filePath),
				"File path value cannot be empty."),
			(() => Memory.TryParse(blockSize, out var b) && b.Value.Between(4 * Memory.Kilobyte, 64 * Memory.Megabyte),
				"Block size must be a valid memory value belonging to range [4KB .. 64MB]."),
			(() => workersCount is null || int.TryParse(workersCount, out var w) && w.Between(1, 32),
				"Workers count must be an integer value belonging to range [1 .. 32].")
		};

		validators.RunAll().ThrowIfInvalid();
		return new(filePath, Memory.Parse(blockSize), workersCount?.To(byte.Parse) ?? defaultWorkersCount);
	}
}

/// <summary>
/// Command filter which measures elapsed time.
/// </summary>
internal class MeasureElapsedTimeFilter : ConsoleAppFilter
{
	/// <inheritdoc />
	public override async ValueTask Invoke(ConsoleAppContext context, Func<ConsoleAppContext, ValueTask> next)
	{
		var stopwatch = Stopwatch.StartNew();
		await next(context);
		context.Logger.LogInformation(@"Elapsed time: {Elapsed:hh\:mm\:ss\.fff}", stopwatch.Elapsed);
	}
}

/// <summary>
/// Command filter which handles <see cref="ValidationException"/> and logs validation error messages.
/// </summary>
internal class HandleValidationExceptionFilter : ConsoleAppFilter
{
	/// <inheritdoc />
	public override async ValueTask Invoke(ConsoleAppContext context, Func<ConsoleAppContext, ValueTask> next)
	{
		try
		{
			await next(context);
		}
		catch (Exception e) when (e.TryUnwrap<OperationCanceledException>(out _))
		{
		}
		catch (Exception e) when (e.TryUnwrap<ValidationException>(out var exception))
		{
			context.Logger.LogError($"Some parameter values are invalid:{Environment.NewLine}{exception.Message}");
		}
	}
}