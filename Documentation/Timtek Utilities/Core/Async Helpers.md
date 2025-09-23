# Asynchrony and Threading

## ConfigureAwait

There is an extension method in .NET used to configure awaitable tasks, called `ConfigureAwait(bool)`.
The method affects how the task awaiter schedules its continuation.
With `ConfigureAwait(true)` the task continues on the current synchronization context.
That usually means on the same thread, and is particularly relevant when the awaiter is a user interface thread.
Conversely, `ConfigureAwait(false)` means that continuation can happen on any thread, 
and typically that will be a thread pool worker thread.
The implications are quite profound, especially for apartment-threaded GUI applications such as WinForms or WPF. 
Consider the following method:
```csharp
public async Task SomeMethod()
    {
	Console.WriteLine("Starting on thread {0}", Thread.CurrentThread.ManagedThreadId);
	await Task.Delay(1000).ConfigureAwait(false);
	Console.WriteLine("Continuing on thread {0}", Thread.CurrentThread.ManagedThreadId);
    }
```
When you run this, you may get something like

> Starting on thread 14  
> Continuing on thread 11

But it is not at all ovious how `ConfigureAwait()` should be used.
What if you don't specifiy?
Is the await configured or unconfigured?
Does `ConfigureAwait(false)` mean you don't want to configure it, or that you want to configure it not to do something?
It's just horrible.
You can't read the code and instantly understand what it does, and that violates the _Principle of Least Astonishment_.

So we made some extension methods that essentially do the same thing, but make more sense. Our aync method now becomes:
```csharp
public async Task SomeMethod()
    {
	Console.WriteLine("Starting on thread {0}", Thread.CurrentThread.ManagedThreadId);
	await Task.Delay(1000).ContinueOnAnyThread();
	Console.WriteLine("Continuing on thread {0}", Thread.CurrentThread.ManagedThreadId);
    }
```
and we get

> Starting on thread 15  
> Continuing on thread 13

Alternatively:
```csharp
public async Task SomeMethod()
    {
	Console.WriteLine("Starting on thread {0}", Thread.CurrentThread.ManagedThreadId);
	await Task.Delay(1000).ContinueInCurrentContext();
	Console.WriteLine("Continuing on thread {0}", Thread.CurrentThread.ManagedThreadId);
    }
```
The await captures the current `SynchronizationContext` and uses it to schedule the continuation.
What happens next depends on the application model and how it implements `SynchronizationContext`.
For a user interface application, the UI generally runs in a Single Threaded Apartment (STA thread).
In this model, asynchronous operations are posted to the message queue of the STA thread.
The continuation will then happen on the UI thread once the thread is idle and the message pump runs.
In a free-threaded application model such as a console application, the continuation will likely
still happen on a different thread.

Here you can see the danger of this option.
If the continuation is queued in the message queue waiting for messages to be pumped, but the UI is blocked waiting for the task to complete, then the continuation may never get to run.
The task is prevented from completing and we are in deadlock.
Therefore, best practice for library writers is to always use `ContinueOnAnyThread()`.

## Cancel Culture

One final extension method is `Task.WithCancellation(token)`.
This takes a task that is not cancellable and wraps it in a cancellable task.
Awaiters can then wait on the cancellable wrapper and will get to run if the wrapper is cancelled.
Note that this doesn't stop the original task from running and it may still run to completion,
but its result will be discarded as there should be nothing awaiting the result.
