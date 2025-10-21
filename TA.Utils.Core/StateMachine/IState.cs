using System.Threading;
using System.Threading.Tasks;

namespace TA.Utils.Core.StateMachine;

/// <summary>
///     Marker interface for all state classes that participate in a finite state machine.
///     States expose a display name, enter/exit hooks and an asynchronous run method that accepts
///     a cancellation token which is cancelled when the state exits.
/// </summary>
public interface IState
{
    /// <summary>
    ///     A human-readable display name for the state.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    ///     Called when the state becomes active. Invoked during a transition after the previous state's OnExit has completed.
    /// </summary>
    void OnEnter();

    /// <summary>
    ///     Called when the state is about to be exited during a transition.
    ///     Use this to perform any necessary cleanup before deactivation.
    /// </summary>
    void OnExit();

    /// <summary>
    ///     Executes the main logic of the state asynchronously while the state is active.
    ///     Implementations should observe <paramref name="cancelOnExit"/> and exit promptly when cancelled.
    /// </summary>
    Task RunAsync(CancellationToken cancelOnExit);
}
