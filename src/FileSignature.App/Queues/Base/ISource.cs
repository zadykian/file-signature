namespace FileSignature.App.Queues.Base;

/// <summary>
/// Source of items of type <typeparamref name="T"/>.
/// </summary>
public interface ISource<out T>
{
	/// <summary>
	/// Determine if will there be more elements to pull. 
	/// </summary>
	bool ShouldPullNext();

	/// <summary>
	/// Pull element from source.
	/// </summary>
	T Pull();
}