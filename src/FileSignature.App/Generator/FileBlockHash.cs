namespace FileSignature.App.Generator;

/// <summary>
/// Result of hash code generation - its' value and index of file's block.
/// </summary>
/// <param name="Index">
/// File block's zero-based index.
/// </param>
/// <param name="HashCode">
/// Hash code value generated based on file's block.
/// </param>
internal readonly record struct FileBlockHash(uint Index, byte[] HashCode)
{
	/// <inheritdoc />
	public override string ToString() => $"{Index:D8}: {Convert.ToHexString(HashCode)}";
}