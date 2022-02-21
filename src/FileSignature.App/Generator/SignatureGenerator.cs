using FileSignature.App.Reader;

namespace FileSignature.App.Generator;

internal interface IBackgroundWorkers
{
	void Enqueue(Action workUnit, byte degreeOfParallelism);
}

/// <inheritdoc />
internal class SignatureGenerator : ISignatureGenerator
{
	private readonly IInputReader inputReader;

	public SignatureGenerator(IInputReader inputReader)
	{
		this.inputReader = inputReader;
	}

	/// <inheritdoc />
	IEnumerable<FileBlockHash> ISignatureGenerator.Generate(
		GenParameters genParameters,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		inputReader
			.Read(genParameters, cancellationToken);


		// todo
		throw new NotImplementedException();
	}
}