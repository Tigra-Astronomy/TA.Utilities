using System;
using System.Threading;
using System.Threading.Tasks;
using TA.Utils.Core.StateMachine;
using TA.Utils.Samples.TrafficLightStateMachine.Services;
using TA.Utils.Samples.TrafficLightStateMachine.ViewModels;

namespace TA.Utils.Samples.TrafficLightStateMachine.States;

public sealed class GreenPhaseState : IState
{
    public string DisplayName => "Green";

    private readonly Action<IState> go;
    private readonly TrafficController controller;
    private readonly MainViewModel vm;

    public GreenPhaseState(Action<IState> transition, TrafficController controller, MainViewModel vm)
    {
        go = transition; this.controller = controller; this.vm = vm;
    }

    public void OnEnter() { /* no-op */ }
    public void OnExit()  { /* no-op */ }

    public async Task RunAsync(CancellationToken cancelOnExit)
    {
        try
        {
            await Task.Delay(controller.CurrentGreenDuration, cancelOnExit).ConfigureAwait(false);
            go(new AmberPhaseState(go, controller, vm));
        }
        catch (OperationCanceledException) { }
    }
}