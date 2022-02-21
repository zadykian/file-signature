using System.Diagnostics.CodeAnalysis;

namespace FileSignature.App.Queues.Base;

/// <summary>
/// Source of items of type <typeparamref name="T"/>.
/// </summary>
internal interface ISource<T>
{
	/// <summary>
	/// Try pull element from source.
	/// </summary>
	/// <param name="item">
	/// Pulled item.
	/// </param>
	/// <returns>
	/// <c>true</c> if item is pulled, otherwise - <c>false</c>. 
	/// </returns>
	bool TryPull([NotNullWhen(returnValue: true)] out T? item);
}