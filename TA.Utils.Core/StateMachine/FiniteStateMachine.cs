using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Core.StateMachine;

/// <summary>
///     Concrete implementation of a generic finite state machine.
///     Orchestrates cancellation, orderly exit/enter, and asynchronous run for each state.
/// </summary>
/// <typeparam name="TState">The state type managed by this state machine. Must implement <see cref="IState"/>.</typeparam>
public sealed class FiniteStateMachine<TState> : IFiniteStateMachine<TState> where TState : class, IState
{
    private readonly ILog            log;
    private          Subject<TState> currentStateSubject = new();
    private          bool            stopped;

    // Serialisation of transitions
    private readonly object transitionSync = new();
    private          Task   transitionChain = Task.CompletedTask;

    /// <inheritdoc />
    public IObservable<TState> ObservableStates => currentStateSubject.AsObservable();

    /// <summary>
    ///     A synchronization primitive that is set whenever a transition fully completes.
    /// </summary>
    private ManualResetEvent StateChanged { get; } = new(false);

    private CancellationTokenSource stateCancellation = new();

    /// <inheritdoc />
    public TState? CurrentState { get; private set; }

    private Task? currentStateTask;

    /// <summary>
    ///     Creates a new finite state machine.
    /// </summary>
    /// <param name="log">Optional logger instance. When omitted, a degenerate (no-op) logger is used.</param>
    public FiniteStateMachine(ILog? log = null) => this.log = log ?? new DegenerateLoggerService();

    /// <summary>
    ///     Convenience property returning CurrentState.DisplayName or empty when not active.
    /// </summary>
    public string CurrentStateName => CurrentState?.DisplayName ?? string.Empty;

    /// <summary>
    ///     Blocks the calling thread until the provided <paramref name="predicate"/> evaluates to true
    ///     against the current state, or the <paramref name="timeout"/> elapses.
    /// </summary>
    /// <param name="predicate">Condition that must become true for the wait to complete.</param>
    /// <param name="timeout">Maximum time to wait before throwing <see cref="TimeoutException"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="predicate"/> is null.</exception>
    /// <exception cref="TimeoutException">Thrown if the timeout elapses before the condition becomes true.</exception>
    public void WaitUntil(Func<TState?, bool> predicate, TimeSpan timeout)
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));
        // Fast-path: if already satisfied, return immediately.
        if (predicate(CurrentState)) return;

        var start = DateTime.UtcNow;
        while (true)
        {
            var elapsed = DateTime.UtcNow - start;
            var remaining = timeout - elapsed;
            if (remaining <= TimeSpan.Zero) break;
            if (!StateChanged.WaitOne(remaining)) break;
            if (predicate(CurrentState)) return;
        }
        throw new TimeoutException("Timed out waiting for condition in WaitUntil");
    }

    /// <summary>
    ///     Blocks until the specified <paramref name="expected"/> state instance becomes current, or until <paramref name="timeout"/>.
    /// </summary>
    /// <param name="expected">The expected state instance.</param>
    /// <param name="timeout">Maximum time to wait.</param>
    public void WaitUntil(TState expected, TimeSpan timeout)
    {
        if (expected is null) throw new ArgumentNullException(nameof(expected));
        WaitUntil(s => ReferenceEquals(s, expected), timeout);
    }

    /// <inheritdoc />
    public void TransitionTo(TState newState)
    {
        if (newState is null) return;
        if (stopped) return;

        log.Info()
            .Message("State transition {stateType} => {targetState}", typeof(TState).Name, newState.DisplayName)
            .Property(nameof(CurrentState), CurrentState)
            .Property(nameof(newState), newState)
            .Write();

        lock (transitionSync)
        {
            try
            {
                StateChanged.Reset();
                // Append this transition to the serial chain.
                var next = newState;
                transitionChain = transitionChain.ContinueWith(_ =>
                {
                    StartNextStateWhenCurrentStateCompletes(next);
                }, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                log.Error().Exception(ex).Message("Failed to enqueue state transition: {message}", ex.Message).Write();
            }
        }
    }

    private void StartNextStateWhenCurrentStateCompletes(TState nextState)
    {
        if (nextState is null) return;

        // Cancel the current state's run loop before waiting for it to finish
        CancelCurrentState();

        try
        {
            currentStateTask?.Wait(); // Wait for any current state run task to fully complete.
        }
        catch (OperationCanceledException ex)
        {
            log.Warn().Exception(ex).Message("Operation cancelled waiting for state run task to complete").Write();
        }
        catch (AggregateException agex)
        {
            foreach (var ex in agex.InnerExceptions)
                log.Error().Exception(ex).Message("Unhandled exception from previous state RunAsync: {message}", ex.Message).Write();
        }
        catch (Exception ex)
        {
            log.Error().Exception(ex).Message("Unhandled exception from previous state RunAsync: {message}", ex.Message).Write();
        }

        try
        {
            CurrentState?.OnExit();
        }
        catch (Exception ex)
        {
            log.Error().Exception(ex).Message("Exception in OnExit: {message}", ex.Message).Write();
        }

        if (stopped)
        {
            // Do not allow the new state to start.
            CurrentState = null;
            currentStateTask = null;
            StateChanged.Set();
            return;
        }

        try
        {
            CurrentState = nextState;
            currentStateSubject.OnNext(nextState);
            try
            {
                CurrentState?.OnEnter();
            }
            catch (Exception ex)
            {
                log.Error().Exception(ex).Message("Exception in OnEnter: {message}", ex.Message).Write();
            }
            // Track the actual run task from the state using a fresh cancellation token
            try
            {
                currentStateTask = CurrentState?.RunAsync(stateCancellation.Token);
            }
            catch (Exception ex)
            {
                // If RunAsync throws synchronously, log and clear the task
                log.Error().Exception(ex).Message("RunAsync threw synchronously: {message}", ex.Message).Write();
                currentStateTask = null;
            }
        }
        finally
        {
            StateChanged.Set(); // Signal state change completed.
        }
    }

    /// <inheritdoc />
    public void StartStateMachine(TState initialState)
    {
        if (CurrentState is not null)
            throw new InvalidOperationException("Cannot start the state machine that is already started. Call StopStateMachine() first.");
        if (initialState is null) throw new ArgumentNullException(nameof(initialState));

        stopped = false;
        TransitionTo(initialState);
    }

    /// <inheritdoc />
    public void StopStateMachine()
    {
        Task prior;
        lock (transitionSync)
        {
            stopped = true; // Prevent any further state transitions.
            prior = transitionChain; // capture any enqueued transitions
            StateChanged.Reset();
        }

        try
        {
            try
            {
                prior?.Wait(); // Wait for any in-flight transition sequencing
            }
            catch (AggregateException agex)
            {
                foreach (var ex in agex.InnerExceptions)
                    log.Error().Exception(ex).Message("Unhandled exception in transition chain: {message}", ex.Message).Write();
            }

            CancelCurrentState();

            try
            {
                currentStateTask?.Wait(); // Synchronous wait for end of the last state
            }
            catch (OperationCanceledException ex)
            {
                log.Warn().Exception(ex).Message("Operation cancelled waiting for state run task to complete during Stop").Write();
            }
            catch (AggregateException agex)
            {
                foreach (var ex in agex.InnerExceptions)
                    log.Error().Exception(ex).Message("Unhandled exception from state RunAsync during Stop: {message}", ex.Message).Write();
            }
            catch (Exception ex)
            {
                log.Error().Exception(ex).Message("Unhandled exception waiting for state RunAsync during Stop: {message}", ex.Message).Write();
            }

            try
            {
                CurrentState?.OnExit();
            }
            catch (Exception ex)
            {
                log.Error().Exception(ex).Message("Exception in OnExit during Stop: {message}", ex.Message).Write();
            }
        }
        finally
        {
            currentStateTask = null;
            CurrentState = null;
            // Complete the current stream and replace the subject so new subscribers can attach for the next run.
            var oldSubject = Interlocked.Exchange(ref currentStateSubject, new Subject<TState>());
            oldSubject.OnCompleted();
            oldSubject.Dispose();
            StateChanged.Set();
        }
    }

    /// <summary>
    ///     Cancels the current state's run task and resets the cancellation token source.
    /// </summary>
    private void CancelCurrentState()
    {
        var old = Interlocked.Exchange(ref stateCancellation, new CancellationTokenSource());
        try
        {
            try { old.Cancel(); }
            catch (ObjectDisposedException) { /* already disposed */ }
        }
        finally
        {
            old.Dispose();
        }
    }
}
