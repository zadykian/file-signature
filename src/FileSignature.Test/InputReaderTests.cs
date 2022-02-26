using FileSignature.App.Generator;
using FileSignature.App.Reader;
using NUnit.Framework;
using TypeDecorators.Lib.Extensions;
using TypeDecorators.Lib.Types;

namespace FileSignature.Test;

/// <summary>
/// Tests of <see cref="IInputReader"/> component.
/// </summary>
public class InputReaderTests : TestBase
{
	/// <summary>
	/// Name of temporary directory for test files.
	/// Being recreated on each test cycle.
	/// </summary>
	private const string tempDirName = "temp-test-files";

	/// <summary>
	/// Map from file size to its' full path.
	/// </summary>
	private static readonly IDictionary<Memory, string> pathBySize = new[]
		{
			Memory.Zero,
			02 * Memory.Megabyte,
			16 * Memory.Megabyte
		}
		.ToDictionary(
			keySelector: memory => memory,
			elementSelector: memory => Path.Combine(tempDirName, $"{memory}.txt"));

	/// <summary>
	/// Create file with size <paramref name="fileSize"/> in <see cref="tempDirName"/> directory.
	/// </summary>
	private static void CreateFileOfSize(Memory fileSize)
	{
		var filePath = pathBySize[fileSize];

		var content = Enumerable
			.Range(1, (int)fileSize.TotalBytes)
			.Select(index => (byte)(index % byte.MaxValue))
			.ToArray();

		File.WriteAllBytes(filePath, content);
	}

	/// <summary>
	/// Create <see cref="IInputReader"/> instance.
	/// </summary>
	private static IInputReader Reader() => new InputReader(Logger<InputReader>());

	/// <summary>
	/// Create <see cref="tempDirName"/> directory and fill it with test files.
	/// </summary>
	[OneTimeSetUp]
	public static void CreateTestFiles()
	{
		if (Directory.Exists(tempDirName)) DropTestDirectory();
		Directory.CreateDirectory(tempDirName);
		pathBySize.Keys.ForEach(CreateFileOfSize);
	}

	/// <summary>
	/// <see cref="IInputReader.Read"/> throws <see cref="FileNotFoundException"/>
	/// if specified file does not exists.
	/// </summary>
	[Test]
	public void ThrowExceptionIfNotExists()
	{
		var reader = Reader();
		var genParams = new GenParameters("non-existing-file.txt", Memory.Megabyte, 4);

		// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
		Assert.Throws<FileNotFoundException>(() => reader.Read(genParams).ToArray());
	}

	/// <summary>
	/// Empty file transformed to empty sequence of blocks.
	/// </summary>
	[Test]
	public void ReadEmptyFile()
	{
		var reader = Reader();
		var genParams = new GenParameters(pathBySize[Memory.Zero], Memory.Kilobyte, 4);

		var result = reader.Read(genParams);
		Assert.IsEmpty(result, "Resulting sequence is expected to be empty!");
	}

	/// <summary>
	/// Read file which size is less than specified block size.
	/// </summary>
	[Test]
	public void FileLessThanBlockSize()
	{
		var reader = Reader();
		var fileSize = 2 * Memory.Megabyte;

		var genParams = new GenParameters(pathBySize[fileSize], 4 * Memory.Megabyte, 4);

		var result = reader.Read(genParams).ToArray();

		Assert.AreEqual(
			expected: 1, actual: result.Length,
			"Resulting sequence is expected to contain single block!");

		Assert.AreEqual(
			expected: fileSize.TotalBytes, actual: result[0].Content.Count,
			"File's block has unexpected size!");
	}

	/// <summary>
	/// Read file which size is a multiple of specified block size.
	/// </summary>
	[Test]
	public void ReadSeveralBlocks()
	{
		var reader = Reader();
		var genParams = new GenParameters(pathBySize[16 * Memory.Megabyte], Memory.Megabyte, 4);

		var result = reader.Read(genParams).ToArray();

		Assert.AreEqual(
			expected: 16, actual: result.Length,
			"Resulting block sequence has unexpected length!");

		Assert.IsTrue(
			result.All(block => block.Content.Count == (int)genParams.BlockSize.TotalBytes),
			"Some of blocks in resulting sequence has unexpected size!");

		Assert.IsTrue(
			result
				.Select(block => (int)block.Index)
				.SequenceEqual(Enumerable.Range(0, 16)),
			"Resulting sequence has unexpected indexing!");
	}

	/// <summary>
	/// Drop <see cref="tempDirName"/> directory.
	/// </summary>
	[OneTimeTearDown]
	public static void DropTestDirectory() => Directory.Delete(tempDirName, recursive: true);
}