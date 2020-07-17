// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: LoggingService.cs  Last modified: 2020-07-16@20:09 by Tim Long

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

        private IFluentLogBuilder CreateLogBuilder(LogLevel logLevel, string callerFilePath)
            {
            string name = !string.IsNullOrWhiteSpace(callerFilePath)
                ? Path.GetFileNameWithoutExtension(callerFilePath)
                : null;
            var logger = string.IsNullOrWhiteSpace(name) ? DefaultLogger : LogManager.GetLogger(name);
            var builder = new LogBuilder(logger, logLevel);
            return builder;
            }

        /// <inheritdoc />
        public void Shutdown() => LogManager.Shutdown();
        }
    }