using System;

namespace TA.Utils.Core.StateMachine;

/// <summary>
///     Represents a finite state machine (FSM) that manages transitions between states of type <typeparamref name="TState"/>.
/// </summary>
/// <typeparam name="TState">The type of the state managed by the finite state machine. Must implement <see cref="IState"/>.</typeparam>
public interface IFiniteStateMachine<TState> where TState : class, IState
{
    /// <summary>
    ///     Transition the finite state machine to a new state.
    /// </summary>
    /// <param name="newState">The new state to transition to.</param>
    void TransitionTo(TState newState);

    /// <summary>
    ///     Start the finite state machine with the specified initial state.
    /// </summary>
    /// <param name="initialState">The initial state to activate.</param>
    void StartStateMachine(TState initialState);

    /// <summary>
    ///     Stop the finite state machine and transition it to an inactive state.
    /// </summary>
    void StopStateMachine();

    /// <summary>
    ///     Observable sequence of states representing transitions.
    /// </summary>
    IObservable<TState> ObservableStates { get; }

    /// <summary>
    ///     The current state of the finite state machine, or null if inactive.
    /// </summary>
    TState? CurrentState { get; }
}
