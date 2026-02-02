# AsyncRelayCommand

The `AsyncRelayCommand<TParam>` is an asynchronous implementation of the `IRelayCommand<TParam>` interface. It allows you to execute long-running operations safely without blocking the UI, while automatically preventing overlapping executions.

## Overview

`AsyncRelayCommand<TParam>` is designed for scenarios where command execution involves I/O, network calls, or other async operations. While a command is executing, `CanExecute` automatically returns `false`, ensuring that the user cannot accidentally trigger multiple simultaneous executions.

## Features

- **Async/await support**: Execute `Task`-returning methods
- **Overlapping prevention**: Automatically disables while executing
- **Typed parameters**: Execute with `TParam` parameters
- **Can Execute notifications**: Notify UI to re-check executability via `RaiseCanExecuteChanged()`
- **Integrated logging**: Built-in diagnostic logging
- **UI thread dispatch**: Automatic marshalling to UI thread
- **Thread-safe execution state**: Uses `Interlocked` for thread-safe flags

## Basic Usage

### Simple Async Operation

```csharp
using TA.Utils.Core.MVVM;
using System.Threading.Tasks;

public class DataViewModel : ViewModelBase
{
    private string _data = "Click to load";
    private AsyncRelayCommand<string> _loadDataCommand;

    public string Data
    {
        get => _data;
        set => SetField(ref _data, value);
    }

    public AsyncRelayCommand<string> LoadDataCommand => _loadDataCommand ??= 
        new AsyncRelayCommand<string>(
            execute: LoadDataAsync,
            canExecute: CanLoadData,
            name: "LoadData"
        );

    private async Task LoadDataAsync(string url)
    {
        Data = "Loading...";
        
        try
        {
            // Simulate async operation (network call, database query, etc.)
            await Task.Delay(2000);
            Data = $"Loaded from {url}";
        }
        catch (Exception ex)
        {
            Data = $"Error: {ex.Message}";
        }
    }

    private bool CanLoadData(string url)
    {
        return !string.IsNullOrEmpty(url);
    }
}
```

XAML binding:

```xml
<Button Command="{Binding LoadDataCommand}" 
        CommandParameter="https://example.com" 
        Content="Load Data" />
<TextBlock Text="{Binding Data}" />
```

### File Processing Example

```csharp
public class FileProcessorViewModel : ViewModelBase
{
    private AsyncRelayCommand<string> _processFileCommand;
    private string _status = "Ready";

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public AsyncRelayCommand<string> ProcessFileCommand => _processFileCommand ??= 
        new AsyncRelayCommand<string>(
            execute: ProcessFileAsync,
            canExecute: CanProcessFile,
            name: "ProcessFile"
        );

    private async Task ProcessFileAsync(string filePath)
    {
        Status = "Processing...";
        try
        {
            // Simulate file processing
            var lines = await File.ReadAllLinesAsync(filePath);
            Status = $"Processed {lines.Length} lines";
        }
        catch (Exception ex)
        {
            Status = $"Failed: {ex.Message}";
        }
    }

    private bool CanProcessFile(string filePath)
    {
        return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
    }
}
```

## Constructor

```csharp
public AsyncRelayCommand(
    Func<TParam, Task> execute,
    Func<TParam, bool>? canExecute = null,
    string? name = null,
    ILog? log = null)
```

### Parameters

- **execute** (required): A function that accepts a parameter and returns a `Task`. Exceptions in this task are logged but do not propagate.
- **canExecute** (optional): A predicate that determines executability; defaults to always returning `true`. Only evaluated when not currently executing.
- **name** (optional): Display name for diagnostics; defaults to "unnamed"
- **log** (optional): Logger instance; defaults to a degenerate (no-op) logger

## Properties

### Name

The name of the command (for diagnostic/display purposes).

```csharp
string commandName = command.Name;  // "LoadData"
```

## Methods

### CanExecute(object? parameter)

Returns `false` if the command is currently executing, regardless of the `canExecute` predicate. Returns the result of the `canExecute` predicate otherwise.

```csharp
if (command.CanExecute(parameter))
{
    // Command is not executing and predicate returned true
}
```

### Execute(object? parameter)

Executes the command asynchronously. The method returns immediately (non-blocking). Exceptions thrown in the task are logged and do not propagate to the caller.

```csharp
command.Execute("https://example.com");  // Returns immediately
// Actual async work happens in the background
```

### RaiseCanExecuteChanged()

Notifies the UI that the `CanExecute` state may have changed. Marshalled to the UI thread.

```csharp
// After an external state change that affects canExecute
_externalStateChanged = false;
command.RaiseCanExecuteChanged();
```

## Events

### CanExecuteChanged

Raised to notify the UI that `CanExecute` should be re-evaluated. Invoked on the UI thread.

```csharp
command.CanExecuteChanged += (sender, e) =>
{
    Console.WriteLine("Can execute state changed");
};
```

## Execution State Management

The command automatically manages its execution state:

1. **Before execution**: `CanExecute` returns `true` (if predicate allows)
2. **During execution**: `CanExecute` returns `false` (blocking further executions)
3. **After execution completes**: `CanExecute` reverts to predicate-based result

This prevents overlapping executions without explicit user code.

```csharp
// Conceptually, here's what happens internally:
private async Task Execute(TParam parameter)
{
    // Set executing flag
    Interlocked.Increment(ref _isExecuting);
    try
    {
        await executeFunction(parameter);  // User's async method
    }
    finally
    {
        // Clear executing flag
        Interlocked.Decrement(ref _isExecuting);
    }
}
```

## Error Handling

Exceptions thrown in the `execute` task are caught, logged, and do not propagate. Check your logger to diagnose errors.

```csharp
private async Task LoadDataAsync(string url)
{
    try
    {
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        // ...
    }
    catch (HttpRequestException ex)
    {
        log.Error().Exception(ex).Message("HTTP request failed").Write();
        Data = "Network error";
    }
}
```

## Logging

When a logger is provided, `AsyncRelayCommand<TParam>` logs:

- **Trace level**: `CanExecute` queries and execution state changes
- **Error level**: Exceptions during task execution

```csharp
var logger = new MyLoggerService();
var command = new AsyncRelayCommand<string>(
    execute: LoadDataAsync,
    name: "LoadData",
    log: logger
);
```

## UI Responsiveness

Because execution is asynchronous, the UI remains responsive:

```csharp
// This does NOT block the UI
await command.Execute(param);

// The UI continues to process user input while the task runs
```

## Cancellation

`AsyncRelayCommand<TParam>` does not have built-in cancellation support. If you need cancellation, use `CancellationToken`:

```csharp
private CancellationTokenSource _cancellationTokenSource;

private async Task LoadDataAsync(string url)
{
    using (_cancellationTokenSource = new CancellationTokenSource())
    {
        try
        {
            var data = await FetchDataAsync(url, _cancellationTokenSource.Token);
            Data = data;
        }
        catch (OperationCanceledException)
        {
            Data = "Cancelled";
        }
    }
}

public void CancelOperation()
{
    _cancellationTokenSource?.Cancel();
}
```

## Best Practices

1. **Always provide meaningful names** for diagnostics:
   ```csharp
   new AsyncRelayCommand<string>(execute, canExecute, name: "LoadUserData")
   ```

2. **Handle exceptions gracefully** in your execute method:
   ```csharp
   private async Task SaveAsync(Entity entity)
   {
        try { await _repository.SaveAsync(entity); }
        catch (Exception ex) { HandleError(ex); }
   }
   ```

3. **Use guard predicates** to prevent invalid operations:
   ```csharp
   canExecute: (id) => id > 0 && !string.IsNullOrEmpty(_userId)
   ```

4. **Provide user feedback**:
   ```csharp
   private async Task LongOperationAsync(string data)
   {
       Status = "Processing...";
       await DoWorkAsync(data);
       Status = "Complete";
   }
   ```

5. **Use descriptive UI states**:
   ```xml
   <Button Command="{Binding LoadDataCommand}" 
           IsEnabled="{Binding LoadDataCommand.CanExecute}"
           Content="{Binding LoadDataCommand, Converter={StaticResource CommandToStatusConverter}}" />
   ```

## See Also

- [RelayCommand](RelayCommand.md) - For synchronous command execution
- [ViewModelBase](ViewModelBase.md) - Base class for view models
- [UI Thread Dispatcher](UI%20Thread%20Dispatcher.md) - Thread dispatch abstraction
