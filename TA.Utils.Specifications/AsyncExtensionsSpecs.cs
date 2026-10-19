#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications
{
    [Subject(typeof(AsyncExtensions), "Continuation configuration")]
    class when_configuring_async_continuations
    {
        It should_not_capture_context_for_value_tasks = () =>
            VerifyContinuation(async task =>
            {
                var valueTask = new ValueTask(task);
                await valueTask.ContinueOnAnyThread();
            }, false);

        It should_capture_context_for_value_tasks = () =>
            VerifyContinuation(async task =>
            {
                var valueTask = new ValueTask(task);
                await valueTask.ContinueInCurrentContext();
            }, true);

        It should_not_capture_context_for_value_tasks_with_results = () =>
            VerifyContinuation(async task =>
            {
                var valueTask = new ValueTask<bool>(task);
                var result = await valueTask.ContinueOnAnyThread();
                result.ShouldBeTrue();
            }, false);

        It should_capture_context_for_value_tasks_with_results = () =>
            VerifyContinuation(async task =>
            {
                var valueTask = new ValueTask<bool>(task);
                var result = await valueTask.ContinueInCurrentContext();
                result.ShouldBeTrue();
            }, true);

        It should_not_capture_context_for_async_disposal = () =>
            VerifyContinuation(async task =>
            {
                var resource = new AsyncResource(task);
                await using (resource.ContinueOnAnyThread()) { }
                resource.DisposeCount.ShouldEqual(1);
            }, false);

        It should_capture_context_for_async_disposal = () =>
            VerifyContinuation(async task =>
            {
                var resource = new AsyncResource(task);
                await using (resource.ContinueInCurrentContext()) { }
                resource.DisposeCount.ShouldEqual(1);
            }, true);

        It should_not_capture_context_for_async_enumeration = () =>
            VerifyContinuation(async task =>
            {
                var sequence = new AsyncSequence(task);
                var items = new List<int>();
                await foreach (var item in sequence.ContinueOnAnyThread())
                    items.Add(item);
                items.ShouldContainOnly(42);
                sequence.DisposeCount.ShouldEqual(1);
            }, false);

        It should_capture_context_for_async_enumeration = () =>
            VerifyContinuation(async task =>
            {
                var sequence = new AsyncSequence(task);
                var items = new List<int>();
                await foreach (var item in sequence.ContinueInCurrentContext())
                    items.Add(item);
                items.ShouldContainOnly(42);
                sequence.DisposeCount.ShouldEqual(1);
            }, true);

        It should_not_capture_context_for_async_enumerator_disposal = () =>
            VerifyContinuation(async task =>
            {
                var sequence = new AsyncSequence(Task.FromResult(true), task);
                await foreach (var item in sequence.ContinueOnAnyThread())
                    item.ShouldEqual(42);
                sequence.DisposeCount.ShouldEqual(1);
            }, false);

        It should_capture_context_for_async_enumerator_disposal = () =>
            VerifyContinuation(async task =>
            {
                var sequence = new AsyncSequence(Task.FromResult(true), task);
                await foreach (var item in sequence.ContinueInCurrentContext())
                    item.ShouldEqual(42);
                sequence.DisposeCount.ShouldEqual(1);
            }, true);

        It should_preserve_enumeration_cancellation = () =>
        {
            using var cancellation = new CancellationTokenSource();
            var sequence = new AsyncSequence(Task.FromResult(true));
            var configured = sequence.ContinueOnAnyThread().WithCancellation(cancellation.Token);
            var enumerator = configured.GetAsyncEnumerator();
            cancellation.Cancel();
            var exception = Catch.Exception(() => enumerator.MoveNextAsync().GetAwaiter().GetResult());
            exception.ShouldBeOfExactType<OperationCanceledException>();
            sequence.CancellationToken.ShouldEqual(cancellation.Token);
            enumerator.DisposeAsync().GetAwaiter().GetResult();
            sequence.DisposeCount.ShouldEqual(1);
        };

        It should_preserve_completed_value_task_results = () =>
        {
            var valueTask = new ValueTask<int>(42);
            var anyThreadResult = valueTask.ContinueOnAnyThread().GetAwaiter().GetResult();
            var currentContextResult = valueTask.ContinueInCurrentContext().GetAwaiter().GetResult();
            anyThreadResult.ShouldEqual(42);
            currentContextResult.ShouldEqual(42);
        };

        It should_preserve_value_task_exceptions = () =>
        {
            var failure = new InvalidOperationException("Expected failure");
            var task = Task.FromException<bool>(failure);
            var valueTask = new ValueTask<bool>(task);
            var anyThreadException = Catch.Exception(() => valueTask.ContinueOnAnyThread().GetAwaiter().GetResult());
            var currentContextException = Catch.Exception(() => valueTask.ContinueInCurrentContext().GetAwaiter().GetResult());
            anyThreadException.ShouldBeTheSameAs(failure);
            currentContextException.ShouldBeTheSameAs(failure);
        };

        It should_preserve_value_task_cancellation = () =>
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var task = Task.FromCanceled(cancellation.Token);
            var valueTask = new ValueTask(task);
            var anyThreadException = Catch.Exception(() => valueTask.ContinueOnAnyThread().GetAwaiter().GetResult());
            var currentContextException = Catch.Exception(() => valueTask.ContinueInCurrentContext().GetAwaiter().GetResult());
            anyThreadException.ShouldBeOfExactType<TaskCanceledException>();
            currentContextException.ShouldBeOfExactType<TaskCanceledException>();
        };

        static void VerifyContinuation(Func<Task<bool>, Task> operation, bool captureContext)
        {
            var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var context = new RecordingSynchronizationContext();
            var previousContext = SynchronizationContext.Current;
            Task pending;
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                pending = operation(completion.Task);
                pending.IsCompleted.ShouldBeFalse();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }

            completion.SetResult(true);
            pending.Wait(TimeSpan.FromSeconds(5)).ShouldBeTrue();
            context.PostCount.ShouldEqual(captureContext ? 1 : 0);
        }

        sealed class RecordingSynchronizationContext : SynchronizationContext
        {
            public int PostCount;

            public override void Post(SendOrPostCallback callback, object state)
            {
                Interlocked.Increment(ref PostCount);
                ThreadPool.QueueUserWorkItem(_ => callback(state));
            }
        }

        sealed class AsyncResource : IAsyncDisposable
        {
            readonly Task completion;
            public int DisposeCount;

            public AsyncResource(Task completion) => this.completion = completion;

            public ValueTask DisposeAsync()
            {
                ++DisposeCount;
                return new ValueTask(completion);
            }
        }

        sealed class AsyncSequence : IAsyncEnumerable<int>
        {
            readonly Task<bool> completion;
            readonly Task disposal;
            public int DisposeCount;
            public CancellationToken CancellationToken;

            public AsyncSequence(Task<bool> completion, Task disposal = null)
            {
                this.completion = completion;
                this.disposal = disposal ?? Task.CompletedTask;
            }

            public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                CancellationToken = cancellationToken;
                return new Enumerator(this);
            }

            sealed class Enumerator : IAsyncEnumerator<int>
            {
                readonly AsyncSequence sequence;
                bool moved;

                public Enumerator(AsyncSequence sequence) => this.sequence = sequence;

                public int Current => 42;

                public ValueTask<bool> MoveNextAsync()
                {
                    sequence.CancellationToken.ThrowIfCancellationRequested();
                    if (moved)
                        return new ValueTask<bool>(false);
                    moved = true;
                    return new ValueTask<bool>(sequence.completion);
                }

                public ValueTask DisposeAsync()
                {
                    ++sequence.DisposeCount;
                    return new ValueTask(sequence.disposal);
                }
            }
        }
    }
}
#endif
