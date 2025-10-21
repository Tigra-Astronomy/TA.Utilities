# NLog adapter

An implementation of the logging abstraction (`ILog`/`IFluentLogBuilder`) that targets NLog as the back‑end. It maps fluent builder calls to NLog’s `LogEventInfo`, supports semantic properties, and integrates cleanly with NLog targets including Seq.

- Package: TA.Utils.Logging.Nlog
- Classes: `LoggingService`, `LogBuilder`, `LogServiceOptions`

## Quick start

```csharp
using TA.Utils.Logging.NLog;

var options = LogServiceOptions.DefaultOptions
    .UseVerbosity()
    .CustomSeverityPropertyName("CustomLevel");

ILog log = new LoggingService(options)
    .WithAmbientProperty("CorrelationId", Guid.NewGuid());

log.Info().Message("Hello {who}").Property("who", "world").Write();
log.Level("Important").Message("This is important").Write();
```

## Verbosity and custom levels

- UseVerbosity(propertyName = "verbosity")
  - Adds a numeric verbosity property to each entry when you pass the number to the severity method (e.g., `Info(2)`).
- CustomSeverityPropertyName(name = "CustomLevel")
  - The custom severity property used when you call `log.Level("Name")`.

### Seq target mapping example

To use custom levels with Seq, configure the Seq target to take its level from the custom property when present:

```xml
<target xsi:type="Seq" name="seq"
        serverUrl="http://your-server-url:5341"
        apiKey="your-seq-api-key"
        seqLevel="${event-properties:CustomLevel:whenEmpty=${level}}" />
```

## Logger names and ambient properties

- `WithName("Source")` sets the logger name, which NLog can use for routing and filtering (e.g., `${logger}` in layouts).
- `WithAmbientProperty("name", value)` adds a property to all subsequent entries; useful for correlation IDs, component, tenant, etc.

## Message templates and properties

- Use either indexed placeholders (`{0}`) or named placeholders (`{name}`); named placeholders are recommended for semantic logging.
- Attach additional properties explicitly via `.Property("name", value)` or `.Properties(dict)`.

## Shutdown

- Call `log.Shutdown()` or rely on NLog’s `AutoShutdown = true` set by the adapter type initialiser.

## Troubleshooting

- If a property name collides, the adapter auto‑deconflicts by suffixing a number (e.g., `property`, `property2`).
- Ensure `IsEnabled(Level)` is true for the chosen level; otherwise `.Write()` is a no‑op.

See also
- [[Diagnostics and Logging]]
- [[Core/Console Logger (Diagnostics)|Console logger (Diagnostics)]]
- [[Core/ASCII Mnemonic Expansion|ASCII Mnemonic Expansion]]