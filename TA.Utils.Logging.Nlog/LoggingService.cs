// This file is part of the TA.Utils project
// Copyright © 2016-2023 Timtek Systems Limited, all rights reserved.
// File: LoggingService.cs  Last modified: 2023-08-13@23:30 by Tim Long

using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using NLog;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Logging.NLog
    {
    /// <summary>
    ///     A logging service that uses NLog as the back-end logger. Implements the
    ///     <see cref="TA.Utils.Core.Diagnostics.ILog" />
    /// </summary>
    /// <seealso cref="TA.Utils.Core.Diagnostics.ILog" />
    public sealed class LoggingService : ILog
        {
        private static readonly ILogger DefaultLogger = LogManager.GetCurrentClassLogger();
        private readonly IDictionary<string, object> ambientProperties = new Dictionary<string, object>();
        private string sourceName = string.Empty;

        /// <summary>Static initializer can be used to perform 1-time NLog configurations.</summary>
        static LoggingService()
            {
            LogManager.AutoShutdown = true;
            }

        /// <inheritdoc />
        public IFluentLogBuilder Trace([CallerFilePath] string callerFilePath = null)
            {
            return CreateLogBuilder(LogLevel.Trace, callerFilePath);
            }

        /// <inheritdoc />
        public IFluentLogBuilder Debug([CallerFilePath] string callerFilePath = null)
            {
            return CreateLogBuilder(LogLevel.Debug, callerFilePath);
            }

        /// <inheritdoc />
        public IFluentLogBuilder Info([CallerFilePath] string callerFilePath = null)
            {
            return CreateLogBuilder(LogLevel.Info, callerFilePath);
            }

        /// <inheritdoc />
        public IFluentLogBuilder Warn([CallerFilePath] string callerFilePath = null)
            {
            return CreateLogBuilder(LogLevel.Warn, callerFilePath);
            }

        /// <inheritdoc />
        public IFluentLogBuilder Error([CallerFilePath] string callerFilePath = null)
            {
            return CreateLogBuilder(LogLevel.Error, callerFilePath);
            }

        /// <inheritdoc />
        public IFluentLogBuilder Fatal([CallerFilePath] string callerFilePath = null)
            {
            return CreateLogBuilder(LogLevel.Fatal, callerFilePath);
            }

        /// <inheritdoc />
        public void Shutdown()
            {
            LogManager.Shutdown();
            }

        /// <inheritdoc />
        public ILog WithAmbientProperty(string name, object value)
            {
            ambientProperties[name] = value;
            return this;
            }

        /// <inheritdoc />
        public ILog WithName(string logSourceName)
            {
            sourceName = logSourceName;
            return this;
            }

        private IFluentLogBuilder CreateLogBuilder(LogLevel logLevel, string callerFilePath)
            {
            var name = GetLoggerName(callerFilePath);
            var logger = string.IsNullOrWhiteSpace(name) ? DefaultLogger : LogManager.GetLogger(name);
            var builder = new LogBuilder(logger, logLevel, ambientProperties);
            return builder;
            }

        /// <summary>
        ///     Gets the name for this logger instance.
        ///     Any explicitly set user preference takes priority.
        ///     Otherwise, we try to build a logger name from the caller file path.
        ///     If all else fails, we return <see cref="string.Empty" />.
        /// </summary>
        /// <param name="callerFilePath">The name of the caller's source file, if known.</param>
        /// <returns>A string containing the suggested logger name.</returns>
        private string GetLoggerName(string callerFilePath)
            {
            if (!string.IsNullOrWhiteSpace(sourceName))
                return sourceName;
            var name = !string.IsNullOrWhiteSpace(callerFilePath)
                ? Path.GetFileNameWithoutExtension(callerFilePath)
                : string.Empty;
            return name;
            }
        }
    }