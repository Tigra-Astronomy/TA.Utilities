// This file is part of the TA.Utils project
// Copyright © 2015-2026 Timtek Systems Limited, all rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
//
// File: Program.cs  Last modified: 2026-02-23 by tim.long

using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Logging.OpenTelemetry;

namespace TA.Utils.Samples.OpenTelemetryConsoleApp;

/// <summary>
///     Demonstrates how to use the <see cref="ILog" /> logging service with the
///     OpenTelemetry back-end.  Log events are exported via OTLP and can be viewed
///     in any compatible collector such as Seq, Jaeger, or the Aspire Dashboard.
/// </summary>
/// <remarks>
///     <para>
///         To see the output locally, start a Seq instance (e.g. via Docker):
///         <c>docker run --rm -it -e ACCEPT_EULA=Y -p 5341:80 datalust/seq</c>
///         Then browse to <c>http://localhost:5341</c>.
///     </para>
///     <para>
///         The sample configures both a <see cref="TracerProvider" /> (for spans) and the
///         <see cref="OpenTelemetryLoggingService" /> (for structured logs).  A root span covers
///         the entire program lifetime, child spans wrap each iteration, and every log entry
///         written while a span is active is automatically correlated by trace/span ID.
///     </para>
/// </remarks>
internal static class Program
{
    private const string ServiceName = "OTelSampleConsoleApp";
    private const string SeqOtlpBase = "http://seq.devops.oceansignal.com:5341/ingest/otlp";
    private const string SeqApiKey = "JYtH6yxiC1SQQc6Pc6ui";

    private static readonly List<int> SuperstitiousNumbers = [13, 7, 666, 3, 8, 88, 888];

    private static readonly ActivitySource SampleTracer = new("TA.Utils.Samples.OTelConsole");

    private static async Task Main()
    {
        // --- Configure tracing (spans) ---
        // The TracerProvider listens to our ActivitySource and exports spans via OTLP.
        // Seq (and other OTLP collectors) will correlate logs with these spans.
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName))
            .AddSource(SampleTracer.Name)
            .AddOtlpExporter(otlp =>
            {
                otlp.Endpoint = new Uri(SeqOtlpBase + "/v1/traces");
                otlp.Protocol = OtlpExportProtocol.HttpProtobuf;
                otlp.Headers = $"X-Seq-ApiKey={SeqApiKey}";
            })
            .Build();

        // --- Configure logging ---
        var logOptions = OpenTelemetryLoggingServiceOptions.Default
            .WithOtlpEndpoint(new Uri(SeqOtlpBase + "/v1/logs"))
            .WithServiceName(ServiceName)
            .WithSeqApiKey(SeqApiKey)
            .WithConsoleLogging();

        using var log = new OpenTelemetryLoggingService(logOptions);
        log.WithAmbientProperty("CorrelationId", Guid.NewGuid());

        // --- Root span: covers the entire program lifetime ---
        using var programSpan = SampleTracer.StartActivity("ProgramRun", ActivityKind.Internal);
        programSpan?.SetTag("machine", Environment.MachineName);

        log.Info()
            .Message("Application starting")
            .Property("MachineName", Environment.MachineName)
            .Write();

        var seed = DateTime.Now.Millisecond;
        var gameOfChance = new Random(seed);
        log.Debug().Message("Random seed {Seed}", seed).Write();

        // --- Main loop: each iteration is a child span of ProgramRun ---
        for (var i = 0; i < 50; i++)
        {
            using var iterationSpan = SampleTracer.StartActivity("ProcessNumber");
            iterationSpan?.SetTag("number", i);

            try
            {
                log.Debug(1).Message("Starting iteration {Iteration}", i).Write();

                if (SuperstitiousNumbers.Contains(i))
                    throw new SuperstitiousNumberException(
                        $"Skipping {i} because it is a superstitious number");

                if (gameOfChance.Next(100) < 5)
                    throw new ApplicationException("Random failure");

                log.Trace(2).Message("Nothing special about {Iteration}", i).Write();
                iterationSpan?.SetTag("outcome", "ok");
            }
            catch (SuperstitiousNumberException ex)
            {
                iterationSpan?.SetStatus(ActivityStatusCode.Error, ex.Message);
                iterationSpan?.SetTag("outcome", "superstitious");

                log.Warn()
                    .Message("Superstitious number encountered: {Number}", i)
                    .Exception(ex)
                    .Property("SuperstitiousNumbers", SuperstitiousNumbers)
                    .Write();
            }
            catch (ApplicationException ae)
            {
                iterationSpan?.SetStatus(ActivityStatusCode.Error, ae.Message);
                iterationSpan?.SetTag("outcome", "random-failure");

                log.Error()
                    .Exception(ae)
                    .Message("Failed iteration {Iteration}", i)
                    .Write();
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            log.Debug(1).Message("Finished iteration {Iteration}", i).Write();
        }

        // --- Custom severity level ---
        log.Level("Important")
            .Message("All iterations complete")
            .Property("TotalIterations", 50)
            .Write();

        log.Info().Message("Application shutting down").Write();
        log.Shutdown();
    }
}
