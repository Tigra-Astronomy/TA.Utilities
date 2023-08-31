// This file is part of the TA.LoggingService project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: NLogLogBuilder.cs  Last modified: 2020-07-14@03:27 by Tim Long

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NLog;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Logging.NLog
{
    internal sealed class LogBuilder : IFluentLogBuilder
    {
        private readonly LogEventInfo logEvent;
        private readonly ILogger logger;

        public LogBuilder(ILogger logger, LogLevel level, IDictionary<string, object> ambientProperties = null)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (level == null)
                throw new ArgumentNullException(nameof(level));
            this.logger = logger;
            logEvent = new LogEventInfo { LoggerName = logger.Name, Level = level };
            // Add ambient properties to the log event, if there are any.
            if (ambientProperties?.Any() ?? false)
                foreach (var property in ambientProperties)
                    logEvent.Properties[property.Key] = property.Value;
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
        public IFluentLogBuilder Message([StructuredMessageTemplate] string message)
        {
            logEvent.Message = message;
            return this;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Message([StructuredMessageTemplate] string format, params object[] args)
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
        public IFluentLogBuilder Property(string name, object value)
            {
            try
                {
                logEvent.Properties.Add(name, value);
                return this;
                }
            catch (ArgumentException ex)
                {
                // Augment the exception with a more useful message and include the LogEventInfo object.
                var message = $"{ex.Message} name='{name}' value='{value.ToString()}'";
                var aex = new ArgumentException(message, ex);
                throw aex;
                }
            }

        /// <inheritdoc />
        public IFluentLogBuilder Properties(IDictionary<string, object> properties)
        {
            var logProperties = properties.Select(p => new KeyValuePair<object, object>(p.Key, p.Value));
            foreach (var keyValuePair in logProperties)
            {
                logEvent.Properties.Add(keyValuePair);
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

        /// <summary>
        /// Builds and returns the <see cref="LogEventInfo"/> without writing it to the log.
        /// </summary>
        internal LogEventInfo Build() => logEvent;
    }
}