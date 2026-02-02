using System;

namespace TA.Utils.Core.MVVM;

/// <summary>
///     Abstraction for dispatching work to the UI thread.
///     This interface allows commands to be testable in unit tests where UI thread dispatch may not be available.
/// </summary>
public interface IUiThreadDispatcher
{
    /// <summary>
    ///     Execute an action on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute on the UI thread.</param>
    void Post(Action action);
}

/// <summary>
///     Factory and context manager for UI thread dispatchers.
///     This allows the dispatcher to be configured globally (e.g., per test) without modifying command constructors.
/// </summary>
/// <remarks>
///     The dispatcher must be explicitly set via <see cref="SetDispatcher" /> before use.
///     For WinForms applications, set this to a WinForms-specific dispatcher during application startup.
///     For unit tests, use <see cref="CurrentThreadDispatcher" />.
/// </remarks>
public static class UiThreadDispatcherContext
{
    [ThreadStatic]
    private static IUiThreadDispatcher? dispatcher;

    /// <summary>
    ///     Gets the current UI thread dispatcher.
    ///     If a dispatcher has been set (via <see cref="SetDispatcher" />), returns that.
    ///     Otherwise, returns a <see cref="CurrentThreadDispatcher" /> as a fallback.
    /// </summary>
    public static IUiThreadDispatcher Current => dispatcher ?? new CurrentThreadDispatcher();

    /// <summary>
    ///     Sets the dispatcher for the current thread.
    ///     For WinForms applications, call this during startup with a WinForms-specific dispatcher.
    ///     For unit tests, use <see cref="CurrentThreadDispatcher" />.
    /// </summary>
    /// <param name="value">The dispatcher to use, or null to clear and revert to the default.</param>
    public static void SetDispatcher(IUiThreadDispatcher? value)
    {
        dispatcher = value;
    }
}
