namespace TA.Utils.Core.OverridableContentProvider;

/// <summary>
///     Thrown when a requested content item could not be found in any override directory, nor as an embedded
///     resource.
/// </summary>
public sealed class OverridableContentNotFoundException : Exception
{
    /// <summary>Initializes a new instance with the specified message.</summary>
    public OverridableContentNotFoundException(string message) : base(message) { }

    /// <summary>Initializes a new instance with the specified message and inner exception.</summary>
    public OverridableContentNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
