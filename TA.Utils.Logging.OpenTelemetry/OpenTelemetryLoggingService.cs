// This file is part of the TA.Utils project
// Copyright © 2015-2026 Timtek Systems Limited, all rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
//
// File: OpenTelemetryLoggingService.cs  Last modified: 2026-02-23 by tim.long

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Logging.OpenTelemetry;

/// <summary>
///     A logging service that uses OpenTelemetry as the back-end, exporting structured log records
///     via the OTLP protocol.  Log events are automatically correlated with active
///     <see cref="Activity" /> spans so that a Seq server (or any OTLP-compatible collector) can
///     associate logs with distributed traces.
/// </summary>
/// <remarks>
///     <para>
///         This service owns an <see cref="ILoggerFactory" /> configured with the OpenTelemetry
///         logging provider and the OTLP exporter.  The factory is disposed when
///         <see cref="Shutdown" /> is called, which flushes any buffered log records.
///     </para>
///     <para>
///         Typical Seq OTLP ingestion endpoint: <c>http://localhost:5341/ingest/otlp/v1/logs</c>.
///     </para>
/// </remarks>
public sealed class OpenTelemetryLoggingService : ILog, IDisposable
{
    private readonly IDictionary<string, object> ambientProperties = new Dictionary<string, object>();
    private readonly ILoggerFactory loggerFactory;
    private readonly OpenTelemetryLoggingServiceOptions options;
    private string sourceName;

    /// <summary>
    ///     Creates a new instance of the OpenTelemetry logging service.
    /// </summary>
    /// <param name="options">
    ///     Options that configure the OTLP exporter and service behaviour.
    ///     When <c>null</c>, <see cref="OpenTelemetryLoggingServiceOptions.Default" /> is used.
    /// </param>
    public OpenTelemetryLoggingService(OpenTelemetryLoggingServiceOptions? options = null)
    {
        this.options = options ?? OpenTelemetryLoggingServiceOptions.Default;

        try
        {
            var callerStackFrame = new StackFrame(1);
            var callerMethod = callerStackFrame.GetMethod();
            var callerType = callerMethod?.ReflectedType?.DeclaringType;
            sourceName = callerType?.Name ?? string.Empty;
        }
        catch
        {
            sourceName = "unknown";
        }

        loggerFactory = BuildLoggerFactory(this.options);
    }

    /// <inheritdoc />
    public IFluentLogBuilder Trace(int verbosity = 0, string? sourceNameOverride = null) =>
        CreateLogBuilder(LogLevel.Trace, verbosity, sourceNameOverride);

    /// <inheritdoc />
    public IFluentLogBuilder Debug(int verbosity = 0, string? sourceNameOverride = null) =>
        CreateLogBuilder(LogLevel.Debug, verbosity, sourceNameOverride);

    /// <inheritdoc />
    public IFluentLogBuilder Info(int verbosity = 0, string? sourceNameOverride = null) =>
        CreateLogBuilder(LogLevel.Information, verbosity, sourceNameOverride);

    /// <inheritdoc />
    public IFluentLogBuilder Warn(int verbosity = 0, string? sourceNameOverride = null) =>
        CreateLogBuilder(LogLevel.Warning, verbosity, sourceNameOverride);

    /// <inheritdoc />
    public IFluentLogBuilder Error(int verbosity = 0, string? sourceNameOverride = null) =>
        CreateLogBuilder(LogLevel.Error, verbosity, sourceNameOverride);

    /// <inheritdoc />
    public IFluentLogBuilder Fatal(int verbosity = 0, string? sourceNameOverride = null) =>
        CreateLogBuilder(LogLevel.Critical, verbosity, sourceNameOverride);

    /// <inheritdoc />
    public IFluentLogBuilder Level(string levelName, int verbosity = 0, string? sourceNameOverride = null)
    {
        var builder = CreateLogBuilder(LogLevel.Information, verbosity, sourceNameOverride);
        builder.Property(options.CustomLevelPropertyName, levelName);
        return builder;
    }

    /// <inheritdoc />
    public void Shutdown()
    {
        loggerFactory.Dispose();
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

    /// <inheritdoc />
    public void Dispose()
    {
        Shutdown();
    }

    private IFluentLogBuilder CreateLogBuilder(LogLevel logLevel, int verbosity, string? sourceNameOverride)
    {
        var effectiveName = sourceNameOverride ?? sourceName;
        var logger = string.IsNullOrWhiteSpace(effectiveName)
            ? loggerFactory.CreateLogger("Default")
            : loggerFactory.CreateLogger(effectiveName);

        var builder = new OpenTelemetryLogBuilder(logger, logLevel, ambientProperties);

        if (options.VerbosityEnabled)
            builder.Property(options.VerbosityPropertyName, verbosity);

        return builder;
    }

    private static ILoggerFactory BuildLoggerFactory(OpenTelemetryLoggingServiceOptions options)
    {
        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);

            if (options.ConsoleLoggingEnabled)
                builder.AddConsole();

            builder.AddOpenTelemetry(otelLogging =>
            {
                otelLogging.IncludeFormattedMessage = true;
                otelLogging.IncludeScopes = true;

                if (options.ServiceName is not null)
                    otelLogging.SetResourceBuilder(
                        ResourceBuilder.CreateDefault().AddService(options.ServiceName));

                otelLogging.AddOtlpExporter(otlp =>
                {
                    if (options.OtlpEndpoint is not null)
                        otlp.Endpoint = options.OtlpEndpoint;

                    otlp.Protocol = options.OtlpProtocol;

                    if (options.SeqApiKey is not null)
                        otlp.Headers = $"X-Seq-ApiKey={options.SeqApiKey}";
                });
            });
        });
    }
}