namespace FileSignature.App.Scheduler;

/// <summary>
/// Application lifetime manager.
/// </summary>
internal interface ILifetimeManager
{
	/// <summary>
	/// Initiate application graceful shutdown.
	/// </summary>
	void RequestAppCancellation();
}