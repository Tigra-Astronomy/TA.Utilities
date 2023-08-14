using System.Runtime.CompilerServices;

namespace TA.Utils.Core.Diagnostics
    {
    /// <summary>
    ///     Logging service interface
    /// </summary>
    public interface ILog
        {
        /// <summary>
        ///     Creates a log builder for a log entry with Trace severity.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Trace([CallerFilePath] string callerFilePath = null);

        /// <summary>
        ///     Creates a log builder for a log entry with Debug severity.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Debug([CallerFilePath] string callerFilePath = null);

        /// <summary>
        ///     Creates a log builder for a log entry with Information severity.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Info([CallerFilePath] string callerFilePath = null);

        /// <summary>
        ///     Creates a log builder for a log entry with Warning severity.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Warn([CallerFilePath] string callerFilePath = null);

        /// <summary>
        ///     Creates a log builder for a log entry with Error severity.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Error([CallerFilePath] string callerFilePath = null);

        /// <summary>
        ///     Creates a log builder for a log entry with Fatal severity.
        ///     Writing a Fatal log entry also terminates the process.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Fatal([CallerFilePath] string callerFilePath = null);

        /// <summary>
        ///     Instructs the logging service to shut down.
        ///     This should flush any buffered log entries and close any open files or streams.
        ///     It is best practice to call <c>Shutdown</c> before exiting from the program.
        /// </summary>
        void Shutdown();

        /// <summary>
        ///     Sets an "ambient property" that should be included in all log events.
        ///     Once added, the property persists for the lifetim of the instance.
        ///     Useful for loggers that support semantic logging.
        /// </summary>
        ILog WithAmbientProperty(string name, object value);

        /// <summary>
        /// Sets the name of the logger, which can be used by a logging back-end for filtering and routing
        /// of log entries. If unset, then the value is implementation dependent and may default to the name
        /// of the class or source file where the ILog instance is created or injected, or some other value.
        /// </summary>
        /// <param name="logSourceName"></param>
        /// <returns></returns>
        ILog WithName(string logSourceName);
        }
    }