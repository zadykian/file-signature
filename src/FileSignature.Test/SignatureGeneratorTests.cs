using System.Security.Cryptography;
using FileSignature.App.Generator;
using FileSignature.App.Reader;
using FileSignature.App.Scheduler;
using NUnit.Framework;
using TypeDecorators.Lib.Extensions;
using TypeDecorators.Lib.Types;

#pragma warning disable CA1816

namespace FileSignature.Test;

/// <summary>
/// Tests of <see cref="ISignatureGenerator"/> component.
/// </summary>
public class SignatureGeneratorTests : TestBase, IDisposable
{
	private static readonly Random random = new();
	private readonly SHA256 sha256 = SHA256.Create();

	/// <summary>
	/// Create new <see cref="GenParameters"/> instance.
	/// </summary>
	private static GenerationContext Context() => new(
		new GenParameters("test-file-name", Memory.Megabyte, 8),
		CancellationToken.None);

	/// <summary>
	/// Create new <see cref="ISignatureGenerator"/> instance.
	/// </summary>
	private static ISignatureGenerator Generator(IInputReader inputReader)
		=> new SignatureGenerator(
			inputReader,
			new ThreadWorkScheduler(new TokenLifetimeManager(), Logger<ThreadWorkScheduler>()));

	/// <summary>
	/// Attempt to generate signature of empty file leads to <see cref="InvalidOperationException"/>.
	/// </summary>
	[Test]
	public void EmptyInputLeadsEmptyOutput()
	{
		var testInput = Enumerable.Empty<IndexedSegment>();
		var generator = Generator(new MemoryInputReader(testInput));

		using var context = Context();
		var hashCodes = generator.Generate(context);
		Assert.IsEmpty(hashCodes);
	}

	/// <summary>
	/// Calculate hash codes in parallel based on memory input data.
	/// </summary>
	[Test]
	[Timeout(5000)]
	public void CalculateHashCodes()
	{
		var testInput = TestInput();

		// We need to copy original input because SignatureGenerator disposes received segments.

		var inputCopy = testInput
			.Select(segment =>
			{
				var (index, originalContent) = segment;
				var contentCopy = new byte[originalContent.Count];
				originalContent.Array!.CopyTo(contentCopy.AsSpan());
				return new IndexedSegment(index, Content: new ArraySegment<byte>(contentCopy));
			})
			.ToArray();

		var generator = Generator(new MemoryInputReader(testInput));

		using var context = Context();
		var hashCodes = generator.Generate(context).ToArray();

		Assert.IsTrue(
			hashCodes.Select(segment => segment.Index).IsOrdered(),
			"Received hash codes sequence is not ordered by index!");

		inputCopy
			.Select(segment => sha256.ComputeHash(segment.Content.Array!))
			.Zip(hashCodes.Select(segment => segment.Content.Array!))
			.ForEach(tuple =>
			{
				var (expected, actual) = tuple;
				Assert.IsTrue(
					actual.SequenceEqual(expected),
					"Some of output blocks has unexpected hash code!");
			});
	}

	/// <inheritdoc />
	private sealed class MemoryInputReader : IInputReader
	{
		private readonly IReadOnlyCollection<IndexedSegment> testInput;

		public MemoryInputReader(IEnumerable<IndexedSegment> testInput) => this.testInput = testInput.ToArray();

		/// <inheritdoc />
		IEnumerable<IndexedSegment> IInputReader.Read(
			GenParameters genParameters,
			CancellationToken cancellationToken) => testInput;
	}

	/// <summary>
	/// Generate test input file segments.
	/// </summary>
	private static IReadOnlyCollection<IndexedSegment> TestInput()
	{
		static byte[] RandomOfSize(Memory length)
			=> Enumerable
				.Range(0, (int)length.TotalBytes).Select(_ => (byte)(random.Next() % 255))
				.ToArray();

		return Enumerable
			.Range(0, 32)
			.Select(index => new IndexedSegment((uint)index, new ArraySegment<byte>(RandomOfSize(Memory.Megabyte))))
			.Add(new IndexedSegment(32, RandomOfSize(256 * Memory.Kilobyte)))
			.ToArray();
	}

	/// <inheritdoc />
	void IDisposable.Dispose() => sha256.Dispose();
}