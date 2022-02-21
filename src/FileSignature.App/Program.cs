using FileSignature.App.Generator;
using FileSignature.App.Queues;
using FileSignature.App.Reader;
using FileSignature.App.Worker;
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
			.AddSingleton<IBackgroundWorker, ThreadsBackgroundWorker>()
			.AddSingleton<IQueue<IndexedSegment>, BoundedConcurrentQueue<IndexedSegment>>()
			.AddSingleton<IPriorityQueue<IndexedSegment>, BoundedConcurrentPriorityQueue<IndexedSegment>>()
			.AddSingleton<ISignatureGenerator, SignatureGenerator>();
}