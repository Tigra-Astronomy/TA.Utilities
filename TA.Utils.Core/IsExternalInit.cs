#if NETSTANDARD2_0
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    /// <summary>Polyfill enabling <c>init</c> accessors and records when targeting .NET Standard 2.0.</summary>
    internal static class IsExternalInit { }
}
#endif
