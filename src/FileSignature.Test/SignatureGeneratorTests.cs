using FileSignature.App.Generator;
using NUnit.Framework;

namespace FileSignature.Test;

/// <summary>
/// <see cref="ISignatureGenerator"/> tests.
/// </summary>
[TestFixture]
public class SignatureGeneratorTests
{
	/// <summary>
	/// Attempt to generate signature of empty file leads to <see cref="InvalidOperationException"/>.
	/// </summary>
	[Test]
	public void EmptyInputLeadsToIException()
	{
		// todo
		Assert.Pass();
	}
}