# OpenTelemetry Adapter

An implementation of the logging abstraction (`ILog`/`IFluentLogBuilder`) that exports structured log records via the OpenTelemetry Protocol (OTLP). It maps fluent builder calls to `Microsoft.Extensions.Logging.ILogger` configured with the OpenTelemetry provider and the OTLP exporter. Log records are automatically correlated with active `System.Diagnostics.Activity` spans.

- Package: `TA.Utils.Logging.OpenTelemetry`
- Classes: `OpenTelemetryLoggingService`, `OpenTelemetryLogBuilder`, `OpenTelemetryLoggingServiceOptions`

## Quick start

```csharp
using TA.Utils.Logging.OpenTelemetry;

var options = OpenTelemetryLoggingServiceOptions.Default
    .WithOtlpEndpoint(new Uri("http://localhost:5341/ingest/otlp/v1/logs"))
    .WithServiceName("MyApplication")
    .WithSeqApiKey("your-api-key")
    .UseVerbosity()
    .WithConsoleLogging();

using var log = new OpenTelemetryLoggingService(options);
log.WithAmbientProperty("CorrelationId", Guid.NewGuid());

log.Info().Message("Hello {who}").Property("who", "world").Write();
log.Level("Important").Message("This is important").Write();
```

## Configuration options

All options are set via the fluent API on `OpenTelemetryLoggingServiceOptions`:

| Method | Purpose |
|---|---|
| `WithOtlpEndpoint(uri)` | OTLP endpoint URI. Falls back to the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable, then `http://localhost:4317`. |
| `WithOtlpProtocol(protocol)` | Export protocol. Defaults to `HttpProtobuf` (what Seq expects). |
| `WithServiceName(name)` | OpenTelemetry service name shown in the collector. Falls back to `OTEL_SERVICE_NAME`. |
| `WithSeqApiKey(key)` | Sent as the `X-Seq-ApiKey` header. |
| `UseVerbosity(propertyName)` | Adds a numeric verbosity property (default `"verbosity"`) to every entry. |
| `WithCustomLevelPropertyName(name)` | Property name used by `ILog.Level()` for custom severity (default `"CustomLevel"`). |
| `WithConsoleLogging()` | Also writes log entries to the console via `Microsoft.Extensions.Logging.Console`. |

## Trace correlation

Log entries written while a `System.Diagnostics.Activity` is active automatically carry the trace and span IDs. To export those traces, set up a `TracerProvider` alongside the logging service:

```csharp
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var tracer = new ActivitySource("MyApplication.Tracer");

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyApplication"))
    .AddSource(tracer.Name)
    .AddOtlpExporter(otlp =>
    {
        otlp.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
        otlp.Protocol = OtlpExportProtocol.HttpProtobuf;
    })
    .Build();

// Logs emitted inside this span are automatically correlated
using var span = tracer.StartActivity("ImportantOperation");
log.Info().Message("Starting important operation").Write();
```

In Seq (or any OTLP-aware UI) you can then view the trace waterfall and see log events grouped under their parent spans.

## Logger names and ambient properties

- `WithName("Source")` sets the logger category name used by `ILoggerFactory.CreateLogger()`.
- `WithAmbientProperty("name", value)` adds a property to all subsequent log entries; useful for correlation IDs, component identifiers, tenant, etc.

## Message templates and properties

- Use either indexed placeholders (`{0}`) or named placeholders (`{name}`); named placeholders are recommended for semantic logging.
- Attach additional properties explicitly via `.Property("name", value)` or `.Properties(dict)`.

## Shutdown

Call `log.Shutdown()` (or `Dispose()`) to flush buffered log records and release the underlying `ILoggerFactory`. The service implements `IDisposable`, so `using var log = ...` handles this automatically.

## Migrating from the NLog adapter

Because both adapters implement `ILog`, migrating is straightforward:

1. Replace `LoggingService` with `OpenTelemetryLoggingService`.
2. Replace `LogServiceOptions` with `OpenTelemetryLoggingServiceOptions`.
3. Remove `NLog.config` — configuration is now in code via the fluent options API.
4. All existing `log.Info().Message(...).Write()` calls remain unchanged.

The `NLog.config` file and the `NLog.Targets.Seq` package are no longer needed; the OTLP exporter sends data directly to the collector.

## Sample application

A complete working example is provided in `Samples/TA.Utils.Samples.OpenTelemetryConsoleApp`. The sample demonstrates structured logging at all severity levels, ambient properties, exception logging, custom severity levels, and trace/span correlation with `ActivitySource`.

## See also

- [[Diagnostics and Logging]]
- [[Logging/NLog Adapter|NLog Adapter]]
- [[Core/Console Logger (Diagnostics)|Console logger (Diagnostics)]]
