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
			.CreateBuilder(args, options => options.GlobalFilters = new ConsoleAppFilter[]
			{
				new HandleValidationExceptionFilter { Order = 0 },
				new MeasureElapsedTimeFilter        { Order = 1 }
			})
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