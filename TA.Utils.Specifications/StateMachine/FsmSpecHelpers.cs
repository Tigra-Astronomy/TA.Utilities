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
            // Use the Observable-based WaitUntil to avoid relying on any wait handles
            WaitUntil(fsm, s => s != null, timeout);
        }

        public static void WaitForState<TState>(FiniteStateMachine<TState> fsm, string expectedDisplayName, TimeSpan timeout)
            where TState : class, IState
        {
            // Use the Observable-based WaitUntil to avoid relying on any wait handles
            WaitUntil(fsm, s => s != null && s.DisplayName == expectedDisplayName, timeout);
            fsm.CurrentState.ShouldNotBeNull();
            fsm.CurrentState!.DisplayName.ShouldEqual(expectedDisplayName);
        }

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
