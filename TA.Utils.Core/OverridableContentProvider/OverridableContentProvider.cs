using System.IO.Abstractions;
using System.Reflection;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Core.OverridableContentProvider;

/// <summary>
///     Provides named content items, searching a prioritised list of override directories first, then falling
///     back to an embedded resource in a configured assembly if no override is found.
/// </summary>
/// <param name="embeddedResourceAssembly">The assembly containing the embedded resource fallback content.</param>
/// <param name="embeddedResourceRootNamespace">
///     The fully-qualified namespace prefix under which embedded resources are rooted, e.g.
///     <c>"MyAssembly.Assets.AcceptanceCriteria"</c>. Combined with <c>itemName</c> (path separators replaced by
///     '.') to build the embedded resource name.
/// </param>
/// <param name="fileSystem">A file system abstraction, used to search the override directories.</param>
/// <param name="log">A logging service.</param>
public sealed class OverridableContentProvider(
    Assembly    embeddedResourceAssembly,
    string      embeddedResourceRootNamespace,
    IFileSystem fileSystem,
    ILog        log) : IOverridableContentProvider
{
    /// <inheritdoc />
    public OverridableContentResolution Resolve(IReadOnlyList<string> overrideDirectories, string itemName)
    {
        var overridePath = FindOverridePath(overrideDirectories, itemName);
        if (overridePath is not null)
        {
            log.Debug().Message("Resolved {itemName} to override file {path}", itemName, overridePath).Write();
            return new OverridableContentResolution(OverridableContentSource.Override, overridePath);
        }

        if (EmbeddedResourceExists(itemName))
        {
            log.Debug().Message("Resolved {itemName} to embedded resource", itemName).Write();
            return new OverridableContentResolution(OverridableContentSource.EmbeddedResource, null);
        }

        log.Debug().Message("Unable to resolve {itemName}; no override or embedded resource found", itemName).Write();
        return new OverridableContentResolution(OverridableContentSource.NotFound, null);
    }

    /// <inheritdoc />
    public Stream Open(IReadOnlyList<string> overrideDirectories, string itemName)
    {
        var resolution = Resolve(overrideDirectories, itemName);
        switch (resolution.Source)
        {
            case OverridableContentSource.Override:
                return fileSystem.File.OpenRead(resolution.Path!);
            case OverridableContentSource.EmbeddedResource:
                return OpenEmbeddedResource(itemName);
            case OverridableContentSource.NotFound:
            default:
                throw new OverridableContentNotFoundException(
                    $"Unable to find content item '{itemName}' in any override directory " +
                    $"({string.Join(", ", overrideDirectories)}) or as an embedded resource " +
                    $"({BuildEmbeddedResourceName(itemName)}) in assembly '{embeddedResourceAssembly.GetName().Name}'.");
        }
    }

    private string? FindOverridePath(IReadOnlyList<string> overrideDirectories, string itemName)
    {
        foreach (var directory in overrideDirectories)
        {
            var candidatePath = fileSystem.Path.Combine(directory, itemName);
            if (fileSystem.File.Exists(candidatePath))
                return candidatePath;
        }

        return null;
    }

    private bool EmbeddedResourceExists(string itemName) =>
        Array.IndexOf(embeddedResourceAssembly.GetManifestResourceNames(), BuildEmbeddedResourceName(itemName)) >= 0;

    private Stream OpenEmbeddedResource(string itemName)
    {
        var resourceName = BuildEmbeddedResourceName(itemName);
        var stream = embeddedResourceAssembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new OverridableContentNotFoundException(
                $"Embedded resource '{resourceName}' was not found in assembly " +
                $"'{embeddedResourceAssembly.GetName().Name}', even though it was previously resolved.");
        return stream;
    }

    private string BuildEmbeddedResourceName(string itemName)
    {
        var normalizedItemName = itemName.Replace('/', '.').Replace('\\', '.');
        return $"{embeddedResourceRootNamespace}.{normalizedItemName}";
    }
}
