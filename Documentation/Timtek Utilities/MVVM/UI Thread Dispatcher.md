# UI Thread Dispatcher

The `IUiThreadDispatcher` abstraction allows commands and other components to safely marshal work to the UI thread. It provides a testable, platform-independent way to handle thread synchronization in MVVM applications.

## Overview

UI frameworks like WPF and WinForms require that UI updates happen on the UI thread. The `IUiThreadDispatcher` interface provides a clean abstraction for this requirement, allowing your business logic and commands to work correctly regardless of which UI framework is in use.

The dispatcher can be configured globally per thread via the `UiThreadDispatcherContext`, making it easy to test code that uses `IRelayCommand` without requiring a real UI thread.

## Components

### IUiThreadDispatcher Interface

```csharp
public interface IUiThreadDispatcher
{
    /// <summary>
    ///     Execute an action on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute on the UI thread.</param>
    void Post(Action action);
}
```

### UiThreadDispatcherContext

A static context manager for accessing the current dispatcher:

```csharp
public static class UiThreadDispatcherContext
{
    public static IUiThreadDispatcher Current { get; }
    public static void SetDispatcher(IUiThreadDispatcher? value);
}
```

## Basic Usage

### WPF Applications

For WPF applications, set the dispatcher during application startup:

```csharp
using System.Windows.Threading;
using TA.Utils.Core.MVVM;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Set up the UI thread dispatcher for MVVM commands
        var wpfDispatcher = new WpfUiThreadDispatcher();
        UiThreadDispatcherContext.SetDispatcher(wpfDispatcher);
    }
}

public class WpfUiThreadDispatcher : IUiThreadDispatcher
{
    public void Post(Action action)
    {
        Dispatcher.CurrentDispatcher.BeginInvoke(action);
    }
}
```

### Unit Tests

For unit tests, use the `CurrentThreadDispatcher` to execute on the current thread:

```csharp
using TA.Utils.Core.MVVM;

[TestFixture]
public class MyViewModelTests
{
    [SetUp]
    public void Setup()
    {
        // Use the current thread dispatcher for testing
        UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
    }

    [Test]
    public void ClickCommand_ShouldUpdateProperty()
    {
        var viewModel = new MyViewModel();
        viewModel.ClickCommand.Execute(null);
        
        Assert.That(viewModel.Message, Is.EqualTo("Button was clicked!"));
    }
}
```

### Console Applications

For console applications that don't have a UI thread:

```csharp
using TA.Utils.Core.MVVM;

class Program
{
    static void Main()
    {
        // Set up the dispatcher for the main thread
        UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
        
        var viewModel = new MyViewModel();
        viewModel.MyCommand.Execute(null);
    }
}
```

## CurrentThreadDispatcher

The `CurrentThreadDispatcher` is a simple implementation that executes actions on the current thread. It's useful for testing and console applications where no real UI thread dispatch is needed.

```csharp
public class CurrentThreadDispatcher : IUiThreadDispatcher
{
    public void Post(Action action)
    {
        action();  // Execute immediately on current thread
    }
}
```

## Advanced Usage

### Custom Dispatcher for WinForms

```csharp
using System.Windows.Forms;
using TA.Utils.Core.MVVM;

public class WinFormsDispatcher : IUiThreadDispatcher
{
    private readonly Control _uiControl;

    public WinFormsDispatcher(Control uiControl)
    {
        _uiControl = uiControl;
    }

    public void Post(Action action)
    {
        if (_uiControl.InvokeRequired)
        {
            _uiControl.BeginInvoke(new Action(action));
        }
        else
        {
            action();
        }
    }
}

// In your Form_Load or startup:
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        UiThreadDispatcherContext.SetDispatcher(new WinFormsDispatcher(this));
    }
}
```

### Thread-Local Context

The dispatcher context is thread-static, meaning each thread can have its own dispatcher:

```csharp
// Main UI thread
UiThreadDispatcherContext.SetDispatcher(new WpfUiThreadDispatcher());

// Background worker thread
Task.Run(() =>
{
    // This thread gets its own dispatcher context
    UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
    
    // Commands created and executed here use the background dispatcher
    var command = new RelayCommand(() => Console.WriteLine("Background work"));
    command.Execute(null);
});
```

### Testing Complex Scenarios

```csharp
[TestFixture]
public class CommandDispatcherTests
{
    private List<string> _executedActions;

    [SetUp]
    public void Setup()
    {
        _executedActions = new List<string>();
        
        // Custom test dispatcher that tracks executions
        UiThreadDispatcherContext.SetDispatcher(
            new TrackingDispatcher(_executedActions)
        );
    }

    [Test]
    public void RaiseCanExecuteChanged_MarshalsThroughDispatcher()
    {
        var command = new RelayCommand(
            execute: () => { },
            name: "TestCommand"
        );

        command.RaiseCanExecuteChanged();

        Assert.That(_executedActions, Contains.Item("Dispatched"));
    }

    private class TrackingDispatcher : IUiThreadDispatcher
    {
        private readonly List<string> _actions;

        public TrackingDispatcher(List<string> actions)
        {
            _actions = actions;
        }

        public void Post(Action action)
        {
            _actions.Add("Dispatched");
            action();
        }
    }
}
```

## How RelayCommand Uses the Dispatcher

The `RelayCommand` classes automatically obtain the current dispatcher and use it to marshal `CanExecuteChanged` notifications to the UI thread:

```csharp
public class RelayCommand : IRelayCommand
{
    private readonly IUiThreadDispatcher dispatcher;

    public RelayCommand(Action execute, Func<bool>? canExecute = null, 
                       string? name = null, ILog? log = null)
    {
        // Obtain the current dispatcher during construction
        dispatcher = UiThreadDispatcherContext.Current;
        
        // ... other initialization
    }

    public void RaiseCanExecuteChanged()
    {
        try
        {
            // Marshal the notification to the UI thread
            dispatcher.Post(OnCanExecuteChanged);
        }
        catch (Exception e)
        {
            log.Error().Exception(e).Write();
        }
    }
}
```

## Default Behavior

If no dispatcher is explicitly set via `SetDispatcher()`, the `Current` property returns a `CurrentThreadDispatcher`:

```csharp
// Without explicit setup:
var current = UiThreadDispatcherContext.Current;  
// Returns new CurrentThreadDispatcher() if none was set
```

This ensures that commands work even if no dispatcher is configured, though they will execute on the current thread rather than the UI thread.

## Best Practices

1. **Set the dispatcher early**: Configure it during application startup, before creating view models.

2. **Use thread-local configuration**: Each thread can have its own dispatcher if needed.

3. **Test with CurrentThreadDispatcher**: Makes your tests deterministic and fast.

4. **Provide custom dispatchers**: For custom UI frameworks or platforms.

5. **Don't dispatch to UI thread from UI thread**: The dispatcher implementation should check this if needed.

## See Also

- [RelayCommand](RelayCommand.md) - Uses the dispatcher for `CanExecuteChanged` notifications
- [AsyncRelayCommand](AsyncRelayCommand.md) - Also uses the dispatcher for event marshalling
- [ViewModelBase](ViewModelBase.md) - Often used with commands that use the dispatcher
