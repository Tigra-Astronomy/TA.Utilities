using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Specifications.Support;

/// <summary>
///     Provides access to a shared test logger instance for all specifications.
/// </summary>
public static class TestLog
{
    /// <summary>
    ///     Gets the shared logger for tests. Initialised in the assembly context.
    /// </summary>
    public static ILog Log { get; internal set; } = new DegenerateLoggerService();
}
