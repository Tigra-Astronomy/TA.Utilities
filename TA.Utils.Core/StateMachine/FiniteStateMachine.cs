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
public class FiniteStateMachine<TState> : IFiniteStateMachine<TState> where TState : class, IState
{
    private readonly ILog            log;
    private          Subject<TState> currentStateSubject = new();
    private          bool            stopped;

    /// <inheritdoc />
    public IObservable<TState> ObservableStates => currentStateSubject.AsObservable();

    /// <summary>
    ///     A synchronization primitive that is set whenever a transition fully completes.
    /// </summary>
    public ManualResetEvent StateChanged { get; } = new(false);

    private CancellationTokenSource stateCancellation = new();

    /// <inheritdoc />
    public TState? CurrentState { get; private set; }

    protected Task? currentStateTask;

    public FiniteStateMachine(ILog? log = null) => this.log = log ?? new DegenerateLoggerService();

    /// <summary>
    ///     Convenience property returning CurrentState.DisplayName or empty when not active.
    /// </summary>
    public string CurrentStateName => CurrentState?.DisplayName ?? string.Empty;

    /// <inheritdoc />
    public virtual void TransitionTo(TState newState)
    {
        log.Info()
            .Message("State transition {stateType} => {targetState}", typeof(TState).Name, newState.DisplayName)
            .Property(nameof(CurrentState), CurrentState)
            .Property(nameof(newState), newState)
            .Write();

        // Phase 1 - cancel the current state task.
        CancelCurrentState();

        // Phase 2 - wait for the state to fully exit.
        try
        {
            StateChanged.Reset();
            Task.Run(() => StartNextStateWhenCurrentStateCompletes(newState)).ContinueOnAnyThread();
        }
        catch
        {
            // Non-fatal path: transition should not throw. Any errors are logged inside the async path.
        }
    }

    private void StartNextStateWhenCurrentStateCompletes(TState nextState)
    {
        if (nextState is null) return;
        try
        {
            currentStateTask?.Wait(); // Wait for any current state run task to fully complete.
        }
        catch (OperationCanceledException ex)
        {
            log.Warn().Exception(ex).Message("Operation cancelled waiting for state run task to complete").Write();
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
            CurrentState?.OnEnter();
            currentStateTask = Task.Run(() => CurrentState?.RunAsync(stateCancellation.Token).ContinueOnAnyThread());
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
    public virtual void StopStateMachine()
    {
        stopped = true; // Prevent any further state transitions.
        try
        {
            CancelCurrentState();
            currentStateTask?.Wait(); // Synchronous wait for end of the last state
            CurrentState?.OnExit();
        }
        finally
        {
            currentStateTask = null;
            CurrentState = null;
            // Complete the current stream and replace the subject so new subscribers can attach for the next run.
            var oldSubject = Interlocked.Exchange(ref currentStateSubject, new Subject<TState>());
            oldSubject.OnCompleted();
            oldSubject.Dispose();
        }
    }

    /// <summary>
    ///     Cancels the current state's run task and resets the cancellation token source.
    /// </summary>
    private void CancelCurrentState()
    {
        try
        {
            stateCancellation.Cancel(); // cancel the current state run task.
        }
        catch (TaskCanceledException)
        {
            // Already cancelled.
        }
        finally
        {
            stateCancellation = new CancellationTokenSource();
        }
    }
}
