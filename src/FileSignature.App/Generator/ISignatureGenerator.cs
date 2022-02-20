using FileSignature.App.Reader;

namespace FileSignature.App.Generator;

/// <summary>
/// File signature generator.
/// </summary>
internal interface ISignatureGenerator
{
	/// <summary>
	/// Generate signature of file based on input <paramref name="fileBlocks"/>.
	/// </summary>
	/// <param name="fileBlocks">
	/// File splitted into sequential blocks.
	/// </param>
	/// <returns>
	/// Sequence of calculated hash-codes (one for each block).
	/// Block size is determined by <see crGenParametersnput.BlockSize"/> value.
	/// </returns>
	IEnumerable<FileBlockHash> Generate(IEnumerable<FileBlock> fileBlocks);
}