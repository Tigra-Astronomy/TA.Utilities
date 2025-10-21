using System;
using System.Threading;
using System.Threading.Tasks;
using TA.Utils.Core.StateMachine;
using TA.Utils.Samples.TrafficLightStateMachine.Services;
using TA.Utils.Samples.TrafficLightStateMachine.ViewModels;

namespace TA.Utils.Samples.TrafficLightStateMachine.States;

public sealed class AmberPhaseState : IState
{
    public string DisplayName => "Amber";

    private readonly Action<IState> go;
    private readonly TrafficController controller;
    private readonly MainViewModel vm;

    public AmberPhaseState(Action<IState> transition, TrafficController controller, MainViewModel vm)
    {
        go = transition; this.controller = controller; this.vm = vm;
    }

    public void OnEnter() { }
    public void OnExit()  { }

    public async Task RunAsync(CancellationToken cancelOnExit)
    {
        try
        {
            await Task.Delay(controller.CurrentAmberDuration, cancelOnExit).ConfigureAwait(false);
            // After amber completes, rotate to the next approach, then enter the red (dead time)
            controller.AdvanceToNextApproach();
            go(new RedPhaseState(go, controller, vm));
        }
        catch (OperationCanceledException) { }
    }
}
