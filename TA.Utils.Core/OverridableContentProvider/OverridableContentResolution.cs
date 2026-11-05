namespace TA.Utils.Core.OverridableContentProvider;

/// <summary>
///     Describes the outcome of resolving a content item, without opening it.
/// </summary>
/// <param name="Source">Where the item was resolved from (or <see cref="OverridableContentSource.NotFound" />).</param>
/// <param name="Path">
///     The fully-qualified path to the override file, when <paramref name="Source" /> is
///     <see cref="OverridableContentSource.Override" />; <c>null</c> otherwise.
/// </param>
public sealed record OverridableContentResolution(OverridableContentSource Source, string? Path);
