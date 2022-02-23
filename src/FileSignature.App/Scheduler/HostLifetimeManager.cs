using Microsoft.Extensions.Hosting;

namespace FileSignature.App.Scheduler;

/// <inheritdoc />
internal class HostLifetimeManager : ILifetimeManager
{
	private readonly IHostApplicationLifetime applicationLifetime;

	public HostLifetimeManager(IHostApplicationLifetime applicationLifetime)
		=> this.applicationLifetime = applicationLifetime;

	/// <inheritdoc />
	void ILifetimeManager.RequestAppCancellation() => applicationLifetime.StopApplication();
}