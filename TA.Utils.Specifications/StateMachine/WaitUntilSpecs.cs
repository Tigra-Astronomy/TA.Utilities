#nullable disable
using System;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Core.StateMachine;

namespace TA.Utils.Specifications.StateMachine
{
    [Subject(typeof(FiniteStateMachine<>), "WaitUntil")]
    public class when_waiting_until_a_state_is_activated
    {
        static FiniteStateMachine<WUState> fsm;
        static WUState first;
        static WUState second;

        Establish context = () =>
        {
            fsm = new FiniteStateMachine<WUState>(Support.TestLog.Log);
            first = new WUState("First");
            second = new WUState("Second");
        };

        Because of = () =>
        {
            fsm.StartStateMachine(first);
            // Schedule a delayed transition to the second state
            Task.Run(async () => { await Task.Delay(50).ConfigureAwait(false); fsm.TransitionTo(second); });
            FsmSpecHelpers.WaitUntil(fsm, "Second", TimeSpan.FromSeconds(2));
        };

        Cleanup cleanup = () => fsm.StopStateMachine();

        It should_have_transitioned_to_the_expected_state = () => fsm.CurrentState.DisplayName.ShouldEqual("Second");
    }

    [Subject(typeof(FiniteStateMachine<>), "WaitUntil")]
    public class when_waiting_until_a_condition_that_never_becomes_true
    {
        static FiniteStateMachine<WUState> fsm;
        static Exception exception;

        Establish context = () =>
        {
            fsm = new FiniteStateMachine<WUState>(Support.TestLog.Log);
            fsm.StartStateMachine(new WUState("Alpha"));
        };

        Because of = () => exception = Catch.Exception(() => FsmSpecHelpers.WaitUntil(fsm, "Beta", TimeSpan.FromMilliseconds(100)));

        Cleanup cleanup = () => fsm.StopStateMachine();

        It should_timeout = () => exception.ShouldBeOfExactType<TimeoutException>();
    }

    internal class WUState : IState
    {
        public string DisplayName { get; }
        public WUState(string name) => DisplayName = name;
        public void OnEnter() { }
        public void OnExit() { }
        public Task RunAsync(CancellationToken cancelOnExit) => Task.CompletedTask;
    }
}