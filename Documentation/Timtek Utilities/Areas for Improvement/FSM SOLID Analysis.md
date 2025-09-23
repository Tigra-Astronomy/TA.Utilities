# FSM SOLID analysis and recommendations

This document reviews the finite state machine (FSM) design in TA.Utils.Core.StateMachine with respect to the SOLID principles, and proposes pragmatic improvements. It assumes familiarity with [[Core/Finite State Machine|Finite State Machine]] as implemented in this repository.

Summary
- The design is clean and pragmatic: small, composable abstractions, with a clear split between "state" and "machine" and sensible use of observability.
- The main areas to tighten are: concurrency safety and visibility across threads, transition error handling (especially exceptions from a state’s RunAsync), resource disposal, and API shape.

Strengths (per SOLID)
- Single Responsibility
  - `IState` encapsulates a state’s lifecycle: enter, run, exit, with a clear cancellation boundary.
  - `IFiniteStateMachine\<TState\>` focuses on orchestration and observation (start/stop/transition + observability).
  - `FiniteStateMachine\<TState\>` centralises transition sequencing: cancel current, wait, exit, enter, run next.
- Open–Closed
  - Virtual methods permit extension via subclassing without modifying core code.
  - Logging depends on `ILog` and remains pluggable.
- Liskov Substitution
  - The `IState` contract is straightforward; implementations can substitute so long as they honour cancellation and lifecycle hooks.
- Interface Segregation
  - `IState` and `IFiniteStateMachine\<TState\>` are small and coherent.
- Dependency Inversion
  - Depends on abstractions (`IState`, `ILog`, `IObservable`) rather than concrete frameworks.

Weaknesses and risks
- Scope creep in the core class (SRP pressure)
  - `FiniteStateMachine\<TState\>` handles orchestration, concurrency/cancellation, signalling, event publication and logging. This increases the reasons for the class to change.
- Limited extension points (OCP pressure)
  - No injection point for custom scheduling, transition guards, timeout policies, or exception handling strategies. Subclassing risks invariant breakage.
- Invariants are not formalised (LSP risk)
  - Transition semantics are not strictly documented; overrides could violate cancellation/ordering guarantees. A public `ManualResetEvent` leaks an implementation detail a subclass might change.
- Interface details
  - `DisplayName` on `IState` is UI‑oriented; some domains may not wish to couple display concerns into the core interface.
- API leakage and synchronisation primitive
  - Exposing `ManualResetEvent` couples consumers to a particular synchronisation model; an awaitable API would be a better abstraction.
- Concurrency/thread safety
  - No explicit locking around `CurrentState`/`currentStateTask`/`stateCancellation`. Concurrent `TransitionTo` calls can interleave.
  - `Subject<T>` emissions occur on background threads; consumers may incorrectly assume a context.
- Error handling of state failures
  - Only `OperationCanceledException` is caught when waiting for the prior state; unexpected exceptions from `RunAsync` could become unobserved or skip completion signalling.
- Resource disposal
  - The previous `CancellationTokenSource` is replaced but never disposed; repeated transitions can leak handles.
- Logging responsibility
  - Logging inside the core class conflicts with the repository preference for a logging decorator.
- Test synchronisation ergonomics
  - Exposing `ManualResetEvent` invites blocking APIs; async/await would be more idiomatic and composable.

Recommendations
1) Serialise transitions and protect shared state
- Use a private lock or a serial queue to ensure `TransitionTo` is processed one at a time and to protect `CurrentState`, `currentStateTask`, `stateCancellation` and the `stopped` flag.

2) Dispose the previous `CancellationTokenSource`
- Swap, cancel, and dispose the old CTS to avoid leaking OS handles.

```csharp
// inside CancelCurrentState()
var old = Interlocked.Exchange(ref stateCancellation, new CancellationTokenSource());
try { old.Cancel(); } catch (ObjectDisposedException) { }
old.Dispose();
```

3) Catch and log all exceptions from state tasks and waits
- Ensure the transition path cannot fault and skip completion signalling; log unexpected exceptions at Error and proceed.

```csharp
try
{
    currentStateTask?.Wait();
}
catch (OperationCanceledException ex)
{
    log.Warn().Exception(ex).Message("State cancelled while awaiting completion").Write();
}
catch (Exception ex)
{
    log.Error().Exception(ex).Message("Unhandled exception from state RunAsync: {message}", ex.Message).Write();
}
```

4) Replace `ManualResetEvent` with an awaitable API
- Add `TransitionAsync(next, ct)` that completes when the new state is fully activated (after `OnEnter` and `RunAsync` is scheduled). Retain the event internally for tests if required.

```csharp
public Task TransitionAsync(TState next, CancellationToken ct = default)
{
    var tcs = new TaskCompletionSource();
    TransitionTo(next);
    Task.Run(() =>
    {
        StateChanged.WaitOne();
        tcs.TrySetResult();
    }, ct);
    return tcs.Task;
}
```

5) Introduce extension points (favour composition)
- `ITransitionScheduler` – how transitions are queued/executed.
- `IExceptionPolicy` – how to deal with exceptions from `OnExit`/`RunAsync` (log, rethrow, transition to fallback).
- `ITransitionGuard` – optional policy to allow/deny a transition.

6) Seal the core class and add a logging decorator
- Seal `FiniteStateMachine\<TState\>` to protect invariants.
- Move logging into a `FiniteStateMachineLoggingDecorator\<TState\>` that wraps `IFiniteStateMachine\<TState\>` and logs around calls.

7) Provide async‑first APIs
- Add `StartAsync`, `TransitionAsync`, `StopAsync` (with optional timeouts). Keep synchronous counterparts for convenience, but prefer async in usage and examples.

8) Document or parameterise the observer context
- Clearly document that `ObservableStates` emits on background threads, or accept an `IScheduler` to marshal notifications.

9) Optional: make state publication replayable
- Provide a way for late subscribers to receive the current state immediately (e.g., cache last activation and emit it on subscription).

10) Consider decoupling display concerns
- Make `DisplayName` optional or move UI‑oriented concerns to a separate optional interface.

11) Add transition timeouts and fallbacks
- Ensure a misbehaving state that ignores cancellation cannot block shutdown indefinitely; log at Error and move to a safe state after a timeout.

12) Extend the test suite for race and failure modes
- Add specs for rapid sequential transitions, Stop during a transition, exceptions thrown by `RunAsync`, and `OnExit` throwing. Assert that invariants hold and notifications always complete.

Expected benefits
- Stronger invariants and simpler reasoning about concurrency.
- Cleaner API surface (async/await rather than synchronisation primitives).
- Alignment with repository preferences (sealed by default; logging via decorator).
- Improved testability and resilience, with pluggable policies.
- Fewer leaks and unobserved exceptions; more graceful behaviour under failure.

Related topics
- [[Core/Finite State Machine|Finite State Machine]]
- [[Async Helpers]]
- [[Diagnostics and Logging]]
