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

    /// <summary>
    /// Gets the index of the approach currently permitted to proceed during non-red phases.
    /// 0 corresponds to approach A, 1 corresponds to approach B.
    /// </summary>
    public int ActiveApproachIndex { get; private set; } = 0;

    private const int ApproachCount = 2;

    /// <summary>
    /// Advances the active approach to the next approach in a round-robin fashion.
    /// </summary>
    public void AdvanceToNextApproach() => ActiveApproachIndex = (ActiveApproachIndex + 1) % ApproachCount;

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
        // Start with both directions red for the configured dead time
        var initial = new States.RedPhaseState(go, this, vm);
        fsm.StartStateMachine(initial);
    }

    public void Stop()
    {
        try { cts?.Cancel(); } catch { }
        fsm.StopStateMachine();
    }

    public void ApplyLights()
    {
        var state = fsm.CurrentState;
        if (state is null) return;

        // Determine colours for A and B from current phase and ActiveApproachIndex
        bool aRed = false, aAmber = false, aGreen = false;
        bool bRed = false, bAmber = false, bGreen = false;

        switch (state)
        {
            case States.GreenPhaseState:
                if (ActiveApproachIndex == 0) { aGreen = true; bRed = true; }
                else                         { bGreen = true; aRed = true; }
                break;
            case States.AmberPhaseState:
                if (ActiveApproachIndex == 0) { aAmber = true; bRed = true; }
                else                         { bAmber = true; aRed = true; }
                break;
            case States.RedPhaseState:
                aRed = true; bRed = true;
                break;
            case States.RedAmberPhaseState:
                if (ActiveApproachIndex == 0) { aRed = true; aAmber = true; bRed = true; }
                else                         { bRed = true; bAmber = true; aRed = true; }
                break;
        }

        vm.OnUi(() =>
        {
            vm.ARedOn = aRed; vm.AAmberOn = aAmber; vm.AGreenOn = aGreen;
            vm.BRedOn = bRed; vm.BAmberOn = bAmber; vm.BGreenOn = bGreen;
        });
    }

    /// <summary>
    /// Gets the current green phase duration.
    /// </summary>
    public TimeSpan CurrentGreenDuration => TimeSpan.FromSeconds(vm.GreenDurationSeconds);

    private const double AmberSeconds = 2.0;
    private const double RedAmberSeconds = 2.0;

    /// <summary>
    /// Gets the current amber phase duration. This is fixed at two seconds but can be adjusted in code.
    /// </summary>
    public TimeSpan CurrentAmberDuration => TimeSpan.FromSeconds(AmberSeconds);

    /// <summary>
    /// Gets the current red (dead time) duration where all approaches show red.
    /// </summary>
    public TimeSpan CurrentRedDuration   => TimeSpan.FromSeconds(vm.RedDurationSeconds);

    /// <summary>
    /// Gets the current red+amber phase duration. This is fixed at two seconds but can be adjusted in code.
    /// </summary>
    public TimeSpan CurrentRedAmberDuration => TimeSpan.FromSeconds(RedAmberSeconds);
}
