namespace FileSignature.App.Reader;

/// <summary>
/// Representation of single block of file.
/// </summary>
/// <param name="Index">
/// File block's zero-based index.
/// </param>
/// <param name="Content">
/// File block's content.
/// </param>
internal readonly record struct FileBlock(uint Index, ArraySegment<byte> Content);