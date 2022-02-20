using System.Buffers;
using System.Security.Cryptography;

namespace FileSignature.App.Generator;

/// <inheritdoc />
internal class SignatureGenerator : ISignatureGenerator
{
	/// <inheritdoc />
	IEnumerable<FileBlockHash> ISignatureGenerator.Generate(GenSignatureInput genSignatureInput)
	{
		var inputBuffer = ArrayPool<byte>.Shared.Rent((int)genSignatureInput.BlockSize.TotalBytes);
		var outputBuffer = ArrayPool<byte>.Shared.Rent(32);

		using var sha256 = SHA256.Create();
		sha256.TryComputeHash(inputBuffer, outputBuffer, out _);

		// todo
		throw new NotImplementedException();
	}
}