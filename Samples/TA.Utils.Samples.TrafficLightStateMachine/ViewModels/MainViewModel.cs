using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TA.Utils.Core.StateMachine;

namespace TA.Utils.Samples.TrafficLightStateMachine.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    // Backing fields
    private int queueA;
    private int queueB;
    private bool aRedOn = true, aAmberOn, aGreenOn;
    private bool bRedOn, bAmberOn, bGreenOn = true;

    private double arrivalRatePerMinute = 30.0; // vehicles per minute
    private double secondsPerVehicle   = 2.0;   // service interval during green
    private double greenDurationSeconds = 10.0; // adjustable
    private double redDurationSeconds   = 8.0;  // adjustable

    private readonly Services.TrafficController controller;
    private readonly Services.SimulationService simulation;

    public MainViewModel()
    {
        controller = new Services.TrafficController(this);
        simulation = new Services.SimulationService(this, controller);
    }

    // Bindable properties
    public int QueueA
    {
        get => queueA;
        set { if (value == queueA) return; queueA = value; OnPropertyChanged(); }
    }

    public int QueueB
    {
        get => queueB;
        set { if (value == queueB) return; queueB = value; OnPropertyChanged(); }
    }

    public bool ARedOn   { get => aRedOn;   set { if (value == aRedOn) return;   aRedOn = value;   OnPropertyChanged(); } }
    public bool AAmberOn { get => aAmberOn; set { if (value == aAmberOn) return; aAmberOn = value; OnPropertyChanged(); } }
    public bool AGreenOn { get => aGreenOn; set { if (value == aGreenOn) return; aGreenOn = value; OnPropertyChanged(); } }

    public bool BRedOn   { get => bRedOn;   set { if (value == bRedOn) return;   bRedOn = value;   OnPropertyChanged(); } }
    public bool BAmberOn { get => bAmberOn; set { if (value == bAmberOn) return; bAmberOn = value; OnPropertyChanged(); } }
    public bool BGreenOn { get => bGreenOn; set { if (value == bGreenOn) return; bGreenOn = value; OnPropertyChanged(); } }

    // Parameters
    public double ArrivalRatePerMinute
    {
        get => arrivalRatePerMinute;
        set { if (Math.Abs(value - arrivalRatePerMinute) < 0.0001) return; arrivalRatePerMinute = Math.Max(0, value); OnPropertyChanged(); }
    }

    public double SecondsPerVehicle
    {
        get => secondsPerVehicle;
        set { if (Math.Abs(value - secondsPerVehicle) < 0.0001) return; secondsPerVehicle = Math.Max(0.1, value); OnPropertyChanged(); }
    }

    public double GreenDurationSeconds
    {
        get => greenDurationSeconds;
        set { if (Math.Abs(value - greenDurationSeconds) < 0.0001) return; greenDurationSeconds = Math.Max(1, value); OnPropertyChanged(); }
    }

    public double RedDurationSeconds
    {
        get => redDurationSeconds;
        set { if (Math.Abs(value - redDurationSeconds) < 0.0001) return; redDurationSeconds = Math.Max(1, value); OnPropertyChanged(); }
    }

    // Commands (simple methods for code-behind bindings)
    public void Start()
    {
        QueueA = 0; QueueB = 0;
        controller.Start();
        simulation.Start();
    }

    public void Stop()
    {
        simulation.Stop();
        controller.Stop();
    }

    // Utilities for UI thread dispatch
    public void OnUi(Action action)
    {
        if (MainThread.IsMainThread) action();
        else MainThread.BeginInvokeOnMainThread(action);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}