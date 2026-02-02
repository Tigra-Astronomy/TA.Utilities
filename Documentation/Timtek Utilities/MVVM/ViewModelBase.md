# ViewModelBase

The `ViewModelBase` is a foundational class for implementing the MVVM (Model-View-ViewModel) pattern. It provides built-in support for property change notifications, disposal patterns, and optional logging.

## Overview

`ViewModelBase` implements `INotifyPropertyChanged` and `IDisposable`, making it ideal as a base class for view models in WPF, WinForms, and other UI frameworks that support data binding and property notifications.

## Features

- **INotifyPropertyChanged support**: Automatically notifies UI controls when properties change
- **Property change tracking**: The `SetField<T>` method handles equality checks and notifications
- **Optional logging**: Integrates with the TA.Utilities logging infrastructure
- **Disposal pattern**: Implements the standard Dispose pattern for resource cleanup

## Basic Usage

Create a view model by inheriting from `ViewModelBase`:

```csharp
using TA.Utils.Core.MVVM;

public class MyViewModel : ViewModelBase
{
    private string _name;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public MyViewModel() : base()
    {
        // Initialize your view model
    }
}
```

## SetField Method

The `SetField<T>` method handles property updates with automatic change notifications:

```csharp
protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
```

### Parameters

- **field**: A reference to the backing field
- **value**: The new value to set
- **propertyName**: The property name (automatically populated via `CallerMemberName`)

### Return Value

Returns `true` if the value was changed, `false` if the value was already the same.

### Example

```csharp
public class PersonViewModel : ViewModelBase
{
    private string _firstName;
    private int _age;

    public string FirstName
    {
        get => _firstName;
        set => SetField(ref _firstName, value);  // Auto-uses property name "FirstName"
    }

    public int Age
    {
        get => _age;
        set => SetField(ref _age, value);
    }
}
```

## Logging Integration

When creating a view model, you can optionally pass an `ILog` instance for diagnostic logging:

```csharp
var log = new SomeLoggerService();
var viewModel = new MyViewModel(log);
```

If no logger is provided, a `DegenerateLoggerService` (no-op logger) is used by default.

## Handling Disposal

Override the `Dispose(bool disposing)` method to clean up any resources:

```csharp
public class MyViewModel : ViewModelBase
{
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Release managed resources here
            // Example: unsubscribe from events, dispose timers, etc.
        }
        base.Dispose(disposing);
    }
}
```

## INotifyPropertyChanged Events

Subscribe to property change notifications:

```csharp
var viewModel = new MyViewModel();

viewModel.PropertyChanged += (sender, e) =>
{
    if (e.PropertyName == nameof(viewModel.Name))
    {
        Console.WriteLine("Name changed!");
    }
};

viewModel.Name = "Alice";  // Triggers PropertyChanged event
```

## Thread Safety

Property changes are not inherently thread-safe. If you need to update properties from worker threads, ensure you invoke the change notification on the UI thread using an appropriate dispatcher for your platform.

## See Also

- [RelayCommand](RelayCommand.md) - Command binding support for MVVM
- [AsyncRelayCommand](AsyncRelayCommand.md) - Async command execution
- [UI Thread Dispatcher](UI%20Thread%20Dispatcher.md) - Thread dispatch abstraction
