using FileSignature.App.Scheduler;

namespace FileSignature.Test;

/// <inheritdoc />
internal sealed class TokenLifetimeManager : ILifetimeManager
{
	/// <summary>
	/// <see cref="CancellationTokenSource"/> which represents application total cancellation.
	/// </summary>
	public CancellationTokenSource TokenSource { get; } = new();

	/// <inheritdoc />
	void ILifetimeManager.RequestAppCancellation() => TokenSource.Cancel();
}