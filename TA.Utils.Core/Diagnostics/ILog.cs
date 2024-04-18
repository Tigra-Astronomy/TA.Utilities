// This file is part of the TA.Utils project
// Copyright © 2015-2023 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: ILog.cs  Last modified: 2023-09-01@11:13 by Tim Long

namespace TA.Utils.Core.Diagnostics
{
    /// <summary>
    ///     Abstract logging service interface.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        ///     Creates a log builder for a log entry with Trace severity.
        /// </summary>
        /// <param name="verbosity"></param>
        /// <param name="sourceNameOverride"></param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Trace(int verbosity = 0, string sourceNameOverride = null);

        /// <summary>
        ///     Creates a log builder for a log entry with Debug severity.
        /// </summary>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Debug(int verbosity = 0, string sourceNameOverride = null);

        /// <summary>
        ///     Creates a log builder for a log entry with Information severity.
        /// </summary>
        /// <param name="verbosity">Optional; specify the log verbosity level (default 0).</param>
        /// <param name="sourceNameOverride">Optional; override the log source name.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Info(int verbosity = 0, string sourceNameOverride = null);

        /// <summary>
        ///     Creates a log builder for a log entry with Warning severity.
        /// </summary>
        /// <param name="verbosity">Optional; specify the log verbosity level (default 0).</param>
        /// <param name="sourceNameOverride">Optional; override the log source name.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Warn(int verbosity = 0, string sourceNameOverride = null);

        /// <summary>
        ///     Creates a log builder for a log entry with Error severity.
        /// </summary>
        /// <param name="verbosity">Optional; specify the log verbosity level (default 0).</param>
        /// <param name="sourceNameOverride">Optional; override the log source name.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Error(int verbosity = 0, string sourceNameOverride = null);

        /// <summary>
        ///     Creates a log builder for a log entry with Fatal severity.
        ///     Writing a Fatal log entry also terminates the process.
        /// </summary>
        /// <param name="verbosity">Optional; specify the log verbosity level (default 0).</param>
        /// <param name="sourceNameOverride">Optional; override the log source name.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Fatal(int verbosity = 0, string sourceNameOverride = null);

        /// <summary>
        ///     Specify a custom log level for a log entry. The results are provider-specific and if the logging provider is unable
        ///     to render the custom level, then it will use "Info".
        /// </summary>
        /// <param name="levelName">The name of a custom logging level.</param>
        /// <param name="verbosity">Optional; specify the log verbosity level (default 0).</param>
        /// <param name="sourceNameOverride">Optional; override the log source name.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Level(string levelName, int verbosity = 0, string sourceNameOverride = null);

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
        ///     Sets the name of the logger, which can be used by a logging back-end for filtering and routing
        ///     of log entries. If unset, then the value is implementation dependent and may default to the name
        ///     of the class or source file where the ILog instance is created or injected, or some other value.
        /// </summary>
        /// <param name="logSourceName"></param>
        /// <returns></returns>
        ILog WithName(string logSourceName);
    }
}