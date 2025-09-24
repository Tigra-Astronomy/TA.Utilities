using System;
using System.Globalization;

namespace TA.Utils.Samples.TrafficLightStateMachine;

public sealed class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var on = value is bool b && b;
        var phase = parameter as string ?? string.Empty;
        return (on, phase) switch
        {
            (true, "Red")   => Colors.Red,
            (true, "Amber") => Colors.Orange,
            (true, "Green") => Colors.LimeGreen,
            _ => Color.FromRgba(30, 30, 30, 255)
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}