using System;
using System.Threading;

namespace TA.Utils.Core.StateMachine;

/// <summary>
///     Extension methods for <see cref="IFiniteStateMachine{TState}"/> providing convenient
///     synchronous waiting semantics without exposing synchronisation primitives.
/// </summary>
public static class FiniteStateMachineExtensions
{
    /// <summary>
    ///     Blocks the calling thread until the provided <paramref name="predicate"/> evaluates to true
    ///     against the current state of <paramref name="fsm"/>, or the <paramref name="timeout"/> elapses.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="fsm">The finite state machine to observe.</param>
    /// <param name="predicate">A condition that must become true for the wait to complete.</param>
    /// <param name="timeout">Maximum time to wait before throwing <see cref="TimeoutException"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="fsm"/> or <paramref name="predicate"/> is null.</exception>
    /// <exception cref="TimeoutException">Thrown if the timeout elapses before the condition becomes true.</exception>
    public static void WaitUntil<TState>(this IFiniteStateMachine<TState> fsm, Func<TState?, bool> predicate, TimeSpan timeout)
        where TState : class, IState
    {
        if (fsm is null) throw new ArgumentNullException(nameof(fsm));
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        // Fast-path: already satisfied
        if (predicate(fsm.CurrentState)) return;

        using var gate = new ManualResetEventSlim(false);
        using var subscription = fsm.ObservableStates.Subscribe(state =>
        {
            try
            {
                if (predicate(state)) gate.Set();
            }
            catch
            {
                // Swallow predicate exceptions to avoid corrupting internals; wait will time out instead.
            }
        });

        // Re-check after subscribing to avoid missing a quick emission between first check and subscription
        if (predicate(fsm.CurrentState)) return;

        if (!gate.Wait(timeout)) throw new TimeoutException("Timed out waiting for condition in WaitUntil");
    }

    /// <summary>
    ///     Blocks until the specified <paramref name="expected"/> state instance becomes current, or until <paramref name="timeout"/>.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="fsm">The finite state machine to observe.</param>
    /// <param name="expected">The expected state instance.</param>
    /// <param name="timeout">Maximum time to wait.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="fsm"/> or <paramref name="expected"/> is null.</exception>
    /// <exception cref="TimeoutException">Thrown if the timeout elapses before <paramref name="expected"/> becomes current.</exception>
    public static void WaitUntil<TState>(this IFiniteStateMachine<TState> fsm, TState expected, TimeSpan timeout)
        where TState : class, IState
    {
        if (fsm is null) throw new ArgumentNullException(nameof(fsm));
        if (expected is null) throw new ArgumentNullException(nameof(expected));
        fsm.WaitUntil(s => ReferenceEquals(s, expected), timeout);
    }
}