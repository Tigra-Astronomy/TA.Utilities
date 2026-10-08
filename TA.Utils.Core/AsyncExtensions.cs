// This file is part of the TA.Utils project
// Copyright © 2016-2023 Timtek Systems Limited, all rights reserved.
// File: AsyncExtensions.cs  Last modified: 2023-08-14@01:28 by Tim Long

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TA.Utils.Core;

/// <summary>Helper methods for manipulating strings of ASCII-encoded text.</summary>
public static class AsyncExtensions
{
    /// <summary>
    ///     Adds cancellation to a task that was otherwise not cancellable.
    ///     Note: when cancelled, the underlying task will still execute to completion, but any awaiters will no longer have to
    ///     wait for it. This should not be used as an alternative for implementing cooperative cancellation in your own tasks.
    ///     It is a method-of-last-resort for tasks you didn't write which are not cancellable.
    /// </summary>
    /// <typeparam name="T">The type of result to be returned by the task.</typeparam>
    /// <param name="task">The uncancellable task.</param>
    /// <param name="cancellationToken">The cancellation token that can be used to cancel the task.</param>
    /// <returns>Task{T}.</returns>
    /// <exception cref="T:System.OperationCanceledException">
    ///     Thrown if cancellation occurs before the
    ///     underlying task completes.
    /// </exception>
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
        {
            if (task != await Task.WhenAny(task, tcs.Task))
                throw new OperationCanceledException(cancellationToken);
        }

        // ReSharper disable once AsyncApostle.AsyncWait
        return task.Result;
    }

    /// <summary>
    ///     Configures a task awaiter to schedule its completion on any available thread. Use this when awaiting
    ///     tasks in a user interface thread to avoid deadlock issues.
    ///     This is the recommended best practice for general purpose library writers.
    ///     There is no need to post library internal async operations back to the UI thread,
    ///     and doing so could potentially lead to a deadlock if the UI thread is blocked.
    /// </summary>
    /// <param name="task">The task to configure.</param>
    /// <returns>An awaitable object that may schedule continuation on any thread.</returns>
    /// <remarks>
    ///     This extension method is exactly equivalent to <c>Task.ConfigureAwait(false);</c> but (we think) more
    ///     meaningful.
    /// </remarks>
    public static ConfiguredTaskAwaitable<TResult> ContinueOnAnyThread<TResult>(this Task<TResult> task) =>
        task.ConfigureAwait(false);

    /// <summary>
    ///     Configures a task awaiter to schedule its completion on any available thread. Use this when awaiting
    ///     tasks in a user interface thread to avoid deadlock issues.
    ///     This is the recommended best practice for general purpose library writers.
    ///     There is no need to post library internal async operations back to the UI thread,
    ///     and doing so could potentially lead to a deadlock if the UI thread is blocked.
    /// </summary>
    /// <param name="task">The task to configure.</param>
    /// <returns>An awaitable object that may schedule continuation on any thread.</returns>
    /// <remarks>
    ///     This extension method is exactly equivalent to <c>Task.ConfigureAwait(false);</c> but (we think) more
    ///     meaningful.
    /// </remarks>
    public static ConfiguredTaskAwaitable ContinueOnAnyThread(this Task task) => task.ConfigureAwait(false);

    /// <summary>
    ///     Configures a task awaiter to schedule continuation on the captured synchronization context.
    ///     That is, the continuation should execute on the same thread that created the task. This can be
    ///     risky when the awaiter is a single threaded apartment (STA) thread, such as the user interface
    ///     thread. If the awaiter blocks waiting for the task, then the continuation may never execute,
    ///     resulting in deadlock. Use with care.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <returns>ConfiguredTaskAwaitable.</returns>
    /// <seealso cref="ContinueOnAnyThread(Task)" />
    [Obsolete("Use ContinueInCurrentContext() instead", true)]
    public static ConfiguredTaskAwaitable ContinueOnCurrentThread(this Task task) => task.ConfigureAwait(true);

    /// <summary>
    ///     Configures a task awaiter to schedule continuation on the captured synchronization context.
    ///     What happens next depends on the current synchronization context.
    ///     In a Single Threaded Apartment (STA thread) such as a UI thread, the continuation should
    ///     execute on the same thread. However, in a free threaded context, the continuation can
    ///     still happen on a different thread. Use caution when the awaiter is a single
    ///     threaded apartment (STA) thread. If the awaiter blocks waiting for the task, then
    ///     the continuation may never execute, preventing completion and resulting in deadlock.
    ///     Use with care, especially in general purpose libraries.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <returns>A <see cref="ConfiguredTaskAwaitable" /> that continues on the captured synchronization context.</returns>
    /// <seealso cref="ContinueOnAnyThread(Task)" />
    /// <remarks>
    ///     This extension method is exactly equivalent to using <c>Task.ConfigureAwait(true);</c>
    ///     but is (we think) more meaningful than a boolean flag.
    /// </remarks>
    public static ConfiguredTaskAwaitable ContinueInCurrentContext(this Task task) => task.ConfigureAwait(true);

    /// <summary>
    ///     Configures a task awaiter to schedule continuation on the captured synchronization context.
    ///     What happens next depends on the current synchronization context.
    ///     In a Single Threaded Apartment (STA thread) such as a UI thread, the continuation should
    ///     execute on the same thread. However, in a free threaded context, the continuation can
    ///     still happen on a different thread. Use caution when the awaiter is a single
    ///     threaded apartment (STA) thread. If the awaiter blocks waiting for the task, then
    ///     the continuation may never execute, preventing completion and resulting in deadlock.
    ///     Use with care, especially in general purpose libraries.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <returns>A <see cref="ConfiguredTaskAwaitable{TResult}" /> that continues on the captured synchronization context.</returns>
    /// <seealso cref="ContinueOnAnyThread{TResult}(Task{TResult})" />
    /// <remarks>
    ///     This extension method is exactly equivalent to using <c>Task.ConfigureAwait(true);</c>
    ///     but is (we think) more meaningful than a boolean flag.
    /// </remarks>
    public static ConfiguredTaskAwaitable<TResult> ContinueInCurrentContext<TResult>(this Task<TResult> task) =>
        task.ConfigureAwait(true);

#if NET8_0_OR_GREATER
    /// <summary>Configures a value task awaiter to continue without capturing the synchronization context.</summary>
    /// <param name="task">The value task to configure.</param>
    /// <returns>An awaitable object that may schedule continuation on any thread.</returns>
    public static ConfiguredValueTaskAwaitable ContinueOnAnyThread(this ValueTask task) =>
        task.ConfigureAwait(false);

    /// <summary>Configures a value task awaiter to continue on the captured synchronization context.</summary>
    /// <param name="task">The value task to configure.</param>
    /// <returns>An awaitable object that continues on the captured synchronization context.</returns>
    public static ConfiguredValueTaskAwaitable ContinueInCurrentContext(this ValueTask task) =>
        task.ConfigureAwait(true);

    /// <summary>Configures a value task awaiter to continue without capturing the synchronization context.</summary>
    /// <typeparam name="TResult">The type of the task result.</typeparam>
    /// <param name="task">The value task to configure.</param>
    /// <returns>An awaitable object that may schedule continuation on any thread.</returns>
    public static ConfiguredValueTaskAwaitable<TResult> ContinueOnAnyThread<TResult>(this ValueTask<TResult> task) =>
        task.ConfigureAwait(false);

    /// <summary>Configures a value task awaiter to continue on the captured synchronization context.</summary>
    /// <typeparam name="TResult">The type of the task result.</typeparam>
    /// <param name="task">The value task to configure.</param>
    /// <returns>An awaitable object that continues on the captured synchronization context.</returns>
    public static ConfiguredValueTaskAwaitable<TResult> ContinueInCurrentContext<TResult>(this ValueTask<TResult> task) =>
        task.ConfigureAwait(true);

    /// <summary>Configures asynchronous disposal to continue without capturing the synchronization context.</summary>
    /// <param name="source">The resource to dispose asynchronously.</param>
    /// <returns>A disposable object for use with <c>await using</c> that may continue on any thread.</returns>
    public static ConfiguredAsyncDisposable ContinueOnAnyThread(this IAsyncDisposable source) =>
        source.ConfigureAwait(false);

    /// <summary>Configures asynchronous disposal to continue on the captured synchronization context.</summary>
    /// <param name="source">The resource to dispose asynchronously.</param>
    /// <returns>A disposable object for use with <c>await using</c> that continues on the captured context.</returns>
    public static ConfiguredAsyncDisposable ContinueInCurrentContext(this IAsyncDisposable source) =>
        source.ConfigureAwait(true);

    /// <summary>Configures asynchronous enumeration to continue without capturing the synchronization context.</summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to configure.</param>
    /// <returns>An enumerable for use with <c>await foreach</c> that may continue on any thread.</returns>
    public static ConfiguredCancelableAsyncEnumerable<T> ContinueOnAnyThread<T>(this IAsyncEnumerable<T> source) =>
        source.ConfigureAwait(false);

    /// <summary>Configures asynchronous enumeration to continue on the captured synchronization context.</summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to configure.</param>
    /// <returns>An enumerable for use with <c>await foreach</c> that continues on the captured context.</returns>
    public static ConfiguredCancelableAsyncEnumerable<T> ContinueInCurrentContext<T>(this IAsyncEnumerable<T> source) =>
        source.ConfigureAwait(true);
#endif

    /// <summary>
    ///     Executes a task synchronously and unwraps any <see cref="AggregateException" /> to throw the inner exception
    ///     directly.
    /// </summary>
    /// <param name="task">The task to execute synchronously.</param>
    public static void WaitForCompletion(this Task task)
    {
        try
        {
            task.GetAwaiter().GetResult();
        }
        catch (AggregateException ex)
        {
            throw ex.InnerException ?? ex; // Re-throw the inner exception if available
        }
    }

    /// <summary>
    ///     Executes a task synchronously and returns the result value.
    ///     The result is awaited in a way that would lead to any exceptions being thrown directly, without wrapping in an
    ///     <see cref="AggregateException" /> shouyld not normally occur, buyt if it does it is caught and unwrapped.Unwraps
    ///     any <see cref="AggregateException" />.. However, if an <see cref="AggregateException" /> somehow occurs, then it is
    ///     unwrapped and the first inner exception is re-thrown.
    /// </summary>
    /// <typeparam name="TResult">The type of the task result.</typeparam>
    /// <param name="task">The task to execute synchronously.</param>
    /// <returns>The result of the task.</returns>
    public static TResult WaitForResult<TResult>(this Task<TResult> task)
    {
        try
        {
            return task.GetAwaiter().GetResult();
        }
        catch (AggregateException ex)
        {
            throw ex.InnerException ?? ex; // Re-throw the inner exception if available
        }
    }
}