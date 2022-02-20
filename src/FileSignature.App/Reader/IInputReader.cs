namespace FileSignature.App.Reader;

/// <summary>
/// Input data reader.
/// </summary>
internal interface IInputReader
{
	/// <summary>
	/// Read file and transform it into sequential blocks of length <see crGenParametersnput.BlockSize"/>.
	/// </summary>
	/// <param name="genParameters">
	/// </param>
	/// <returns>
	/// File splitted into sequential blocks.
	/// </returns>
	IEnumerable<FileBlock> Read(GenParameters genParameters);
}