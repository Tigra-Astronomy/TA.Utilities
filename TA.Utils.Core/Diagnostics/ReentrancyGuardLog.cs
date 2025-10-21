// This file is part of the TA.Utils project
// Copyright © 2015-2025 Timtek Systems Limited, all rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
//
// File: ReentrancyGuardLog.cs
// Summary: ILog decorator that prevents re-entrant log writes on the current async-flow.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TA.Utils.Core.Diagnostics
{
    /// <summary>
    ///     An <see cref="ILog" /> decorator that prevents re-entrant log writes on the current async-flow.
    ///     If a nested write is attempted while another write is in progress, the nested write is dropped.
    ///     This helps avoid recursive logging cycles caused by logging during object-graph serialization
    ///     or from property getters/constructors executed while a log event is being rendered.
    /// </summary>
    public sealed class ReentrancyGuardLog : ILog
    {
        private static readonly AsyncLocal<int> Depth = new();
        private readonly ILog inner;

        /// <summary>
        ///     Creates a new re-entrancy guarded logger that wraps an existing <see cref="ILog"/>.
        /// </summary>
        public ReentrancyGuardLog(ILog inner)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        /// <summary>
        ///     Indicates whether the current async-flow is already inside a log write.
        /// </summary>
        public static bool IsActive => Depth.Value > 0;

        /// <inheritdoc />
        public IFluentLogBuilder Trace(int verbosity = 0, string? sourceNameOverride = null)
            => new GuardedLogBuilder(inner.Trace(verbosity, sourceNameOverride));

        /// <inheritdoc />
        public IFluentLogBuilder Debug(int verbosity = 0, string? sourceNameOverride = null)
            => new GuardedLogBuilder(inner.Debug(verbosity, sourceNameOverride));

        /// <inheritdoc />
        public IFluentLogBuilder Info(int verbosity = 0, string? sourceNameOverride = null)
            => new GuardedLogBuilder(inner.Info(verbosity, sourceNameOverride));

        /// <inheritdoc />
        public IFluentLogBuilder Warn(int verbosity = 0, string? sourceNameOverride = null)
            => new GuardedLogBuilder(inner.Warn(verbosity, sourceNameOverride));

        /// <inheritdoc />
        public IFluentLogBuilder Error(int verbosity = 0, string? sourceNameOverride = null)
            => new GuardedLogBuilder(inner.Error(verbosity, sourceNameOverride));

        /// <inheritdoc />
        public IFluentLogBuilder Fatal(int verbosity = 0, string? sourceNameOverride = null)
            => new GuardedLogBuilder(inner.Fatal(verbosity, sourceNameOverride));

        /// <inheritdoc />
        public IFluentLogBuilder Level(string levelName, int verbosity = 0, string? sourceNameOverride = null)
            => new GuardedLogBuilder(inner.Level(levelName, verbosity, sourceNameOverride));

        /// <inheritdoc />
        public void Shutdown() => inner.Shutdown();

        /// <inheritdoc />
        public ILog WithAmbientProperty(string name, object value)
            => new ReentrancyGuardLog(inner.WithAmbientProperty(name, value));

        /// <inheritdoc />
        public ILog WithName(string logSourceName)
            => new ReentrancyGuardLog(inner.WithName(logSourceName));

        /// <summary>
        ///     Wrapper for <see cref="IFluentLogBuilder" /> that intercepts Write/WriteIf and applies the guard.
        /// </summary>
        private sealed class GuardedLogBuilder(IFluentLogBuilder innerBuilder) : IFluentLogBuilder
        {
            private readonly IFluentLogBuilder innerBuilder = innerBuilder ?? throw new ArgumentNullException(nameof(innerBuilder));

            public IFluentLogBuilder Exception(Exception exception)
            {
                innerBuilder.Exception(exception);
                return this;
            }

            public IFluentLogBuilder LoggerName(string loggerName)
            {
                innerBuilder.LoggerName(loggerName);
                return this;
            }

            public IFluentLogBuilder Message(string message)
            {
                innerBuilder.Message(message);
                return this;
            }

            public IFluentLogBuilder Message(string format, params object[] args)
            {
                innerBuilder.Message(format, args);
                return this;
            }

            public IFluentLogBuilder Message(IFormatProvider provider, string format, params object[] args)
            {
                innerBuilder.Message(provider, format, args);
                return this;
            }

            public IFluentLogBuilder Property(string name, object value)
            {
                innerBuilder.Property(name, value);
                return this;
            }

            public IFluentLogBuilder Properties(IDictionary<string, object> properties)
            {
                innerBuilder.Properties(properties);
                return this;
            }

            public IFluentLogBuilder TimeStamp(DateTime timeStamp)
            {
                innerBuilder.TimeStamp(timeStamp);
                return this;
            }

            public IFluentLogBuilder StackTrace(StackTrace stackTrace, int userStackFrame)
            {
                innerBuilder.StackTrace(stackTrace, userStackFrame);
                return this;
            }

            public void Write(string? callerMemberName = null, string? callerFilePath = null, int callerLineNumber = 0)
            {
                if (Depth.Value > 0) return;

                try
                {
                    Depth.Value += 1;
                    innerBuilder.Write(callerMemberName, callerFilePath, callerLineNumber);
                }
                finally
                {
                    Depth.Value = Math.Max(Depth.Value - 1, 0);
                }
            }

            public void WriteIf(Func<bool> condition, string? callerMemberName = null, string? callerFilePath = null, int callerLineNumber = 0)
            {
                if (Depth.Value > 0) return; // Drop nested write

                try
                {
                    Depth.Value += 1;
                    innerBuilder.WriteIf(condition, callerMemberName, callerFilePath, callerLineNumber);
                }
                finally
                {
                    Depth.Value = Math.Max(Depth.Value - 1, 0);
                }
            }

            public void WriteIf(bool condition, string? callerMemberName = null, string? callerFilePath = null, int callerLineNumber = 0)
            {
                if (!condition) return;
                if (Depth.Value > 0) return; // Drop nested write

                try
                {
                    Depth.Value += 1;
                    innerBuilder.WriteIf(condition, callerMemberName, callerFilePath, callerLineNumber);
                }
                finally
                {
                    Depth.Value = Math.Max(Depth.Value - 1, 0);
                }
            }
        }
    }
}
