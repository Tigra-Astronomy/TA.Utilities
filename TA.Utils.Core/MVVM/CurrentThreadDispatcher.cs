using System;

namespace TA.Utils.Core.MVVM;

/// <summary>
///     A no-op implementation of <see cref="IUiThreadDispatcher" /> that executes actions on the current thread.
///     This is useful for unit testing scenarios where UI thread dispatch is not available.
/// </summary>
public class CurrentThreadDispatcher : IUiThreadDispatcher
{
    /// <summary>
    ///     Execute an action on the current thread immediately.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void Post(Action action) => action();
}
