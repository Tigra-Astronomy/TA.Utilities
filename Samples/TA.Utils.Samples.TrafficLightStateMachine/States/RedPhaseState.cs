using System;
using System.Threading;
using System.Threading.Tasks;
using TA.Utils.Core.StateMachine;
using TA.Utils.Samples.TrafficLightStateMachine.Services;
using TA.Utils.Samples.TrafficLightStateMachine.ViewModels;

namespace TA.Utils.Samples.TrafficLightStateMachine.States;

public sealed class RedPhaseState : IState
{
    public string DisplayName => "Red";

    private readonly Action<IState> go;
    private readonly TrafficController controller;
    private readonly MainViewModel vm;

    public RedPhaseState(Action<IState> transition, TrafficController controller, MainViewModel vm)
    {
        go = transition; this.controller = controller; this.vm = vm;
    }

    public void OnEnter() { }
    public void OnExit()  { }

    public async Task RunAsync(CancellationToken cancelOnExit)
    {
        try
        {
            await Task.Delay(controller.CurrentRedDuration, cancelOnExit).ConfigureAwait(false);
            go(new RedAmberPhaseState(go, controller, vm));
        }
        catch (OperationCanceledException) { }
    }
}