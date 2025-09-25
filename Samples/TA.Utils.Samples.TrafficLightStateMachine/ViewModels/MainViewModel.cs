using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TA.Utils.Core.StateMachine;

namespace TA.Utils.Samples.TrafficLightStateMachine.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    // Backing fields
    private int queueA;
    private int queueB;
    private bool aRedOn = true, aAmberOn = false, aGreenOn = false;
    private bool bRedOn = true, bAmberOn = false, bGreenOn = false;

    private double arrivalRatePerMinute = 4.0;  // vehicles per minute (default 4 vpm)
    private double secondsPerVehicle   = 1.5;  // service interval during green (default 1.5s)
    private double greenDurationSeconds = 30.0; // adjustable (default 30s)
    private double redDurationSeconds   = 8.0;  // adjustable

    private readonly Services.TrafficController controller;
    private readonly Services.SimulationService simulation;

    // Chart backing fields
    private readonly ObservableCollection<ObservablePoint> seriesA = new();
    private readonly ObservableCollection<ObservablePoint> seriesB = new();
    private readonly Axis xAxis;

    private CancellationTokenSource? samplingCts;
    private Task? samplingTask;

    public MainViewModel()
    {
        controller = new Services.TrafficController(this);
        simulation = new Services.SimulationService(this, controller);

        // Build chart series and axes
        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = seriesA,
                Name = "A",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.DodgerBlue, 2)
            },
            new LineSeries<ObservablePoint>
            {
                Values = seriesB,
                Name = "B",
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.OrangeRed, 2)
            }
        };

        xAxis = new Axis
        {
            Labeler = value => DateTime.FromOADate(value).ToString("HH:mm:ss"),
            UnitWidth = TimeSpan.FromSeconds(1).TotalDays,
            MinStep = TimeSpan.FromSeconds(10).TotalDays
        };
        XAxes = new[] { xAxis };
        YAxes = new[]
        {
            new Axis
            {
                Labeler = v => v.ToString("F0")
            }
        };
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

    // Chart bindable properties
    /// <summary>
    /// The chart series showing the queue lengths for approaches A and B.
    /// </summary>
    public ISeries[] Series { get; }

    /// <summary>
    /// X axis configured to show time for the last fifteen minutes.
    /// </summary>
    public Axis[] XAxes { get; }

    /// <summary>
    /// Y axis configured to show vehicle counts.
    /// </summary>
    public Axis[] YAxes { get; }

    // Commands (simple methods for code-behind bindings)
    public void Start()
    {
        QueueA = 0; QueueB = 0;
        controller.Start();
        simulation.Start();
        StartSampling();
    }

    public void Stop()
    {
        simulation.Stop();
        controller.Stop();
        StopSampling();
    }

    // Utilities for UI thread dispatch
    public void OnUi(Action action)
    {
        if (MainThread.IsMainThread) action();
        else MainThread.BeginInvokeOnMainThread(action);
    }

    private void StartSampling()
    {
        StopSampling();
        samplingCts = new CancellationTokenSource();
        samplingTask = Task.Run(() => SampleLoop(samplingCts.Token));
    }

    private void StopSampling()
    {
        try { samplingCts?.Cancel(); } catch { }
        try { Task.WaitAll(new[] { samplingTask }, TimeSpan.FromMilliseconds(100)); } catch { }
        samplingTask = null;
        samplingCts?.Dispose();
        samplingCts = null;
    }

    private async Task SampleLoop(CancellationToken token)
    {
        const int samplePeriodMs = 1000;
        var window = TimeSpan.FromMinutes(15);
        while (!token.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var x = now.ToOADate();
            // Capture current queues
            var qa = QueueA;
            var qb = QueueB;

            OnUi(() =>
            {
                // Append new points
                seriesA.Add(new ObservablePoint(x, qa));
                seriesB.Add(new ObservablePoint(x, qb));

                // Trim old points outside the 15-minute window
                var minTime = now - window;
                var minOa = minTime.ToOADate();
                while (seriesA.Count > 0 && seriesA[0].X < minOa) seriesA.RemoveAt(0);
                while (seriesB.Count > 0 && seriesB[0].X < minOa) seriesB.RemoveAt(0);

                // Slide the visible window
                xAxis.MinLimit = minOa;
                xAxis.MaxLimit = x;
            });

            try { await Task.Delay(samplePeriodMs, token).ConfigureAwait(false); } catch (OperationCanceledException) { break; }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}