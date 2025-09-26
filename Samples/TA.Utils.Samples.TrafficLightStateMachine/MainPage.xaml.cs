using Microsoft.Maui.Devices;
using TA.Utils.Samples.TrafficLightStateMachine.ViewModels;

namespace TA.Utils.Samples.TrafficLightStateMachine;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel vm = new();
    private bool snapping;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Attach Android-only drag completed snapping to avoid fine-resolution drift when releasing the thumb
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            if (GreenSlider is not null) GreenSlider.DragCompleted += (_, __) => SnapSliderToStep(GreenSlider, 1.0);
            if (RedSlider is not null) RedSlider.DragCompleted += (_, __) => SnapSliderToStep(RedSlider, 1.0);
            if (ArrivalSlider is not null) ArrivalSlider.DragCompleted += (_, __) => SnapSliderToStep(ArrivalSlider, 1.0);
            if (SecondsSlider is not null) SecondsSlider.DragCompleted += (_, __) => SnapSliderToStep(SecondsSlider, 0.1);
        }
    }

    private void SnapSliderToStep(Slider slider, double step)
    {
        if (snapping) return;
        try
        {
            snapping = true;
            var min = slider.Minimum;
            var max = slider.Maximum;
            var value = slider.Value;
            var snapped = Snap(value, step, min, max);
            if (Math.Abs(snapped - value) > 1e-9)
                slider.Value = snapped;
        }
        finally { snapping = false; }
    }

    private static double Snap(double value, double step, double min, double max)
    {
        if (step <= 0) return Math.Clamp(value, min, max);
        var k = Math.Round((value - min) / step, MidpointRounding.AwayFromZero);
        var snapped = min + (k * step);
        return Math.Clamp(snapped, min, max);
    }

    private void OnGreenMinusClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Platform != DevicePlatform.Android) return;
        vm.GreenDurationSeconds = Math.Max(GreenSlider.Minimum, vm.GreenDurationSeconds - 1.0);
    }

    private void OnGreenPlusClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Platform != DevicePlatform.Android) return;
        vm.GreenDurationSeconds = Math.Min(GreenSlider.Maximum, vm.GreenDurationSeconds + 1.0);
    }

    private void OnRedMinusClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Platform != DevicePlatform.Android) return;
        vm.RedDurationSeconds = Math.Max(RedSlider.Minimum, vm.RedDurationSeconds - 1.0);
    }

    private void OnRedPlusClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Platform != DevicePlatform.Android) return;
        vm.RedDurationSeconds = Math.Min(RedSlider.Maximum, vm.RedDurationSeconds + 1.0);
    }

    private void OnArrivalsMinusClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Platform != DevicePlatform.Android) return;
        vm.ArrivalRatePerMinute = Math.Max(ArrivalSlider.Minimum, vm.ArrivalRatePerMinute - 1.0);
    }

    private void OnArrivalsPlusClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Platform != DevicePlatform.Android) return;
        vm.ArrivalRatePerMinute = Math.Min(ArrivalSlider.Maximum, vm.ArrivalRatePerMinute + 1.0);
    }

    private void OnSecondsMinusClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Platform != DevicePlatform.Android) return;
        vm.SecondsPerVehicle = Math.Max(SecondsSlider.Minimum, vm.SecondsPerVehicle - 0.1);
    }

    private void OnSecondsPlusClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Platform != DevicePlatform.Android) return;
        vm.SecondsPerVehicle = Math.Min(SecondsSlider.Maximum, vm.SecondsPerVehicle + 0.1);
    }

    private void OnStartClicked(object? sender, EventArgs e) => vm.Start();
    private void OnStopClicked(object? sender, EventArgs e)  => vm.Stop();
}
