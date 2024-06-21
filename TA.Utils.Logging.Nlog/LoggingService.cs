// This file is part of the TA.Utils project
// Copyright © 2015-2023 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: LoggingService.cs  Last modified: 2023-09-01@11:46 by Tim Long

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly LogServiceOptions options;
        private string sourceName;

        /// <summary>
        ///     Static constructor ensures that the logging back-end is loaded and configured,
        ///     and will shut down cleanly on application domain unload.
        /// </summary>
        static LoggingService()
        {
            LogManager.AutoShutdown = true;
        }

        /// <summary>Static initializer can be used to perform 1-time NLog configurations.</summary>
        public LoggingService(LogServiceOptions options = null)
        {
            this.options = options ?? LogServiceOptions.DefaultOptions;
            try
            {
                // Reflect on the calling type and use its type name as the log source name.
                // This can be overridden later using the .WithName() method.
                var callerStackFrame = new StackFrame(1);
                var callerMethod = callerStackFrame.GetMethod();
                var callerType = callerMethod.ReflectedType?.DeclaringType;
                var callerTypeName = callerType?.Name ?? string.Empty;
                sourceName = callerTypeName;
            }
            catch (Exception)
            {
                sourceName = "unknown";
            }
        }

        /// <inheritdoc />
        public IFluentLogBuilder Trace(int verbosity = 0, string sourceNameOverride = null)
        {
            return CreateLogBuilder(LogLevel.Trace, verbosity)
                .Property(nameof(verbosity), verbosity);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Debug(int verbosity = 0, string sourceNameOverride = null)
        {
            return CreateLogBuilder(LogLevel.Debug, verbosity)
                .Property(nameof(verbosity), verbosity);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Info(int verbosity = 0, string sourceNameOverride = null)
        {
            return CreateLogBuilder(LogLevel.Info, verbosity)
                .Property(nameof(verbosity), verbosity);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Warn(int verbosity = 0, string sourceNameOverride = null)
        {
            return CreateLogBuilder(LogLevel.Warn, verbosity)
                .Property(nameof(verbosity), verbosity);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Error(int verbosity = 0, string sourceNameOverride = null)
        {
            return CreateLogBuilder(LogLevel.Error, verbosity)
                .Property(nameof(verbosity), verbosity);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Fatal(int verbosity = 0, string sourceNameOverride = null)
        {
            return CreateLogBuilder(LogLevel.Fatal, verbosity)
                .Property(nameof(verbosity), verbosity);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Level(string levelName, int verbosity = 0, string sourceNameOverride = null) =>
            CreateLogBuilder(levelName, verbosity);

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

        private IFluentLogBuilder CreateLogBuilder(LogLevel logLevel, int verbosity)
        {
            var logger = string.IsNullOrWhiteSpace(sourceName) ? DefaultLogger : LogManager.GetLogger(sourceName);
            var builder = new LogBuilder(logger, logLevel, ambientProperties);
            if (options.VerbosityEnabled)
                builder.Property(options.VerbosityPropertyName, verbosity);
            return builder;
        }

        private IFluentLogBuilder CreateLogBuilder(string logLevel, int verbosity)
        {
            var logger = string.IsNullOrWhiteSpace(sourceName) ? DefaultLogger : LogManager.GetLogger(sourceName);
            var builder = new LogBuilder(logger, logLevel, ambientProperties);
            if (options.VerbosityEnabled)
                builder.Property(options.VerbosityPropertyName, verbosity);
            return builder;
        }
    }
}