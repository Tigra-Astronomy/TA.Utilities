// This file is part of the TA.LoggingService project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: NLogLogBuilder.cs  Last modified: 2020-07-14@03:27 by Tim Long

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ascom.Utilities.Logging.LoggingService;
using NLog;

namespace TA.Utils.Logging.NLog
    {
    internal sealed class NlogLogBuilder : IFluentLogBuilder
        {
        private readonly LogEventInfo logEvent;
        private readonly ILogger logger;

        internal NLogLogBuilder(ILogger logger, LogLevel level)
            {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (level == null)
                throw new ArgumentNullException(nameof(level));
            this.logger = logger;
            logEvent = new LogEventInfo {LoggerName = logger.Name, Level = level};
            }

        /// <inheritdoc />
        public IFluentLogBuilder Exception(Exception exception)
            {
            logEvent.Exception = exception;
            return this;
            }

        /// <inheritdoc />
        public IFluentLogBuilder LoggerName(string loggerName)
            {
            logEvent.LoggerName = loggerName;
            return this;
            }

        /// <inheritdoc />
        public IFluentLogBuilder Message(string message)
            {
            logEvent.Message = message;
            return this;
            }

        /// <inheritdoc />
        public IFluentLogBuilder Message(string format, params object[] args)
            {
            logEvent.Message = format;
            logEvent.Parameters = args;
            return this;
            }

        /// <inheritdoc />
        public IFluentLogBuilder Message(IFormatProvider provider, string format, params object[] args)
            {
            logEvent.FormatProvider = provider;
            logEvent.Message = format;
            logEvent.Parameters = args;
            return this;
            }

        /// <inheritdoc />
        public IFluentLogBuilder Property(object name, object value)
            {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            logEvent.Properties[name] = value;
            return this;
            }

        /// <inheritdoc />
        public IFluentLogBuilder Properties(IDictionary properties)
            {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            foreach (var key in properties.Keys)
                {
                logEvent.Properties[key] = properties[key];
                }
            return this;
            }

        /// <inheritdoc />
        public IFluentLogBuilder TimeStamp(DateTime timeStamp)
            {
            logEvent.TimeStamp = timeStamp;
            return this;
            }

        /// <inheritdoc />
        public IFluentLogBuilder StackTrace(StackTrace stackTrace, int userStackFrame)
            {
            logEvent.SetStackTrace(stackTrace, userStackFrame);
            return this;
            }

        /// <inheritdoc />
        public void Write([CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = default)
            {
            if (!logger.IsEnabled(logEvent.Level)) return;
            SetCallerInfo(callerMemberName, callerFilePath, callerLineNumber);
            logger.Log(logEvent);
            }

        /// <inheritdoc />
        public void WriteIf(Func<bool> condition,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
            {
            if (condition == null || !condition() || !logger.IsEnabled(logEvent.Level))
                return;
            SetCallerInfo(callerMemberName, callerFilePath, callerLineNumber);
            logger.Log(logEvent);
            }

        /// <inheritdoc />
        public void WriteIf(bool condition,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
            {
            if (condition == false || !logger.IsEnabled(logEvent.Level))
                return;
            SetCallerInfo(callerMemberName, callerFilePath, callerLineNumber);
            logger.Log(logEvent);
            }

        private void SetCallerInfo(string callerMethodName, string callerFilePath, int callerLineNumber)
            {
            if (callerMethodName != null || callerFilePath != null || callerLineNumber != 0)
                logEvent.SetCallerInfo(null, callerMethodName, callerFilePath, callerLineNumber);
            }
        }
    }