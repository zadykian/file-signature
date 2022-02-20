namespace FileSignature.App.Generator;

/// <summary>
/// Result of hash code generation - its' value and index of file's block.
/// </summary>
/// <param name="BlockIndex">
/// File block's zero-based index.
/// </param>
/// <param name="BlockHashCode">
/// Hash code value generated based on file's block.
/// </param>
internal readonly record struct FileBlockHash(uint BlockIndex, byte[] BlockHashCode);