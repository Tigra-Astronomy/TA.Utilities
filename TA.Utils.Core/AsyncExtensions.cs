// This file is part of the TA.Utils project
// Copyright © 2016-2023 Timtek Systems Limited, all rights reserved.
// File: AsyncExtensions.cs  Last modified: 2023-08-14@01:28 by Tim Long

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TA.Utils.Core
    {
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
            public static ConfiguredTaskAwaitable<TResult> ContinueOnAnyThread<TResult>(this Task<TResult> task)
            {
            return task.ConfigureAwait(false);
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
            public static ConfiguredTaskAwaitable ContinueOnAnyThread(this Task task)
            {
            return task.ConfigureAwait(false);
            }

        /// <summary>
        ///     Configures a task awaiter to schedule continuation on the captured synchronization context.
        ///     That is, the continuation should execute on the same thread that created the task. This can be
        ///     risky when the awaiter is a single threaded apartment (STA) thread, such as the user interface
        ///     thread. If the awaiter blocks waiting for the task, then the continuation may never execute,
        ///     resulting in deadlock. Use with care.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>ConfiguredTaskAwaitable.</returns>
        /// <seealso cref="ContinueOnAnyThread" />
        [Obsolete("Use ContinueInCurrentContext() instead", true)]
        public static ConfiguredTaskAwaitable ContinueOnCurrentThread(this Task task)
            {
            return task.ConfigureAwait(true);
            }

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
            /// <seealso cref="ContinueOnAnyThread" />
            /// <remarks>
            ///     This extension method is exactly equivalent to using <c>Task.ConfigureAwait(true);</c>
            ///     but is (we think) more meaningful than a boolean flag.
            /// </remarks>
            public static ConfiguredTaskAwaitable ContinueInCurrentContext(this Task task)
            {
            return task.ConfigureAwait(true);
            }

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
            /// <seealso cref="ContinueOnAnyThread" />
            /// <remarks>
            ///     This extension method is exactly equivalent to using <c>Task.ConfigureAwait(true);</c>
            ///     but is (we think) more meaningful than a boolean flag.
            /// </remarks>
            public static ConfiguredTaskAwaitable<TResult> ContinueInCurrentContext<TResult>(this Task<TResult> task) =>
                task.ConfigureAwait(true);
        }
    }