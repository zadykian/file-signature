using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace FileSignature.Test;

/// <summary>
/// Base class for unit tests
/// </summary>
[TestFixture]
public abstract class TestBase
{
	/// <summary>
	/// Create empty logger for service <typeparamref name="T"/>.
	/// </summary>
	private protected static ILogger<T> Logger<T>() => new Logger<T>(new NullLoggerFactory());
}