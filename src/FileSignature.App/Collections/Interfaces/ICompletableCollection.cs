namespace FileSignature.App.Collections.Interfaces;

/// <summary>
/// Completable collection.
/// </summary>
internal interface ICompletableCollection
{
	/// <summary>
	/// Set that collection won't be filled with new items again.
	/// </summary>
	void Complete();
}