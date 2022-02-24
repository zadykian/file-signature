using System.Runtime.CompilerServices;
using FileSignature.App.Generator;
using FileSignature.App.Reader;
using FileSignature.App.Scheduler;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("FileSignature.Test")]

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
			.AddSingleton<ILifetimeManager, HostLifetimeManager>()
			.AddSingleton<IWorkScheduler, ThreadWorkScheduler>()
			.AddSingleton<ISignatureGenerator, SignatureGenerator>();
}