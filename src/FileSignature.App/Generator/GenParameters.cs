namespace FileSignature.App.Generator;

/// <summary>
/// Input parameters for file signature generation algorithm.
/// </summary>
/// <param name="FilePath">
/// Path to file.
/// </param>
/// <param name="BlockSize">
/// Size of single block to generate hash code of.
/// </param>
internal readonly record struct GenParameters(string FilePath, Memory BlockSize);