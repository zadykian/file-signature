using System.Buffers;

namespace FileSignature.App.Reader;

/// <summary>
/// Indexed (zero-based) array segment.
/// </summary>
/// <param name="Index">
/// Zero-based index.
/// </param>
/// <param name="Content">
/// Content.
/// </param>
internal readonly record struct IndexedSegment(uint Index, ArraySegment<byte> Content) : IDisposable
{
	/// <summary>
	/// Rent <see cref="ArraySegment{Byte}"/> from <see cref="ArrayPool{Byte}"/>.
	/// </summary>
	private static ArraySegment<byte> Rent(Memory blockSize)
		=> new(
			array:  ArrayPool<byte>.Shared.Rent((int)blockSize.TotalBytes),
			offset: 0,
			count:  (int)blockSize.TotalBytes);

	/// <summary>
	/// Create new <see cref="IndexedSegment"/> instance.
	/// </summary>
	/// <param name="index">
	/// Zero-based index.
	/// </param>
	/// <param name="blockSize">
	/// Size of <see cref="Content"/>.
	/// </param>
	public IndexedSegment(uint index, Memory blockSize) : this(index, Rent(blockSize))
	{
	}

	/// <inheritdoc />
	public override string ToString() => $"{Index:D8}: {Convert.ToHexString(Content)}";

	/// <inheritdoc />
	/// <remarks>
	/// Return underlying array to <see cref="ArrayPool{Byte}"/>.
	/// </remarks>
	public void Dispose() => ArrayPool<byte>.Shared.Return(Content.Array!);
}