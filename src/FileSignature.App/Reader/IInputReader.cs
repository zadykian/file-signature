namespace FileSignature.App.Reader;

/// <summary>
/// Input data reader.
/// </summary>
internal interface IInputReader
{
	/// <summary>
	/// Read file and transform it into sequential blocks of length <see cref="GenParameters.BlockSize"/>.
	/// </summary>
	/// <param name="genParameters">
	/// Input parameters for file signature generation algorithm.
	/// </param>
	/// <returns>
	/// File splitted into sequential blocks.
	/// </returns>
	IEnumerable<FileBlock> Read(GenParameters genParameters);
}