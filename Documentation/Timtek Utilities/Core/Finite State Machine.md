# Finite State Machine (FSM)

This package provides a lightweight, composable finite state machine for orchestrating asynchronous workflows. It is designed to be simple enough for embedded or real‑time adjacent scenarios while remaining idiomatic for .NET applications.

- Interfaces: `IState`, `IFiniteStateMachine<TState>`
- Implementation: `FiniteStateMachine<TState>`
- Cross‑cutting: integrates cleanly with [[Diagnostics and Logging]] and [[Async Helpers]]

## Why use an FSM?

An FSM is a robust way to model systems that have a small number of modes of operation with well‑defined rules for moving between them. Typical examples include:
- Device and protocol controllers (Idle → Connecting → Connected → Error → Reconnect)
- UI or workflow wizards (Start → Collecting Input → Validating → Completed/Failed)
- Stream/transport handlers (Listening → Handshaking → Streaming → Closing)

Benefits
- Deterministic behaviour: states explicitly define entry, run, and exit behaviour.
- Clear separation of concerns: each state has a single responsibility.
- Testability: transitions and hooks are easy to verify (see the included specifications).
- Safe shutdown/transition: the framework cancels the running state and waits for it to exit before activating the next state.
- Observability: transitions are published via `IObservable` so other components can react.

Real‑time considerations
- Bounded transitions: when `TransitionTo()` is called, the framework cancels the old state and only starts the new state after the old state’s `RunAsync` completes and `OnExit` has run. This prevents overlap and race conditions.
- No UI thread dependence: states run on background tasks (`Task.Run`) and use [[Async Helpers|ContinueOnAnyThread]] semantics, avoiding message‑pump deadlocks.
- Cooperative cancellation: each state’s `RunAsync(CancellationToken)` cooperates to exit promptly on cancellation, allowing responsive hand‑overs.

## Programming model

Implement the `IState` interface for each state and drive transitions through `IFiniteStateMachine<TState>`.

```csharp
public interface IState
{
    string DisplayName { get; }
    void OnEnter();
    void OnExit();
    Task RunAsync(CancellationToken cancelOnExit);
}
```

```csharp
public interface IFiniteStateMachine<TState> where TState : class, IState
{
    void TransitionTo(TState newState);
    void StartStateMachine(TState initialState);
    void StopStateMachine();

    IObservable<TState> ObservableStates { get; }
    TState? CurrentState { get; }
}
```

Key runtime guarantees
- Cancellation first: `TransitionTo(next)` cancels the current state’s token before waiting.
- Orderly exit/enter: after the current state’s run task completes, `OnExit()` is called, then `OnEnter()` for the next state, then `RunAsync()` for the next state is started.
- Blocking wait helper: `WaitUntil(predicate, timeout)` enables hosts/tests to synchronously wait until a condition on `CurrentState` is true (e.g., a specific state is activated).
- Observable transitions: each activation is pushed to `ObservableStates`.

## Minimal example

Below is a simplified controller with three states: Idle, Connecting, Connected. The Connecting state simulates work and then transitions to Connected; Connected waits until cancelled.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Core.StateMachine;

public class IdleState : IState
{
    public string DisplayName => "Idle";
    private readonly Action<IState> go;

    public IdleState(Action<IState> transition)
        => go = transition;

    public void OnEnter() { /* noop */ }
    public void OnExit()  { /* noop */ }

    public Task RunAsync(CancellationToken cancel)
    {
        // Immediately move to Connecting
        go(new ConnectingState(go));
        return Task.CompletedTask;
    }
}

public class ConnectingState : IState
{
    public string DisplayName => "Connecting";
    private readonly Action<IState> go;

    public ConnectingState(Action<IState> transition) => go = transition;

    public void OnEnter() { /* start timers, allocate resources */ }
    public void OnExit()  { /* release temporary resources */ }

    public async Task RunAsync(CancellationToken cancel)
    {
        try
        {
            // Simulate connection establishment
            await Task.Delay(500, cancel).ConfigureAwait(false);
            go(new ConnectedState());
        }
        catch (OperationCanceledException)
        {
            // Transition or stop was requested
        }
    }
}

public class ConnectedState : IState
{
    public string DisplayName => "Connected";
    public void OnEnter() { /* mark online */ }
    public void OnExit()  { /* mark offline */ }

    public async Task RunAsync(CancellationToken cancel)
    {
        // Run until cancelled by a transition or StopStateMachine()
        try
        {
            while (!cancel.IsCancellationRequested)
                await Task.Delay(100, cancel).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
    }
}

// Host
var log = new DegenerateLoggerService();
var fsm = new FiniteStateMachine<IState>(log);

// Helper to allow states to request transitions without holding fsm directly.
void Go(IState next) => fsm.TransitionTo(next);

var idle = new IdleState(Go);
fsm.StartStateMachine(idle);
```

Notes
- States can trigger transitions by holding an `Action<IState>` (or a reference to the FSM) passed via constructor DI.
- Keep `OnEnter`/`OnExit` fast; perform long work inside `RunAsync`.
- Always observe the `cancelOnExit` token and exit promptly on cancellation.

## Observing transitions

Other components can subscribe to state activations.

```csharp
var subscription = fsm.ObservableStates.Subscribe(s =>
{
    Console.WriteLine($"Now in state: {s.DisplayName}");
});
```

In tests or synchronous orchestration, you can wait for a transition to complete using the blocking helper:

```csharp
fsm.TransitionTo(new ConnectedState());
fsm.WaitUntil(s => s != null && s.DisplayName == "Connected", TimeSpan.FromSeconds(2));
```

## Synchronisation helpers

- Concrete class: `FiniteStateMachine<TState>` exposes `WaitUntil(predicate, timeout)` and `WaitUntil(expected, timeout)` for blocking waits in tests or host code.
- Interface extension: for general `IFiniteStateMachine<TState>` usage (e.g., when working against the abstraction), use the provided extension methods in `TA.Utils.Core.StateMachine.FiniteStateMachineExtensions`:

```csharp
IFiniteStateMachine<IState> fsm = new FiniteStateMachine<IState>(log);
fsm.StartStateMachine(new IdleState(Go));
// Extension method: blocks until Connected becomes current or the timeout elapses
fsm.WaitUntil(s => s != null && s.DisplayName == "Connected", TimeSpan.FromSeconds(2));
```

Notes
- These helpers block the calling thread and are not suitable for UI threads.
- Prefer async/reactive orchestration in production code; use blocking waits primarily in tests or simple host loops.

## Error handling and logging

- Exceptions in `RunAsync` should generally be caught inside the state and handled by transitioning to an error/recovery state.
- Exceptions in `OnExit` are caught by the FSM and logged, to avoid derailing the transition.
- Use [[Diagnostics and Logging]] to attach per‑state context (e.g., logger name, correlation ids).

## Timeouts, timers, and external events

- Use `Task.WhenAny`, `Task.Delay(timeout, cancel)` or external event sources to decide when to transition.
- For periodic work, loop inside `RunAsync` with `await Task.Delay(...)` observing `cancelOnExit`.
- For external events (I/O, message bus), subscribe in `OnEnter` and unsubscribe in `OnExit`.

Example: transition on timeout or signal, whichever comes first.

```csharp
public class WaitingForSignal : IState
{
    public string DisplayName => "WaitingForSignal";
    private readonly Action<IState> go;
    private readonly TaskCompletionSource<bool> signal = new();

    public WaitingForSignal(Action<IState> transition) => go = transition;
    public void OnEnter() { /* wire external event => signal.TrySetResult(true) */ }
    public void OnExit()  { /* unwire */ }

    public async Task RunAsync(CancellationToken cancel)
    {
        var timeout = Task.Delay(TimeSpan.FromSeconds(2), cancel);
        var completed = await Task.WhenAny(signal.Task, timeout).ConfigureAwait(false);
        if (completed == signal.Task) go(new ConnectedState());
        else go(new IdleState(go));
    }
}
```

## Best practices

- Keep state objects small, immutable where possible, and focused on one responsibility (SRP).
- Prefer composition: delegate specialized work (I/O, parsing) to collaborators injected into the state.
- Avoid blocking waits inside `RunAsync`—always prefer async and honor cancellation.
- Design explicit Error/Recovering states rather than mixing error handling into every state.
- Publish enough metadata via logging to trace complex sequences (see [[Diagnostics and Logging]] and correlation guidance).

## API reference (summary)

- `IState`
  - `DisplayName` – human‑readable identifier for diagnostics/UI
  - `OnEnter()` – called after previous state exits, before `RunAsync`
  - `OnExit()` – called during transition out, after `RunAsync` completes
  - `RunAsync(CancellationToken)` – the main loop/logic; must observe cancellation
- `IFiniteStateMachine<TState>`
  - `StartStateMachine(TState initial)` – activates the initial state
  - `TransitionTo(TState next)` – cancels current state, waits, then activates `next`
  - `StopStateMachine()` – cancels and exits, completes the observable, resets to inactive
  - `ObservableStates` – `IObservable<TState>` emitting each activated state
  - `CurrentState` – currently active state or null when stopped

## See also
- [[Async Helpers]] – avoiding context capture and UI deadlocks
- [[Diagnostics and Logging]] – adding semantic logging to states and transitions
- [[Versioning]] – how the repository versions assemblies and packages
