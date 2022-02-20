using System.Buffers;
using System.Collections.Immutable;
using System.Security.Cryptography;
using FileSignature.App.Reader;

namespace FileSignature.App.Generator;

/// <inheritdoc />
internal class SignatureGenerator : ISignatureGenerator
{
	/// <inheritdoc />
	IEnumerable<FileBlockHash> ISignatureGenerator.Generate(IEnumerable<FileBlock> fileBlocks)
	{
		var outputBuffer = ArrayPool<byte>.Shared.Rent(32);
		using var sha256 = SHA256.Create();
		sha256.TryComputeHash(fileBlocks.First().Content, outputBuffer, out _);


		// todo
		throw new NotImplementedException();
	}
}