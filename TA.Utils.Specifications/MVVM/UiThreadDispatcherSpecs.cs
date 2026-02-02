// This file is part of the TA.Utils project
// Copyright © 2025 Tigra Astronomy, all rights reserved.

using System.Threading;
using Machine.Specifications;
using TA.Utils.Core.MVVM;

namespace TA.Utils.Specifications.MVVM
{
    [Subject(typeof(CurrentThreadDispatcher), "Execution")]
    class when_posting_an_action_to_current_thread_dispatcher
    {
        Establish context = () =>
        {
            dispatcher = new CurrentThreadDispatcher();
            executed = false;
        };

        Because of = () => dispatcher.Post(() => executed = true);

        It should_execute_the_action = () => executed.ShouldBeTrue();

        static CurrentThreadDispatcher dispatcher;
        static bool executed;
    }

    [Subject(typeof(CurrentThreadDispatcher), "Execution")]
    class when_posting_an_action_with_result_to_current_thread_dispatcher
    {
        Establish context = () =>
        {
            dispatcher = new CurrentThreadDispatcher();
            executionThreadId = 0;
            currentThreadId = Thread.CurrentThread.ManagedThreadId;
        };

        Because of = () => dispatcher.Post(() => executionThreadId = Thread.CurrentThread.ManagedThreadId);

        It should_execute_on_the_same_thread = () => executionThreadId.ShouldEqual(currentThreadId);

        static CurrentThreadDispatcher dispatcher;
        static int executionThreadId;
        static int currentThreadId;
    }

    [Subject(typeof(UiThreadDispatcherContext), "Context Management")]
    class when_getting_current_dispatcher_without_setting_one
    {
        Because of = () => dispatcher = UiThreadDispatcherContext.Current;

        It should_return_a_current_thread_dispatcher = () => dispatcher.ShouldBeOfExactType<CurrentThreadDispatcher>();

        Cleanup after = () => UiThreadDispatcherContext.SetDispatcher(null);

        static IUiThreadDispatcher dispatcher;
    }

    [Subject(typeof(UiThreadDispatcherContext), "Context Management")]
    class when_setting_a_custom_dispatcher
    {
        Establish context = () => customDispatcher = new CurrentThreadDispatcher();

        Because of = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(customDispatcher);
            retrievedDispatcher = UiThreadDispatcherContext.Current;
        };

        It should_return_the_custom_dispatcher = () => retrievedDispatcher.ShouldBeTheSameAs(customDispatcher);

        Cleanup after = () => UiThreadDispatcherContext.SetDispatcher(null);

        static CurrentThreadDispatcher customDispatcher;
        static IUiThreadDispatcher retrievedDispatcher;
    }

    [Subject(typeof(UiThreadDispatcherContext), "Context Management")]
    class when_clearing_the_dispatcher
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
        };

        Because of = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(null);
            dispatcher = UiThreadDispatcherContext.Current;
        };

        It should_return_a_new_default_dispatcher = () => dispatcher.ShouldBeOfExactType<CurrentThreadDispatcher>();

        Cleanup after = () => UiThreadDispatcherContext.SetDispatcher(null);

        static IUiThreadDispatcher dispatcher;
    }

    [Subject(typeof(UiThreadDispatcherContext), "Thread Safety")]
    class when_dispatcher_is_set_on_different_threads
    {
        Establish context = () =>
        {
            dispatcher1 = new CurrentThreadDispatcher();
            dispatcher2 = new CurrentThreadDispatcher();
        };

        Because of = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(dispatcher1);
            retrievedOnMainThread = UiThreadDispatcherContext.Current;

            var thread = new Thread(() =>
            {
                UiThreadDispatcherContext.SetDispatcher(dispatcher2);
                retrievedOnOtherThread = UiThreadDispatcherContext.Current;
            });
            thread.Start();
            thread.Join();
        };

        It should_maintain_separate_dispatchers_per_thread = () =>
        {
            retrievedOnMainThread.ShouldBeTheSameAs(dispatcher1);
            retrievedOnOtherThread.ShouldBeTheSameAs(dispatcher2);
        };

        Cleanup after = () => UiThreadDispatcherContext.SetDispatcher(null);

        static CurrentThreadDispatcher dispatcher1;
        static CurrentThreadDispatcher dispatcher2;
        static IUiThreadDispatcher retrievedOnMainThread;
        static IUiThreadDispatcher retrievedOnOtherThread;
    }
}
