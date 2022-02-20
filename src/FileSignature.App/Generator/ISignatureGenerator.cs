namespace FileSignature.App.Generator;

/// <summary>
/// File signature generator.
/// </summary>
internal interface ISignatureGenerator
{
	/// <summary>
	/// Generate signature of file based on input <paramref name="genSignatureInput"/>.
	/// </summary>
	/// <param name="genSignatureInput">
	/// Input data for file signature generation algorithm.
	/// </param>
	/// <returns>
	/// Sequence of calculated hash-codes (one for each block).
	/// Block size is determined by <see cref="GenSignatureInput.BlockSize"/> value.
	/// </returns>
	IEnumerable<FileBlockHash> Generate(GenSignatureInput genSignatureInput);
}