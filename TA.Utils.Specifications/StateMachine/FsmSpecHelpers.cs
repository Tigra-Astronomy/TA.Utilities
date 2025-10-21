using System;
using System.Threading;
using Machine.Specifications;
using TA.Utils.Core.StateMachine;

namespace TA.Utils.Specifications.StateMachine
{
    internal static class FsmSpecHelpers
    {
        public static void WaitForActivation<TState>(FiniteStateMachine<TState> fsm, TimeSpan timeout)
            where TState : class, IState
        {
            // Prefer the FSM's event-based WaitUntil for deterministic behaviour across TFMs
            WaitUntil(fsm, s => s != null, timeout);
        }

        public static void WaitForState<TState>(FiniteStateMachine<TState> fsm, string expectedDisplayName, TimeSpan timeout)
            where TState : class, IState
        {
            // Prefer the FSM's event-based WaitUntil for deterministic behaviour across TFMs
            WaitUntil(fsm, s => s != null && s.DisplayName == expectedDisplayName, timeout);
            fsm.CurrentState.ShouldNotBeNull();
            fsm.CurrentState!.DisplayName.ShouldEqual(expectedDisplayName);
        }

        // Deterministic path: delegate to the FSM's own WaitUntil (uses a ManualResetEvent internally)
        public static void WaitUntil<TState>(FiniteStateMachine<TState> fsm, Func<TState?, bool> predicate, TimeSpan timeout)
            where TState : class, IState
            => fsm.WaitUntil(predicate, timeout);

        public static void WaitUntil<TState>(FiniteStateMachine<TState> fsm, string expectedDisplayName, TimeSpan timeout)
            where TState : class, IState
            => fsm.WaitUntil(s => s != null && s.DisplayName == expectedDisplayName, timeout);

        // Legacy observable-based helper (kept for completeness and potential reuse elsewhere)
        public static void WaitUntil<TState>(IFiniteStateMachine<TState> fsm, Func<TState?, bool> predicate, TimeSpan timeout)
            where TState : class, IState
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (predicate(fsm.CurrentState)) return;
            using var gate = new ManualResetEventSlim(false);
            using var sub = fsm.ObservableStates.Subscribe(state =>
            {
                if (predicate(state)) gate.Set();
            });
            if (!gate.Wait(timeout)) throw new TimeoutException("Timed out waiting for condition in WaitUntil");
        }

        public static void WaitUntil<TState>(IFiniteStateMachine<TState> fsm, string expectedDisplayName, TimeSpan timeout)
            where TState : class, IState
            => WaitUntil(fsm, s => s != null && s.DisplayName == expectedDisplayName, timeout);
    }
}
