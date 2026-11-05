namespace TA.Utils.Core.OverridableContentProvider;

/// <summary>
///     Provides named content items, searching a prioritised list of override directories first, then falling
///     back to an embedded resource in a configured assembly if no override is found.
/// </summary>
public interface IOverridableContentProvider
{
    /// <summary>
    ///     Resolves where a named content item would be loaded from, without opening it. Useful for diagnostics
    ///     and logging.
    /// </summary>
    /// <param name="overrideDirectories">
    ///     Directories to search for an override file, in priority order (highest priority first). The first
    ///     directory containing a file named <paramref name="itemName" /> wins. May be empty if there are no
    ///     override directories to search (e.g. the caller has none configured for the current context).
    /// </param>
    /// <param name="itemName">
    ///     The name of the content item, e.g. <c>"MOB2-Acceptance-Criteria.json"</c>. Used both as the file name to
    ///     search for in the override directories, and (with path separators replaced by '.') as the suffix of the
    ///     embedded resource name.
    /// </param>
    /// <returns>An <see cref="OverridableContentResolution" /> describing the source (or lack thereof).</returns>
    OverridableContentResolution Resolve(IReadOnlyList<string> overrideDirectories, string itemName);

    /// <summary>
    ///     Opens a stream over the requested content item, searching the override directories first and falling
    ///     back to the embedded resource.
    /// </summary>
    /// <param name="overrideDirectories">The override directories to search. See <see cref="Resolve" /> for details.</param>
    /// <param name="itemName">The name of the content item. See <see cref="Resolve" /> for details.</param>
    /// <returns>A readable stream positioned at the start of the content.</returns>
    /// <exception cref="OverridableContentNotFoundException">
    ///     Thrown if the item could not be found in any override directory, nor as an embedded resource.
    /// </exception>
    Stream Open(IReadOnlyList<string> overrideDirectories, string itemName);
}
