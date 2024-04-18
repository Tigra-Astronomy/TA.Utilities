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
    /// <summary>
    /// Provides a fluent interface for constructing log entries based on the NLog back-end logging service.
    /// The fluent API is loosely based on the API used by NLog.
    /// </summary>
    internal sealed class LogBuilder : IFluentLogBuilder
    {
        private readonly ILogger logger;

        /// <summary>
        ///     This accessor is intended for unit testing so that tests can inspect the contents of the log event.
        /// </summary>
        internal LogEventInfo PeekLogEvent { get; }

        /// <summary>
        ///     Construct a new log event builder at a given logging verbosity level, ensuring that any ambient properties are
        ///     used.
        ///     Instances of the builder object are typically created by the logging service, but can also be constructed on
        ///     demand.
        /// </summary>
        /// <param name="logger">An instance of a logging service that will eventually accept the built log entry.</param>
        /// <param name="level">The verbosity level of the log entry being built.</param>
        /// <param name="ambientProperties">Any ambient properties that should be included in the log entry.</param>
        /// <exception cref="ArgumentNullException">Thrown if there is no logging service or verbosity level specified.</exception>
        public LogBuilder(ILogger logger, LogLevel level, IDictionary<string, object> ambientProperties = null)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (level == null)
                throw new ArgumentNullException(nameof(level));
            this.logger = logger;
            PeekLogEvent = new LogEventInfo { LoggerName = logger.Name, Level = level };
            // Add ambient properties to the log event, if there are any.
            if (ambientProperties?.Any() ?? false)
                foreach (var property in ambientProperties)
                    PeekLogEvent.Properties[property.Key] = property.Value;
        }

        /// <summary>
        ///     Construct a new log event builder at a given logging verbosity level, ensuring that any ambient properties are
        ///     used. Instances of the builder object are typically created by the logging service, but can also be constructed on
        ///     demand.
        /// </summary>
        /// <param name="logger">An instance of a logging service that will eventually accept the built log entry.</param>
        /// <param name="level">
        ///     The severity level of the log entry being built, as a string. The string can take any value, but
        ///     ideally it should be short and one word with no punctuation or spaces. NLOG cannot render custom severities so the
        ///     NLog severity is set to "Info" and the custom severity tag is added as a property named "CustomLevel".
        ///     NLog.Targets.Seq, via an appropriate layout renderer configuration, is able to render this property into the Seq
        ///     level, since Seq supports any text as a severity.
        /// </param>
        /// <param name="ambientProperties">Any ambient properties that should be included in the log entry.</param>
        /// <exception cref="ArgumentNullException">Thrown if there is no logging service or verbosity level specified.</exception>
        public LogBuilder(ILogger logger, string logLevel, IDictionary<string, object> ambientProperties)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (string.IsNullOrWhiteSpace(logLevel))
                throw new ArgumentNullException(nameof(logLevel));
            this.logger = logger;
            PeekLogEvent = new LogEventInfo { LoggerName = logger.Name, Level = LogLevel.Info };
            // Add ambient properties to the log event, if there are any.
            if (ambientProperties?.Any() ?? false)
                foreach (var property in ambientProperties)
                    PeekLogEvent.Properties[property.Key] = property.Value;
            PeekLogEvent.Properties["CustomLevel"] = logLevel;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Exception(Exception exception)
        {
            PeekLogEvent.Exception = exception;
            return this;
        }

        /// <inheritdoc />
        public IFluentLogBuilder LoggerName(string loggerName)
        {
            PeekLogEvent.LoggerName = loggerName;
            return this;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Message([StructuredMessageTemplate] string message)
        {
            PeekLogEvent.Message = message;
            return this;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Message([StructuredMessageTemplate] string format, params object[] args)
        {
            PeekLogEvent.Message = format;
            PeekLogEvent.Parameters = args;
            return this;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Message(IFormatProvider provider, string format, params object[] args)
        {
            PeekLogEvent.FormatProvider = provider;
            PeekLogEvent.Message = format;
            PeekLogEvent.Parameters = args;
            return this;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Property(string name, object value)
        {
            try
            {
                var safeName = DeconflictPropertyName(name);
                PeekLogEvent.Properties.Add(safeName, value);
                return this;
            }
            catch (ArgumentException ex)
            {
                // Augment the exception with a more useful message and include the LogEventInfo object.
                var message = $"{ex.Message} name='{name}' value='{value}'";
                var aex = new ArgumentException(message, ex);
                throw aex;
            }
        }

        private string DeconflictPropertyName(string name)
        {
            const int MaximumAttempts = 10;
            var deconflictor = 0;
            var safeName = name;
            while (PeekLogEvent.Properties.ContainsKey(safeName))
            {
                ++deconflictor;
                if (deconflictor > MaximumAttempts)
                    throw new ArgumentException(
                        $"Property has already been added and was not unique after {MaximumAttempts} deconfliction attempts");
                safeName = $"{name}{deconflictor}";
            }

            return safeName;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Properties(IDictionary<string, object> properties)
        {
            var logProperties = properties.Select(p => new KeyValuePair<object, object>(p.Key, p.Value));
            foreach (var keyValuePair in logProperties) PeekLogEvent.Properties.Add(keyValuePair);
            return this;
        }

    /// <inheritdoc />
        public IFluentLogBuilder TimeStamp(DateTime timeStamp)
        {
            PeekLogEvent.TimeStamp = timeStamp;
            return this;
        }

        /// <inheritdoc />
        public IFluentLogBuilder StackTrace(StackTrace stackTrace, int userStackFrame)
        {
            PeekLogEvent.SetStackTrace(stackTrace, userStackFrame);
            return this;
        }

        /// <inheritdoc />
        public void Write([CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = default)
        {
            if (!logger.IsEnabled(PeekLogEvent.Level)) return;
            SetCallerInfo(callerMemberName, callerFilePath, callerLineNumber);
            logger.Log(PeekLogEvent);
        }

        /// <inheritdoc />
        public void WriteIf(Func<bool> condition,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (condition == null || !condition() || !logger.IsEnabled(PeekLogEvent.Level))
                return;
            SetCallerInfo(callerMemberName, callerFilePath, callerLineNumber);
            logger.Log(PeekLogEvent);
        }

        /// <inheritdoc />
        public void WriteIf(bool condition,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (condition == false || !logger.IsEnabled(PeekLogEvent.Level))
                return;
            SetCallerInfo(callerMemberName, callerFilePath, callerLineNumber);
            logger.Log(PeekLogEvent);
        }

        private void SetCallerInfo(string callerMethodName, string callerFilePath, int callerLineNumber)
        {
            if (callerMethodName != null || callerFilePath != null || callerLineNumber != 0)
                PeekLogEvent.SetCallerInfo(null, callerMethodName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        ///     Builds and returns the <see cref="LogEventInfo" /> without writing it to the log.
        /// </summary>
        internal LogEventInfo Build()
        {
            return PeekLogEvent;
        }
    }
}