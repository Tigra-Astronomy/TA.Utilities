namespace TA.Utils.Core.OverridableContentProvider;

/// <summary>
///     Describes where a requested content item was ultimately resolved from.
/// </summary>
public enum OverridableContentSource
{
    /// <summary>
    ///     The item was found in one of the configured override directories.
    /// </summary>
    Override,

    /// <summary>
    ///     No override was found; the item was found as an embedded resource in the configured assembly.
    /// </summary>
    EmbeddedResource,

    /// <summary>
    ///     The item could not be found in any override directory, nor as an embedded resource.
    /// </summary>
    NotFound
}
