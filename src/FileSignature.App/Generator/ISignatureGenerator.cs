namespace FileSignature.App.Generator;

/// <summary>
/// File signature generator.
/// </summary>
internal interface ISignatureGenerator
{
	/// <summary>
	/// Generate signature of file based on input <paramref name="genParameters"/>.
	/// </summary>
	/// <param name="genParameters">
	/// Input parameters for file signature generation algorithm.
	/// </param>
	/// <param name="cancellationToken">
	/// Token to cancel an operation.
	/// </param>
	/// <returns>
	/// Sequence of calculated hash-codes (one for each block).
	/// </returns>
	IEnumerable<FileBlockHash> Generate(GenParameters genParameters, CancellationToken cancellationToken);
}