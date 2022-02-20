namespace FileSignature.App;

/// <summary>
/// Input data for file signature generation algorithm.
/// </summary>
/// <param name="FilePath">
/// Path to file.
/// </param>
/// <param name="BlockSize">
/// Size of single block to generate hash code of.
/// </param>
public readonly record struct GenSignatureInput(string FilePath, Memory BlockSize);