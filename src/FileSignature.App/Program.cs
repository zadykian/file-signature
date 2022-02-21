using FileSignature.App.Generator;
using FileSignature.App.Queues;
using FileSignature.App.Queues.Base;
using FileSignature.App.Reader;
using Microsoft.Extensions.DependencyInjection;

namespace FileSignature.App;

/// <summary>
/// Application entry point. 
/// </summary>
internal static class Program
{
	/// <summary>
	/// Entry point method. 
	/// </summary>
	private static void Main(string[] args)
		=> ConsoleApp
			.CreateBuilder(args)
			.ConfigureServices(RegisterServices)
			.Build()
			.AddCommands<AppCommands>()
			.Run();

	/// <summary>
	/// Register application services.
	/// </summary>
	private static void RegisterServices(IServiceCollection services)
		=> services
			.AddSingleton<IInputReader, InputReader>()
			.Multiple<BoundedConcurrentQueue<FileBlock>>(options
				=> options
					.As<ISink<FileBlock>>()
					.As<ISource<FileBlock>>())
			.Multiple<BoundedConcurrentPriorityQueue<FileBlockHash>>(options
				=> options
					.As<ISink<FileBlockHash>>()
					.As<ISource<FileBlockHash>>())
			.AddSingleton<ISignatureGenerator, SignatureGenerator>();

}