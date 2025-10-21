using System;
using System.Threading;
using System.Threading.Tasks;
using TA.Utils.Samples.TrafficLightStateMachine.ViewModels;

namespace TA.Utils.Samples.TrafficLightStateMachine.Services;

public sealed class SimulationService
{
    private readonly MainViewModel vm;
    private readonly TrafficController controller;

    private CancellationTokenSource? cts;
    private Task? arrivalsATask, arrivalsBTask, departuresTask;
    private readonly Random random = new();

    public SimulationService(MainViewModel vm, TrafficController controller)
    {
        this.vm = vm;
        this.controller = controller;
    }

    public void Start()
    {
        Stop();
        cts = new CancellationTokenSource();
        arrivalsATask = Task.Run(() => ArrivalLoop(() => true, () => vm.QueueA++, cts.Token));
        arrivalsBTask = Task.Run(() => ArrivalLoop(() => true, () => vm.QueueB++, cts.Token));
        departuresTask = Task.Run(() => DepartureLoop(cts.Token));
    }

    public void Stop()
    {
        try { cts?.Cancel(); } catch { }
        try { Task.WaitAll(new[] { arrivalsATask, arrivalsBTask, departuresTask }, TimeSpan.FromMilliseconds(100)); } catch { }
        arrivalsATask = arrivalsBTask = departuresTask = null;
        cts?.Dispose();
        cts = null;
    }

    private async Task ArrivalLoop(Func<bool> shouldArrive, Action onArrive, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var lambda = vm.ArrivalRatePerMinute / 60.0; // per second
            if (lambda <= 0)
            {
                await Task.Delay(250, token).ConfigureAwait(false);
                continue;
            }
            // Sample exponential inter-arrival time
            var u = Math.Max(double.Epsilon, 1.0 - random.NextDouble());
            var seconds = -Math.Log(u) / lambda;
            var delay = TimeSpan.FromSeconds(seconds);
            try { await Task.Delay(delay, token).ConfigureAwait(false); }
            catch (OperationCanceledException) { break; }
            if (token.IsCancellationRequested) break;
            if (shouldArrive()) vm.OnUi(onArrive);
        }
    }

    private async Task DepartureLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var interval = vm.SecondsPerVehicle;
            if (interval <= 0)
            {
                await Task.Delay(100, token).ConfigureAwait(false);
                continue;
            }

            // Pick which queue can depart based on which approach is currently green
            bool aGreen = controller.ActiveApproachIndex == 0 && vm.AGreenOn;
            bool bGreen = controller.ActiveApproachIndex == 1 && vm.BGreenOn;

            if (aGreen && vm.QueueA > 0)
            {
                try { await Task.Delay(TimeSpan.FromSeconds(interval), token).ConfigureAwait(false); } catch (OperationCanceledException) { break; }
                if (token.IsCancellationRequested) break;
                vm.OnUi(() => { if (vm.QueueA > 0) vm.QueueA--; });
                continue;
            }
            if (bGreen && vm.QueueB > 0)
            {
                try { await Task.Delay(TimeSpan.FromSeconds(interval), token).ConfigureAwait(false); } catch (OperationCanceledException) { break; }
                if (token.IsCancellationRequested) break;
                vm.OnUi(() => { if (vm.QueueB > 0) vm.QueueB--; });
                continue;
            }

            await Task.Delay(100, token).ConfigureAwait(false);
        }
    }
}