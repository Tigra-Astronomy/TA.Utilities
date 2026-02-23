// This file is part of the TA.Utils project
// Copyright © 2015-2026 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: OpenTelemetryLogBuilder.cs  Last modified: 2026-02-23 by tim.long

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Logging.OpenTelemetry;

/// <summary>
///     Fluent log builder that collects log entry data and emits it through an
///     <see cref="ILogger" /> configured with the OpenTelemetry OTLP exporter.
///     Active <see cref="Activity" /> spans are automatically correlated with the log record
///     by the OpenTelemetry logging provider.
/// </summary>
internal sealed partial class OpenTelemetryLogBuilder : IFluentLogBuilder
{
    private static readonly Regex PropertyNameMatcher = PropertyNamePattern();

    private readonly ILogger logger;
    private readonly LogLevel logLevel;
    private readonly LogEntryState state = new();
    private Exception? exception;

    /// <summary>
    ///     Creates a new log builder for the given logger and severity level.
    /// </summary>
    internal OpenTelemetryLogBuilder(ILogger logger, LogLevel logLevel, IDictionary<string, object>? ambientProperties = null)
    {
        this.logger = logger;
        this.logLevel = logLevel;

        if (ambientProperties is { Count: > 0 })
            foreach (var property in ambientProperties)
                state.AddProperty(property.Key, property.Value);
    }

    /// <inheritdoc />
    public IFluentLogBuilder Exception(Exception exception)
    {
        this.exception = exception;
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder LoggerName(string loggerName)
    {
        // ILogger category is fixed at creation time; store as a property instead
        state.AddProperty("LoggerName", loggerName);
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder Message(string message)
    {
        state.SetMessageTemplate(message);
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder Message(string format, params object[] args)
    {
        state.SetMessageTemplate(format);
        var names = GetPropertyNamesFromMessageTemplate(format);
        if (names.Count != args.Length)
            throw new ArgumentException(
                $"Mismatched arguments. Format has {names.Count} placeholders but there are {args.Length} arguments.");
        for (var i = 0; i < args.Length; i++)
            state.AddProperty(names[i], args[i]);
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder Message(IFormatProvider provider, string format, params object[] args)
    {
        state.SetFormatProvider(provider);
        return Message(format, args);
    }

    /// <inheritdoc />
    public IFluentLogBuilder Property(string name, object value)
    {
        state.AddProperty(name, value);
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder Properties(IDictionary<string, object> properties)
    {
        foreach (var property in properties)
            state.AddProperty(property.Key, property.Value);
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder TimeStamp(DateTime timeStamp)
    {
        // ILogger does not expose a timestamp setter; store as a property for the exporter
        state.AddProperty("Timestamp", timeStamp.ToUniversalTime());
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder StackTrace(StackTrace stackTrace, int userStackFrame)
    {
        state.AddProperty("StackTrace", stackTrace.ToString());
        return this;
    }

    /// <inheritdoc />
    public void Write(
        [CallerMemberName] string? callerMemberName = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int callerLineNumber = default)
    {
        EmitLogEntry(callerMemberName, callerFilePath, callerLineNumber);
    }

    /// <inheritdoc />
    public void WriteIf(
        Func<bool> condition,
        [CallerMemberName] string? callerMemberName = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int callerLineNumber = default)
    {
        if (condition())
            EmitLogEntry(callerMemberName, callerFilePath, callerLineNumber);
    }

    /// <inheritdoc />
    public void WriteIf(
        bool condition,
        [CallerMemberName] string? callerMemberName = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int callerLineNumber = default)
    {
        if (condition)
            EmitLogEntry(callerMemberName, callerFilePath, callerLineNumber);
    }

    private void EmitLogEntry(string? callerMemberName, string? callerFilePath, int callerLineNumber)
    {
        if (!logger.IsEnabled(logLevel))
            return;

        if (callerMemberName is not null)
            state.AddProperty("CallerMemberName", callerMemberName);
        if (callerFilePath is not null)
            state.AddProperty("CallerFilePath", callerFilePath);
        if (callerLineNumber != default)
            state.AddProperty("CallerLineNumber", callerLineNumber);

        var frozenState = state.Freeze();

        logger.Log(
            logLevel,
            eventId: default,
            state: frozenState,
            exception: exception,
            formatter: static (s, _) => s.RenderMessage());
    }

    private static List<string> GetPropertyNamesFromMessageTemplate(string format)
    {
        var matches = PropertyNameMatcher.Matches(format);
        var names = new List<string>(matches.Count);
        foreach (Match match in matches)
            names.Add(match.Groups["propertyName"].Value);
        return names;
    }

    [GeneratedRegex(@"\{[@$]?(?<propertyName>\w+)\}", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)]
    private static partial Regex PropertyNamePattern();
}
