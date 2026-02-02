# RelayCommand

The `RelayCommand` is a lightweight implementation of the `ICommand` interface that enables MVVM-style command binding for user interface controls. It supports execution guards (predicates to determine if the command can execute) and provides integrated logging capabilities.

## Overview

The `RelayCommand` pattern allows you to bind UI button clicks, menu selections, and other user interactions directly to view model methods without requiring custom code-behind. The command automatically handles thread marshalling to the UI thread and can notify the UI when its executability state changes.

## Features

- **Action-based execution**: Execute parameterless actions via `RelayCommand`
- **Typed parameters**: Execute actions with typed parameters via `RelayCommand<TParam>`
- **Execution guards**: Optional predicates to control when commands can execute
- **Can Execute notifications**: Notify the UI to re-check command executability via `RaiseCanExecuteChanged()`
- **Integrated logging**: Built-in diagnostic logging for command execution
- **UI thread dispatch**: Commands automatically marshal execution to the UI thread

## Types

### RelayCommand

A command that executes an `Action` without parameters.

```csharp
var command = new RelayCommand(
    execute: () => Console.WriteLine("Executed!"),
    canExecute: null,  // Always executable
    name: "SayHello"
);
```

### RelayCommand\<TParam\>

A command that accepts a typed parameter.

```csharp
var command = new RelayCommand<string>(
    execute: (name) => Console.WriteLine($"Hello, {name}!"),
    canExecute: (name) => !string.IsNullOrEmpty(name),
    name: "Greet"
);
```

## Basic Usage

### Parameterless Commands

```csharp
using TA.Utils.Core.MVVM;

public class MyViewModel : ViewModelBase
{
    private string _message = "Click the button";
    private RelayCommand _clickCommand;

    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }

    public RelayCommand ClickCommand => _clickCommand ??= new RelayCommand(
        execute: OnButtonClicked,
        canExecute: CanClickButton,
        name: "ClickButton"
    );

    private void OnButtonClicked()
    {
        Message = "Button was clicked!";
    }

    private bool CanClickButton()
    {
        return !string.IsNullOrEmpty(Message);
    }
}
```

XAML binding:

```xml
<Button Command="{Binding ClickCommand}" Content="Click Me" />
```

### Parameterized Commands

```csharp
public class MyViewModel : ViewModelBase
{
    private RelayCommand<string> _greetCommand;

    public RelayCommand<string> GreetCommand => _greetCommand ??= new RelayCommand<string>(
        execute: OnGreet,
        canExecute: CanGreet,
        name: "GreetPerson"
    );

    private void OnGreet(string name)
    {
        MessageBox.Show($"Hello, {name}!");
    }

    private bool CanGreet(string name)
    {
        return !string.IsNullOrEmpty(name) && name.Length > 2;
    }
}
```

XAML binding with command parameter:

```xml
<Button Command="{Binding GreetCommand}" 
        CommandParameter="{Binding TextBoxName.Text}" 
        Content="Greet" />
```

## Constructor Parameters

### RelayCommand

```csharp
public RelayCommand(
    Action execute,
    Func<bool>? canExecute = null,
    string? name = null,
    ILog? log = null)
```

- **execute** (required): The action to execute
- **canExecute** (optional): Predicate determining if the command can execute; defaults to always returning `true`
- **name** (optional): Display name for diagnostics; defaults to "unnamed"
- **log** (optional): Logger instance; defaults to a degenerate (no-op) logger

### RelayCommand\<TParam\>

```csharp
public RelayCommand(
    Action<TParam> execute,
    Func<TParam, bool>? canExecute = null,
    string? name = null,
    ILog? log = null)
```

## Methods

### CanExecute(object? parameter)

Determines whether the command can currently execute. Called by the UI framework.

```csharp
bool canExecute = command.CanExecute(null);
```

### Execute(object? parameter)

Executes the command. Called when the user invokes the command (e.g., clicks a button).

```csharp
command.Execute(null);
```

### RaiseCanExecuteChanged()

Notifies the UI that the `CanExecute` state may have changed. The UI will re-query `CanExecute` and enable/disable accordingly.

```csharp
private bool _isProcessing = false;

public MyViewModel()
{
    _clickCommand = new RelayCommand(
        execute: DoWork,
        canExecute: () => !_isProcessing,
        name: "DoWorkCommand"
    );
}

public async void DoWork()
{
    _isProcessing = true;
    _clickCommand.RaiseCanExecuteChanged();  // Disable UI control

    await Task.Delay(1000);  // Simulate work

    _isProcessing = false;
    _clickCommand.RaiseCanExecuteChanged();  // Enable UI control
}
```

## Events

### CanExecuteChanged

Raised to notify the UI that `CanExecute` should be re-evaluated.

```csharp
command.CanExecuteChanged += (sender, e) =>
{
    Console.WriteLine("Command executability changed");
};
```

## Logging

When a logger is provided, `RelayCommand` logs:

- **Trace level**: `CanExecute` queries and `Execute` calls
- **Warn level**: Parameter type mismatches
- **Error level**: Exceptions during execution

```csharp
var logger = new MyLoggerService();
var command = new RelayCommand(
    execute: () => { /* ... */ },
    name: "MyCommand",
    log: logger
);
```

## Thread Safety

- Commands automatically marshal execution to the UI thread via `IUiThreadDispatcher`
- The `execute` and `canExecute` callbacks should not block for long periods
- For long-running operations, use `AsyncRelayCommand` instead

## Best Practices

1. **Use lazy initialization** with the null-coalescing operator:
   ```csharp
   public RelayCommand MyCommand => _myCommand ??= new RelayCommand(...);
   ```

2. **Name your commands** for better diagnostics:
   ```csharp
   new RelayCommand(execute, canExecute, name: "SaveDocument")
   ```

3. **Use guard predicates** to prevent invalid operations:
   ```csharp
   canExecute: () => !string.IsNullOrEmpty(TextInput)
   ```

4. **For async operations**, use `AsyncRelayCommand` to prevent overlapping executions

5. **Provide logging** in production code for diagnostics:
   ```csharp
   new RelayCommand(execute, canExecute, name: "MyCommand", log: logger)
   ```

## See Also

- [AsyncRelayCommand](AsyncRelayCommand.md) - For commands that execute asynchronously
- [ViewModelBase](ViewModelBase.md) - Base class for view models
- [UI Thread Dispatcher](UI%20Thread%20Dispatcher.md) - Thread dispatch abstraction
