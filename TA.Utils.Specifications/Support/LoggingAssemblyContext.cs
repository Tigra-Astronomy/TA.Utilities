using Machine.Specifications;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Specifications.Support;

namespace TA.Utils.Specifications.Support;

/// <summary>
///     MSpec assembly context initialising a single ConsoleLoggerService instance for all specs.
/// </summary>
public class LoggingAssemblyContext : IAssemblyContext
{
    private ConsoleLoggerService logger;

    public void OnAssemblyStart()
    {
        var options = ConsoleLoggerOptions
            .DefaultOptions
            .RenderProperties(false);

        logger = new ConsoleLoggerService(options);
        TestLog.Log = logger;
    }

    public void OnAssemblyComplete()
    {
        logger?.Shutdown();
    }
}
