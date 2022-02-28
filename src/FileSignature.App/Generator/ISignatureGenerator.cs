using FileSignature.App.Reader;

namespace FileSignature.App.Generator;

/// <summary>
/// File signature generator.
/// </summary>
internal interface ISignatureGenerator
{
	/// <summary>
	/// Generate signature of file based on input <see cref="GenerationContext.GenParameters"/>.
	/// </summary>
	/// <param name="context">
	/// Hash codes generation context.
	/// </param>
	/// <returns>
	/// Sequence of calculated hash-codes (one for each block).
	/// </returns>
	IEnumerable<IndexedSegment> Generate(GenerationContext context);
}