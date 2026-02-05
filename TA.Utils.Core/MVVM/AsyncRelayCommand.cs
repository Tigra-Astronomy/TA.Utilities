using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Core.MVVM;

/// <summary>
/// Gets a value indicating whether the async relay command is currently executing.
/// </summary>
public class AsyncRelayCommand<TParam> : IAsyncRelayCommand<TParam>
{
    private readonly Func<TParam, Task> execute;
    private readonly Func<TParam, bool> canExecuteQuery;
    private long isExecuting;
    private readonly ILog log;
    private readonly IUiThreadDispatcher dispatcher;

    /// <summary>
    ///     Initialize a new instance of a relay command that executes asynchronously and takes parameter of type
    ///     <typeparamref name="TParam" />.
    /// </summary>
    /// <param name="execute">A function that returns a task that completes after the command fully executes.</param>
    /// <param name="canExecute">
    ///     A function that returns a boolean indicating whether the command can currently execute. If no
    ///     value is supplied, then the command can always execute.
    /// </param>
    /// <param name="name">The name of the command (for diagnostic/display purposes). Optional; defaults to "unnamed".</param>
    /// <param name="log">
    ///     An optional logging service. If provided, log entries will be generated for <see cref="CanExecute" />
    ///     state changes, <see cref="CanExecuteChanged" /> events and command invocations.
    /// </param>
    public AsyncRelayCommand(
        Func<TParam, Task> execute,
        Func<TParam, bool>? canExecute = null,
        string? name = null,
        ILog? log = null)
    {
        dispatcher = UiThreadDispatcherContext.Current;
        Name = name ?? "unnamed";
        this.execute = execute;
        canExecuteQuery = canExecute ?? (param => true);
        this.log = log ?? new DegenerateLoggerService();
    }

    /// <summary>
    ///    Gets a value indicating whether the command is currently executing.
    /// </summary>
    public bool IsRunning => Interlocked.Read(ref isExecuting) != 0;

    /// <summary>
    ///     The name of the command for diagnostic/display purposes.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Raised as a notification to the control that it should re-check the value of <see cref="CanExecute" />.
    ///     Note: delegates will be invoked on the UI thread.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    protected virtual void OnCanExecuteChanged() =>
        // ReSharper disable once EventExceptionNotDocumented
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    ///     Called from a ViewModel to notify the control that it should check the state of <see cref="CanExecute" />.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        try
        {
            log.Trace()
                .Message("AsyncRelayCommand {name} RaiseCanExecuteChanged", Name)
                .Property("relayCommand", this)
                .Write();
            dispatcher.Post(OnCanExecuteChanged);
        }
        catch (Exception e)
        {
            log.Error()
                .Exception(e)
                .Message("AsyncRelayCommand {name} RaiseCanExecuteChanged exception: {message}", Name, e.Message)
                .Write();
        }
    }

    public bool CanExecute(object? parameter)
    {
        if (Interlocked.Read(ref isExecuting) != 0)
            return false;

        try
        {
            var canExecute = canExecuteQuery((TParam)parameter!);
            log.Trace()
                .Message("RelayCommand {name} CanExecute = {canExecute}", Name, canExecute)
                .Property(nameof(parameter), parameter)
                .Property("relayCommand", this)
                .Write();
            return canExecute;
        }
        catch (Exception e)
        {
            log.Error()
                .Exception(e)
                .Message("RelayCommand {name} CanExecute exception: {message}", Name, e.Message)
                .Write();
        }

        return false;
    }

    /// <summary>
    ///     Execute the command asynchronously.
    ///     Note: the command may run on a different thread; be careful about cross-thread control updates.
    /// </summary>
    /// <param name="parameter">Not used.</param>
    public async void Execute(object? parameter)
    {
        try
        {
            Interlocked.Exchange(ref isExecuting, 1);
            RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(IsRunning));
            log.Trace()
                .Message("AsyncRelayCommand {name} Executing", Name)
                .Property(nameof(parameter), parameter)
                .Property("relayCommand", this)
                .Write();
            await execute((TParam)parameter!).ContinueInCurrentContext();
        }
        // We are starting a fire-and-forget task, so we MUST handle any and all exceptions to avoid application crashes.
        catch (Exception ex)
        {
            log.Error()
                .Exception(ex)
                .Message("AsyncRelayCommand {name} Execute exception: {message}", Name, ex.Message)
                .Write();
        }
        finally
        {
            Interlocked.Exchange(ref isExecuting, 0);
            OnPropertyChanged(nameof(IsRunning));
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    ///   Raised when a property value changes. Required for the IsRunning property, so that UI can bind to it and update accordingly.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        try
        {
            dispatcher.Post(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
        catch (Exception e)
        {
            log.Error()
                .Exception(e)
                .Message("AsyncRelayCommand {name} OnPropertyChanged {propertyName} exception: {message}",
                    Name, propertyName, e.Message)
                .Write();
        }
    }
}

public class AsyncRelayCommand : AsyncRelayCommand<object>
{
    /// <summary>
    ///     A wrapper around <see cref="AsyncRelayCommand{TParam}" /> that just ignores the parameter.
    /// </summary>
    /// <param name="execute">An async method with no parameters.</param>
    /// <param name="canExecute">
    ///     A <see cref="Func{bool}" /> with no parameters.
    ///     Note: unlike some implementations of RelayCommand, the CanExecute method must be supplied.
    ///     If there is no suitable method, use <c>()=>true</c>.
    /// </param>
    /// <param name="name">Display name for diagnostics.</param>
    /// <param name="log">An optional logging service.</param>
    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null, string? name = null,
        ILog? log = null) :
        base(
            async obj => await execute(),
            obj => canExecute?.Invoke() ?? true,
            name,
            log) { }
}
