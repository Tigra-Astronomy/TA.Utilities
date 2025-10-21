# Console logger (Diagnostics)

A minimal, dependency‑free console implementation of the logging abstraction, useful for small tools and early development. It implements `ILog`/`IFluentLogBuilder` and writes entries to the console with optional property rendering and verbosity support.

- Class: `ConsoleLoggerService`
- Builder: `ConsoleLogBuilder`
- Options: `ConsoleLoggerOptions`

## Quick start

```csharp
var options = ConsoleLoggerOptions.DefaultOptions
    .UseVerbosity()
    .RenderProperties(true)
    .RenderSeverityLevels("Info", "Warn", "Error");

ILog log = new ConsoleLoggerService(options);
log.Info(2).Message("Hello {who}").Property("who", "world").Write();
```

## Options

- UseVerbosity(propertyName = "verbosity")
  - Adds a property to each entry (default name "verbosity"). The number passed to the severity method is recorded.
  - Example: `log.Info(2)` → property `{ verbosity: 2 }`.
- RenderProperties(enable = true)
  - Controls whether the property bag is emitted to the console. Enabled by default.
- RenderAllSeverityLevels()
  - Resets filtering so that all severities are shown (the default unless modified).
- RenderSeverityLevels(params string[] levels)
  - An allow‑list for severities to render (e.g., "Info", "Warn", "Error"). Removing a level implicitly overrides previous includes.
- IgnoreSeverityLevels(params string[] levels)
  - A deny‑list; takes precedence over the allow‑list.

Behaviour
- If Ignore includes a level, it is not rendered.
- If Render list is empty, all levels are rendered by default.

## Ambient properties and names

- `WithAmbientProperty(name, value)` adds a property to all subsequent entries.
- `WithName(source)` sets the logger’s source name for downstream filtering or routing.

## Re‑entrancy guard (optional)

To prevent recursive log writes (e.g., logging during serialisation) you can wrap any logger in the guard decorator:

```csharp
ILog baseLog = new ConsoleLoggerService();
ILog log = new ReentrancyGuardLog(baseLog);
```

If a write is attempted while another write is in progress on the same async‑flow, the nested write is dropped.

See also
- [[Diagnostics and Logging]]
- [[Core/ASCII Mnemonic Expansion|ASCII Mnemonic Expansion]]
