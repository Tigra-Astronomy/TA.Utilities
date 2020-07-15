// This file is part of the TA.LoggingService project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: NLogLoggingService.cs  Last modified: 2020-07-14@02:43 by Tim Long

using System.IO;
using Ascom.Utilities.Logging.LoggingService;
using NLog;

namespace TA.Utils.Logging.NLog
    {
    public sealed class NlogLoggingService : ILog
        {
        private static readonly ILogger DefaultLogger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public IFluentLogBuilder Trace(string callerFilePath = null)
            {
            return Create(LogLevel.Trace, callerFilePath);
            }

        /// <inheritdoc />
        public IFluentLogBuilder Debug(string callerFilePath = null)
            {
            return Create(LogLevel.Debug, callerFilePath);
            }

        /// <inheritdoc />
        public IFluentLogBuilder Info(string callerFilePath = null)
            {
            return Create(LogLevel.Info, callerFilePath);
            }

        /// <inheritdoc />
        public IFluentLogBuilder Warning(string callerFilePath = null)
            {
            return Create(LogLevel.Warn, callerFilePath);
            }

        /// <inheritdoc />
        public IFluentLogBuilder Error(string callerFilePath = null)
            {
            return Create(LogLevel.Error, callerFilePath);
            }

        /// <inheritdoc />
        public IFluentLogBuilder Fatal(string callerFilePath = null)
            {
            return Create(LogLevel.Fatal, callerFilePath);
            }

        private IFluentLogBuilder Create(LogLevel logLevel, string callerFilePath)
            {
            string name = !string.IsNullOrWhiteSpace(callerFilePath) ? Path.GetFileNameWithoutExtension(callerFilePath) : null;
            var logger = string.IsNullOrWhiteSpace(name) ? DefaultLogger : LogManager.GetLogger(name);
            var builder = new NlogLogBuilder(logger, logLevel);
            return builder;
            }
        }
    }