#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Core.StateMachine;

namespace TA.Utils.Specifications.StateMachine
{
    [Subject(typeof(FiniteStateMachine<>), "Concurrency")]
    public class when_multiple_transitions_are_requested_concurrently
    {
        static FiniteStateMachine<ConcurrencyState> fsm;
        static List<ConcurrencyState> states;
        static int total;

        Establish context = () =>
        {
            fsm = new FiniteStateMachine<ConcurrencyState>(Support.TestLog.Log);
            states = Enumerable.Range(1, 10).Select(i => new ConcurrencyState($"S{i}")).ToList();
            total = states.Count;
        };

        Because of = () =>
        {
            fsm.StartStateMachine(states[0]);
            var tasks = new List<Task>();
            foreach (var s in states.Skip(1))
            {
                tasks.Add(Task.Run(() => fsm.TransitionTo(s)));
            }
            Task.WhenAll(tasks).Wait();
            // Ensure the last desired transition is requested deterministically
            fsm.TransitionTo(states.Last());
            Thread.Sleep(10);
            fsm.WaitUntil(s => s != null && s.DisplayName == states.Last().DisplayName, TimeSpan.FromSeconds(5));
        };

        Cleanup cleanup = () => fsm.StopStateMachine();

        It should_end_in_the_last_requested_state = () => fsm.CurrentStateName.ShouldEqual(states.Last().DisplayName);
    }

    internal class ConcurrencyState : IState
    {
        public string DisplayName { get; }
        public bool SimulateWork { get; set; }
        public ConcurrencyState(string name) => DisplayName = name;
        public void OnEnter() { }
        public void OnExit() { }
        public async Task RunAsync(CancellationToken cancelOnExit)
        {
            // Minimal work to allow interleaving
            if (!SimulateWork) return;
            try { await Task.Delay(10, cancelOnExit).ConfigureAwait(false); }
            catch (OperationCanceledException) { }
        }
    }
}