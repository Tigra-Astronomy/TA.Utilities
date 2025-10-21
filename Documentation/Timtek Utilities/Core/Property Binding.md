# Property Binding Utilities

The property binding helpers enable you to populate plain C# objects from simple key–value data, such as INI‑style text files, configuration dumps, or instrument logs. You describe how fields map to properties using a small attribute, and a binder performs type conversion and assignment for you.

Components
- Data source: a sequence of `KeyValueDataRecord` instances
- Reader: `KeyValueReader` turns a text stream into `KeyValueDataRecord` items
- Attribute: `DataKeyAttribute` decorates properties with the key name and an optional sequence
- Binder: `PropertyBinder` maps records to properties, including collections

Typical use cases
- Importing device or test logs into typed DTOs
- Parsing simple configuration files for applications and utilities
- Converting textual dumps into strongly typed objects for validation and processing

## Key–value text format

- One pair per line, with a delimiter between key and value. Default delimiters: `:` `=` `#`.
- Lines beginning with a comment character are ignored. Default comment openers: `#`.
- Whitespace around keys and values is trimmed.
- Lines that cannot be parsed into a pair are skipped.

You can override delimiters and comment characters when constructing `KeyValueReader`.

```csharp
using var stream = File.OpenRead(path);
using var reader = new KeyValueReader(stream, delimiters: new[] { ':', '=' }, commentChars: new[] { '#', ';' });
IEnumerable<KeyValueDataRecord> records = reader.KeyValueDataRecords();
```

## Decorating a target type

Annotate properties with `DataKeyAttribute` when the key name differs from the property name, or when you want to supply multiple fallbacks and ordering using `Sequence`.

```csharp
public sealed class DeviceInfo
{
    // Matches either the explicit key "model" or, if omitted, the property name "Model" (case‑insensitive)
    [DataKey("model")]
    public string Model { get; set; }

    // Multiple fallbacks with ordered preference
    [DataKey("serial", Sequence = 0)]
    [DataKey("sn",     Sequence = 1)]
    public string SerialNumber { get; set; }

    // Collection properties must use DataKeyAttribute; all matching values are added
    [DataKey("alias")]
    [DataKey("alt_name")] // both keys, accumulated
    public List<string> Aliases { get; set; }
}
```

Rules
- If one or more `DataKeyAttribute` decorations are present, their `Keyword` values are used in ascending `Sequence` order to search for matching records.
- If no attribute is present, the property’s name is used as the key (case‑insensitive).
- For simple properties, the first successfully converted value is assigned.
- For collection properties that implement `IList` or `IList<T>`, all matching values across all specified keys are converted and added to the collection.

## Binding

Create a binder and produce an instance of your DTO from a sequence of records. The binder logs diagnostic information via the repository’s logging abstraction.

```csharp
var binder = new PropertyBinder();
DeviceInfo dto = binder.BindProperties<DeviceInfo>(records);
```

Behaviour when keys are missing or conversion fails
- If no matching record is found for a property, the property is left at its default value.
- If conversion fails for a record, the binder logs an error and leaves the property unchanged. Other properties continue to bind.

## Type conversion

The binder uses `TypeDescriptor` to convert strings to the destination type using `InvariantCulture`. Special cases:
- Strings are trimmed and returned as‑is.
- `Nullable<T>` is supported; the underlying type is used for conversion.
- For custom types, provide a `TypeConverter` or bind as `string` and convert later.

Examples

```csharp
public sealed class Reading
{
    public string Id { get; set; }
    public int Count { get; set; }              // "42" → 42
    public double TemperatureC { get; set; }    // "23.5" → 23.5 (invariant culture)
    public DateTime Timestamp { get; set; }     // "2025-09-23T10:00:00Z" → DateTime
    public int? OptionalLevel { get; set; }     // supports Nullable<int>
}
```

```csharp
var text = @"
# Device log
model = ZX-200
serial = A1B2C3
alias = ""Alpha Unit""
alt_name = ""Primary""
Count: 42
TemperatureC: 23.5
Timestamp: 2025-09-23T10:00:00Z
";
using var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
using var reader = new KeyValueReader(ms);
var records = reader.KeyValueDataRecords();

var binder = new PropertyBinder();
var info = binder.BindProperties<DeviceInfo>(records);
var reading = binder.BindProperties<Reading>(records);
```

## Collections

When a destination property implements `IList` or `IList<T>`, property binding will:
- Require `DataKeyAttribute` on the property to specify which keys to aggregate.
- Convert all matching values across all listed keys.
- Instantiate the target collection type if it is a concrete `IList` implementation; otherwise, create a `List<T>` at runtime and assign it to the property.

```csharp
public sealed class Tags
{
    [DataKey("tag")]
    [DataKey("label", Sequence = 1)]
    public IList<string> Values { get; set; }
}
```

Input like:

```text
label = good
label = fast
tag = cheap
```

binds to `Values = ["good", "fast", "cheap"]` (order follows appearance in the data).

## Case sensitivity and ordering

- Key lookup is case‑insensitive.
- When multiple `DataKeyAttribute`s decorate a property, they are processed in ascending `Sequence` order.
- For simple properties, the first found value wins. For collections, all matching values are accumulated in the order encountered.

## Error handling and logging

- Conversion failures raise an `InvalidOperationException` internally and are logged through the `ILog` abstraction. Binding then proceeds to the next property.
- For collections, if the collection cannot be instantiated, the binder logs a warning and skips population.

## Practical tips

- Prefer explicit `DataKeyAttribute` on public DTOs, even when keys match property names, to avoid hidden coupling and to document intent.
- Keep DTOs simple and immutable where practical; for immutable records, bind into a mutable intermediate and project.
- If you need culture‑specific number formats, normalise input to invariant format at source or add a preprocessing pass.
- For complex transformations, bind strings first and convert with domain‑specific logic afterwards.

## Reference

- `KeyValueReader` — parse a text stream into `KeyValueDataRecord` items (configurable delimiters and comments)
- `KeyValueDataRecord` — holds a `Key` and a `Value` string
- `DataKeyAttribute` — decorate properties with a `Keyword` and optional `Sequence`
- `PropertyBinder`
  - `TOut BindProperties<TOut>(IEnumerable<KeyValueDataRecord>) where TOut : new()`

See also
- [[Diagnostics and Logging]]
- [[Async Helpers]]
- [[Core/Finite State Machine|Finite State Machine]]
