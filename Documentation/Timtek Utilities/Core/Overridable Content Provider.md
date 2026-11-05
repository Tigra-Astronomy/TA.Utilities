# Overridable Content Provider

`IOverridableContentProvider` supplies named content items (JSON files, templates, images and so on) that ship embedded in an assembly as defaults, but which an operator can override by dropping a file with the same name into a configured directory.

Namespace: `TA.Utils.Core.OverridableContentProvider`

## How it works

1. The caller supplies a prioritised list of override directories (highest priority first) and an item name.
2. The first directory containing a file with that name wins.
3. If no override exists, the item is loaded from an embedded resource in a configured assembly.
4. If neither exists, `OverridableContentNotFoundException` is thrown (by `Open`) or `OverridableContentSource.NotFound` is reported (by `Resolve`).

The embedded resource name is `embeddedResourceRootNamespace` + `.` + the item name, with `/` and `\` in the item name replaced by `.`.

## Types

- `IOverridableContentProvider` - the abstraction. Inject this into your classes.
  - `Resolve(overrideDirectories, itemName)` returns an `OverridableContentResolution` describing where the item would come from, without opening it. Useful for diagnostics and logging.
  - `Open(overrideDirectories, itemName)` returns a readable `Stream` positioned at the start of the content.
- `OverridableContentProvider` - the implementation. Constructor parameters are the assembly holding the embedded resources, the root namespace of those resources, an `IFileSystem` (from the TestableIO `System.IO.Abstractions` package) and an `ILog`.
- `OverridableContentResolution` - a record with `Source` and `Path` (the override file path, or `null` unless `Source` is `Override`).
- `OverridableContentSource` - `Override`, `EmbeddedResource` or `NotFound`.
- `OverridableContentNotFoundException` - thrown when the item exists nowhere.

## Example

```csharp
var provider = new OverridableContentProvider(
    typeof(MyAssemblyMarker).Assembly,
    "MyAssembly.Assets.AcceptanceCriteria",
    new FileSystem(),
    log);

string[] searchPath = [@"C:\ProgramData\MyApp\Overrides", @"C:\Users\me\.myapp"];

var resolution = provider.Resolve(searchPath, "criteria.json");
log.Info()
    .Message("Using {source} for criteria.json")
    .Property("source", resolution.Source)
    .Write();

using var stream = provider.Open(searchPath, "criteria.json");
```

## Notes

- An empty directory list is valid; the provider then uses the embedded resource only.
- `IFileSystem` is used for all file access, so the provider can be tested with `MockFileSystem` from `TestableIO.System.IO.Abstractions.TestingHelpers`.
- `TA.Utils.Core` now depends on the `TestableIO.System.IO.Abstractions` package.

See also: [[Release Notes]].
