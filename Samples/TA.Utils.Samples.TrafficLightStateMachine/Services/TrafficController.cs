using System;
using System.Threading;
using System.Threading.Tasks;
using TA.Utils.Core.StateMachine;
using TA.Utils.Samples.TrafficLightStateMachine.ViewModels;

namespace TA.Utils.Samples.TrafficLightStateMachine.Services;

public sealed class TrafficController
{
    private readonly IFiniteStateMachine<IState> fsm;
    private readonly MainViewModel vm;

    private readonly Action<IState> go;
    private CancellationTokenSource? cts;

    public bool PrimaryIsA { get; private set; } = true; // A starts Green; B starts Red

    public TrafficController(MainViewModel vm)
    {
        this.vm = vm;
        fsm = new FiniteStateMachine<IState>();
        go = s => fsm.TransitionTo(s);
        fsm.ObservableStates.Subscribe(_ => ApplyLights());
    }

    public void Start()
    {
        cts = new CancellationTokenSource();
        var initial = new States.GreenPhaseState(go, this, vm);
        fsm.StartStateMachine(initial);
    }

    public void Stop()
    {
        try { cts?.Cancel(); } catch { }
        fsm.StopStateMachine();
    }

    public void FlipPrimary() => PrimaryIsA = !PrimaryIsA;

    public void ApplyLights()
    {
        var state = fsm.CurrentState;
        if (state is null) return;

        // Determine colours for A and B from current phase and PrimaryIsA flag
        bool aRed = false, aAmber = false, aGreen = false;
        bool bRed = false, bAmber = false, bGreen = false;

        switch (state)
        {
            case States.GreenPhaseState:
                if (PrimaryIsA) { aGreen = true; bRed = true; }
                else            { bGreen = true; aRed = true; }
                break;
            case States.AmberPhaseState:
                if (PrimaryIsA) { aAmber = true; bRed = true; }
                else            { bAmber = true; aRed = true; }
                break;
            case States.RedPhaseState:
                aRed = true; bRed = true;
                break;
            case States.RedAmberPhaseState:
                if (PrimaryIsA) { aRed = true; aAmber = true; bRed = true; }
                else            { bRed = true; bAmber = true; aRed = true; }
                break;
        }

        vm.OnUi(() =>
        {
            vm.ARedOn = aRed; vm.AAmberOn = aAmber; vm.AGreenOn = aGreen;
            vm.BRedOn = bRed; vm.BAmberOn = bAmber; vm.BGreenOn = bGreen;
        });
    }

    public TimeSpan CurrentGreenDuration => TimeSpan.FromSeconds(vm.GreenDurationSeconds);
    public TimeSpan CurrentAmberDuration => TimeSpan.FromSeconds(2); // fixed
    public TimeSpan CurrentRedDuration   => TimeSpan.FromSeconds(vm.RedDurationSeconds);
    public TimeSpan CurrentRedAmberDuration => TimeSpan.FromSeconds(2); // fixed
}