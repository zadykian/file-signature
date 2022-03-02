using Microsoft.Extensions.Hosting;

namespace FileSignature.Test;

/// <inheritdoc />
internal sealed class TokenAppLifetime : IHostApplicationLifetime
{
	/// <summary>
	/// <see cref="CancellationTokenSource"/> which represents application total cancellation.
	/// </summary>
	public CancellationTokenSource TokenSource { get; } = new();

	/// <inheritdoc />
	void IHostApplicationLifetime.StopApplication() => TokenSource.Cancel();

	/// <inheritdoc />
	CancellationToken IHostApplicationLifetime.ApplicationStarted { get; } = CancellationToken.None;

	/// <inheritdoc />
	CancellationToken IHostApplicationLifetime.ApplicationStopping { get; } = CancellationToken.None;

	/// <inheritdoc />
	CancellationToken IHostApplicationLifetime.ApplicationStopped { get; } = CancellationToken.None;
}