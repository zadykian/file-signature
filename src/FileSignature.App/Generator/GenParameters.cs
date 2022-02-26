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
/// <param name="HashWorkersCount">
/// Number of workers to perform hashcode calculations.
/// </param>
internal readonly record struct GenParameters(string FilePath, Memory BlockSize, byte HashWorkersCount);