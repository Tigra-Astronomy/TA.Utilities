using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace TA.Utils.Samples.TrafficLightStateMachine.Triggers;

/// <summary>
/// A simple state trigger that becomes active based on device orientation.
/// </summary>
public sealed class OrientationStateTrigger : StateTriggerBase
{
    public enum OrientationKind
    {
        Portrait,
        Landscape
    }

    public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
        nameof(Orientation), typeof(OrientationKind), typeof(OrientationStateTrigger), OrientationKind.Portrait,
        propertyChanged: (bindable, _, _) => ((OrientationStateTrigger)bindable).Evaluate());

    /// <summary>
    /// Desired orientation for the trigger to be active.
    /// </summary>
    public OrientationKind Orientation
    {
        get => (OrientationKind)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public OrientationStateTrigger()
    {
        DeviceDisplay.MainDisplayInfoChanged += OnDisplayInfoChanged;
        Evaluate();
    }

    private void OnDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e) => Evaluate(e.DisplayInfo);

    private void Evaluate() => Evaluate(DeviceDisplay.MainDisplayInfo);

    private void Evaluate(DisplayInfo info)
    {
        var isLandscape = info.Orientation == DisplayOrientation.Landscape || info.Rotation == DisplayRotation.Rotation90 || info.Rotation == DisplayRotation.Rotation270;
        var active = Orientation == OrientationKind.Landscape ? isLandscape : !isLandscape;
        SetActive(active);
    }
}
