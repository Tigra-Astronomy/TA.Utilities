// This file is part of the TA.Utils project
// Copyright © 2015-2023 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: IFluentLogBuilder.cs  Last modified: 2023-09-01@08:38 by Tim Long

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace TA.Utils.Core.Diagnostics
{
    /// <summary>
    ///     Fluent Log Builder, loosely based on the NLog fluent API, which is simple but effective.
    /// </summary>
    public interface IFluentLogBuilder
    {
        /// <summary>
        ///     Add an exception to the log entry.
        /// </summary>
        IFluentLogBuilder Exception(Exception exception);

        /// <summary>
        ///     Set the log (source) name. By default, this is usually the name of the class or source file.
        /// </summary>
        IFluentLogBuilder LoggerName(string loggerName);

        /// <summary>
        ///     Sets the message template for the log entry.
        ///     The message may be a simple plain text string,
        ///     it may contain numbered substitution tokens like string.Format,
        ///     or it may contain named substitution tokens enclosed in {braces}
        /// </summary>
        IFluentLogBuilder Message([StructuredMessageTemplate] string message);

        /// <summary>
        ///     Sets the message template and property values for the log entry.
        ///     The format string may use numbered positional placeholders like string.Format,
        ///     or it may contain named substitution tokens enclosed in {braces}.
        /// </summary>
        IFluentLogBuilder Message([StructuredMessageTemplate] string format, params object[] args);

        /// <summary>
        ///     Sets the message template and property values for the log entry.
        ///     The format provider will be used when rendering the property values.
        /// </summary>
        IFluentLogBuilder Message(IFormatProvider provider, [StructuredMessageTemplate] string format,
            params object[] args);

        /// <summary>
        ///     Adds a named property and value pair to the log entry.
        /// </summary>
        IFluentLogBuilder Property(string name, object value);

        /// <summary>
        ///     Adds a collection of property/value pairs to the log entry.
        /// </summary>
        IFluentLogBuilder Properties(IDictionary<string, object> properties);

        /// <summary>
        ///     Sets the time stamp of the log entry.
        ///     If not set, the log entry will be timed at the moment Write() was called.
        /// </summary>
        IFluentLogBuilder TimeStamp(DateTime timeStamp);

        /// <summary>
        ///     Adds a stack trace to the log entry.
        /// </summary>
        IFluentLogBuilder StackTrace(StackTrace stackTrace, int userStackFrame);

        /// <summary>
        ///     Writes the log entry.
        /// </summary>
        void Write([CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = default);

        /// <summary>
        ///     Writes the log entry if the supplied predicate is true.
        /// </summary>
        void WriteIf(Func<bool> condition, [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = default);

        /// <summary>
        ///     Writes the log entry if the boolean condition is true.
        /// </summary>
        void WriteIf(bool condition, [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = default);
    }
}