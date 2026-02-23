// This file is part of the TA.Utils project
// Copyright © 2015-2026 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: OpenTelemetryLoggingServiceOptions.cs  Last modified: 2026-02-23 by tim.long

using OpenTelemetry.Exporter;

namespace TA.Utils.Logging.OpenTelemetry;

/// <summary>
///     Options that configure the <see cref="OpenTelemetryLoggingService" />.
///     Use the fluent builder methods to customise the service before passing it to the constructor.
/// </summary>
public sealed class OpenTelemetryLoggingServiceOptions
{
    internal const string VerbosityDefaultPropertyName = "verbosity";
    internal const string CustomLevelDefaultPropertyName = "CustomLevel";

    /// <summary>
    ///     The OTLP endpoint URI (e.g. <c>http://localhost:5341/ingest/otlp/v1/logs</c> for Seq).
    ///     When <c>null</c>, the OTLP exporter falls back to the <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> environment variable
    ///     or its built-in default (<c>http://localhost:4317</c>).
    /// </summary>
    public Uri? OtlpEndpoint { get; private set; }

    /// <summary>
    ///     The OTLP export protocol.  Defaults to <see cref="OtlpExportProtocol.HttpProtobuf" /> which is
    ///     the protocol expected by Seq's OTLP ingestion endpoint.
    /// </summary>
    public OtlpExportProtocol OtlpProtocol { get; private set; } = OtlpExportProtocol.HttpProtobuf;

    /// <summary>
    ///     Optional API key sent as the <c>X-Seq-ApiKey</c> header when exporting to Seq.
    /// </summary>
    public string? SeqApiKey { get; private set; }

    /// <summary>
    ///     The OpenTelemetry service name that identifies this application in traces and logs.
    ///     When <c>null</c>, the OTLP exporter falls back to the <c>OTEL_SERVICE_NAME</c> environment variable.
    /// </summary>
    public string? ServiceName { get; private set; }

    /// <summary>
    ///     When <c>true</c>, a <c>verbosity</c> property is added to every log entry.
    /// </summary>
    public bool VerbosityEnabled { get; private set; }

    /// <summary>
    ///     The property name used for the verbosity value. Defaults to <c>"verbosity"</c>.
    /// </summary>
    public string VerbosityPropertyName { get; private set; } = VerbosityDefaultPropertyName;

    /// <summary>
    ///     The property name used for custom severity levels. Defaults to <c>"CustomLevel"</c>.
    /// </summary>
    public string CustomLevelPropertyName { get; private set; } = CustomLevelDefaultPropertyName;

    /// <summary>
    ///     When <c>true</c>, log entries are also written to the console via the
    ///     <c>Microsoft.Extensions.Logging.Console</c> provider.
    /// </summary>
    public bool ConsoleLoggingEnabled { get; private set; }

    /// <summary>
    ///     Returns a new instance loaded with default options, which can then be modified with the fluent API.
    /// </summary>
    public static OpenTelemetryLoggingServiceOptions Default => new();

    /// <summary>
    ///     Sets the OTLP endpoint URI.
    /// </summary>
    public OpenTelemetryLoggingServiceOptions WithOtlpEndpoint(Uri endpoint)
    {
        OtlpEndpoint = endpoint;
        return this;
    }

    /// <summary>
    ///     Sets the OTLP export protocol.
    /// </summary>
    public OpenTelemetryLoggingServiceOptions WithOtlpProtocol(OtlpExportProtocol protocol)
    {
        OtlpProtocol = protocol;
        return this;
    }

    /// <summary>
    ///     Sets the Seq API key, which is sent as the <c>X-Seq-ApiKey</c> header.
    /// </summary>
    public OpenTelemetryLoggingServiceOptions WithSeqApiKey(string apiKey)
    {
        SeqApiKey = apiKey;
        return this;
    }

    /// <summary>
    ///     Sets the OpenTelemetry service name.
    /// </summary>
    public OpenTelemetryLoggingServiceOptions WithServiceName(string serviceName)
    {
        ServiceName = serviceName;
        return this;
    }

    /// <summary>
    ///     Enables the verbosity property on log entries.
    /// </summary>
    public OpenTelemetryLoggingServiceOptions UseVerbosity(string propertyName = VerbosityDefaultPropertyName)
    {
        VerbosityEnabled = true;
        VerbosityPropertyName = propertyName;
        return this;
    }

    /// <summary>
    ///     Sets the property name used for custom severity levels.
    /// </summary>
    public OpenTelemetryLoggingServiceOptions WithCustomLevelPropertyName(string propertyName = CustomLevelDefaultPropertyName)
    {
        CustomLevelPropertyName = propertyName;
        return this;
    }

    /// <summary>
    ///     Enables console logging in addition to OTLP export.
    /// </summary>
    public OpenTelemetryLoggingServiceOptions WithConsoleLogging()
    {
        ConsoleLoggingEnabled = true;
        return this;
    }
}
