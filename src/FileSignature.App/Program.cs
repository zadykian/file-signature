using FileSignature.App.Generator;
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
			.ConfigureServices(services => services
				.AddSingleton<ISignatureGenerator, SignatureGenerator>())
			.Build()
			.AddCommands<AppCommands>()
			.Run();
}