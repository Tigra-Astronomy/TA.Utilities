// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: AsyncExtensions.cs  Last modified: 2020-07-13@02:11 by Tim Long

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
        ///     Adds cancellation to a task that was otherwise not cancellable. Note: the underlying task may
        ///     still execute to completion.
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
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>) s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            return task.Result;
            }

        /// <summary>
        ///     Configures a task to schedule its completion on any available thread. Use this when awaiting
        ///     tasks in a user interface thread to avoid deadlock issues.
        ///     This is the recommended best practice for library writers.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="task">The task to configure.</param>
        /// <returns>An awaitable object that may schedule continuation on any thread.</returns>
        public static ConfiguredTaskAwaitable<TResult> ContinueOnAnyThread<TResult>(this Task<TResult> task)
            {
            return task.ConfigureAwait(continueOnCapturedContext: false);
            }

        /// <summary>
        ///     Configures a task to schedule its completion on any available thread. Use this when awaiting
        ///     tasks in a user interface thread to avoid deadlock issues.
        ///     This is the recommended best practice for library writers.
        /// </summary>
        /// <param name="task">The task to configure.</param>
        /// <returns>An awaitable object that may schedule continuation on any thread.</returns>
        public static ConfiguredTaskAwaitable ContinueOnAnyThread(this Task task)
            {
            return task.ConfigureAwait(continueOnCapturedContext: false);
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
        [Obsolete("Use ContinueInCurrentContext() instead")]
        public static ConfiguredTaskAwaitable ContinueOnCurrentThread(this Task task)
            {
            return task.ConfigureAwait(continueOnCapturedContext: true);
            }

        /// <summary>
        ///     Configures a task awaiter to schedule continuation on the captured synchronization context.
        ///     What happens next depends on the current synchronization context.
        ///     In a Single Threaded Apartment (STA thread) such as a UI thread, the continuation should
        ///     execute on the same thread. However in a free threaded context, the continuation can
        ///     still happen on a different thread. This can be risky when the awaiter is a single
        ///     threaded apartment (STA) thread. If the awaiter blocks waiting for the task, then
        ///     the continuation may never execute, preventign completion and resulting in deadlock.
        ///     Use with care.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>ConfiguredTaskAwaitable.</returns>
        /// <seealso cref="ContinueOnAnyThread" />

        public static ConfiguredTaskAwaitable ContinueInCurrentContext(this Task task)
            {
            return task.ConfigureAwait(continueOnCapturedContext: true);
            }
        }
    }