#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Core.StateMachine;

namespace TA.Utils.Specifications.StateMachine;

[Subject(typeof(FiniteStateMachine<>))]
public class when_transitioning_between_two_states
{
    static FiniteStateMachine<TestState> fsm;
    static TestState                     first;
    static TestState                     second;
    static List<string>                  events;
    static List<TestState>               observed;
    static IDisposable                   subscription;

    Establish context = () =>
    {
        events = new List<string>();
        observed = new List<TestState>();
        fsm = new FiniteStateMachine<TestState>(new DegenerateLoggerService());
        first = new TestState("First", events);
        second = new TestState("Second", events);
        subscription = fsm.ObservableStates.Subscribe(observed.Add);
    };

    Because of = () =>
    {
        fsm.StartStateMachine(first);
        fsm.StateChanged.WaitOne(); // initial activation complete
        fsm.TransitionTo(second);
        fsm.StateChanged.WaitOne(); // second activation complete
    };

    Cleanup cleanup = () =>
    {
        subscription?.Dispose();
        fsm.StopStateMachine();
    };

    It should_call_exit_and_enter_hooks = () =>
        (events.Contains("First:Exit") && events.Contains("Second:Enter")).ShouldBeTrue();

    It should_set_CurrentState_to_the_second_state = () => fsm.CurrentState.DisplayName.ShouldEqual(second.DisplayName);

    It should_emit_first_state_on_the_observable_sequence = () =>
        observed.Any(s => s.DisplayName == "First").ShouldBeTrue();

    It should_emit_second_state_on_the_observable_sequence = () =>
        observed.Any(s => s.DisplayName == "Second").ShouldBeTrue();
}

[Subject(typeof(FiniteStateMachine<>))]
public class when_stopping_the_state_machine
{
        static FiniteStateMachine<TestState> fsm;
        static TestState                     state;
        static List<string>                  events;
        static bool                          completed;
        static IDisposable                   subscription;

        Establish context = () =>
        {
            events = new List<string>();
            fsm    = new FiniteStateMachine<TestState>(new DegenerateLoggerService());
            state  = new TestState("Running", events) { SimulateWork = true };
            completed = false;
            subscription = fsm.ObservableStates.Subscribe(_ => { }, () => completed = true);
        };

        Because of = () =>
        {
            fsm.StartStateMachine(state);
            // Give the RunAsync loop a moment
            Thread.Sleep(50);
            fsm.StopStateMachine();
        };

        Cleanup cleanup = () => subscription?.Dispose();

        It should_cancel_the_running_state_and_clear_CurrentState = () => fsm.CurrentState.ShouldBeNull();

        It should_complete_the_observable_sequence = () => completed.ShouldBeTrue();

        It should_have_called_OnExit = () => events.ShouldContain("Running:Exit");
    }

    internal class TestState : IState
    {
        readonly IList<string> _events;
        public   bool          SimulateWork { get; set; }

        public TestState(string name, IList<string> events)
        {
            DisplayName = name;
            _events     = events;
        }

        public string DisplayName { get; }

        public void OnEnter() => _events.Add(DisplayName + ":Enter");
        public void OnExit()  => _events.Add(DisplayName + ":Exit");

        public async Task RunAsync(CancellationToken cancelOnExit)
        {
            _events.Add(DisplayName + ":Run");
            if (!SimulateWork) return;
            try
            {
                while (!cancelOnExit.IsCancellationRequested)
                    await Task.Delay(10, cancelOnExit).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // expected during stop/transition
            }
        }
    }
