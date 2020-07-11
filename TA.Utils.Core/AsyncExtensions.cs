// This file is part of the TA.NexDome.AscomServer project
// Copyright © 2019-2019 Tigra Astronomy, all rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TA.Utils.Core
    {
    public static class AsyncExtensions
        {
        /// <summary>
        /// Adds cancellation to a task that was otherwise not cancellable.
        /// Note: the underlying task may still execute to completion.
        /// </summary>
        /// <typeparam name="T">The type of result to be returned by the task.</typeparam>
        /// <param name="task">The uncancellable task.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the task.</param>
        /// <returns>Task{T}.</returns>
        /// <exception cref="T:System.OperationCanceledException">Thrown if cancellation occurs before the underlying task completes.</exception>
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
            {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            return task.Result;
            }

        /// <summary>
        /// Configures a task to schedule its completion on any available thread.
        /// Use this when awaiting tasks in a user interface thread to avoid deadlock issues.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="task">The task to configure.</param>
        /// <returns>An awaitable object that may schedule continuation on any thread.</returns>
        public static ConfiguredTaskAwaitable<TResult> ContinueOnAnyThread<TResult>(this Task<TResult> task)
            {
            return task.ConfigureAwait(continueOnCapturedContext: false);
            }

        /// <summary>
        /// Configures a task to schedule its completion on any available thread.
        /// Use this when awaiting tasks in a user interface thread to avoid deadlock issues.
        /// </summary>
        /// <param name="task">The task to configure.</param>
        /// <returns>An awaitable object that may schedule continuation on any thread.</returns>
        public static ConfiguredTaskAwaitable ContinueOnAnyThread(this Task task)
            {
            return task.ConfigureAwait(continueOnCapturedContext: false);
            }

        /// <summary>
        /// Configures a task awaiter to schedule continuation on the captured synchronization context.
        /// That is, the continuation should execute on the same thread that created the task.
        /// This can be risky when the awaiter is a single threaded apartment (STA) thread, such as
        /// the user interface thread. If the awaiter blocks waiting for the task, then the
        /// continuation may never execute, resulting in deadlock. Use with care.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>ConfiguredTaskAwaitable.</returns>
        /// <seealso cref="ContinueOnAnyThread"/>
        public static ConfiguredTaskAwaitable ContinueOnCurrentThread(this Task task)
            {
            return task.ConfigureAwait(continueOnCapturedContext: true);
            }
        }
    }