// This file is part of the TA.Utils project
// Copyright © 2025 Tigra Astronomy, all rights reserved.

using System;
using System.Threading.Tasks;
using Machine.Specifications;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Core.MVVM;

namespace TA.Utils.Specifications.MVVM
{
    [Subject(typeof(AsyncRelayCommand), "Construction")]
    class when_creating_an_async_relay_command
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executeAction = () => Task.CompletedTask;
        };

        Because of = () => command = new AsyncRelayCommand(executeAction);

        It should_not_be_null = () => command.ShouldNotBeNull();
        It should_have_default_name = () => command.Name.ShouldEqual("unnamed");
        It should_be_executable = () => command.CanExecute(null).ShouldBeTrue();

        static AsyncRelayCommand command;
        static Func<Task> executeAction;
    }

    [Subject(typeof(AsyncRelayCommand), "Construction")]
    class when_creating_an_async_relay_command_with_all_parameters
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executeAction = () => Task.CompletedTask;
            canExecuteQuery = () => true;
            log = new DegenerateLoggerService();
        };

        Because of = () => command = new AsyncRelayCommand(executeAction, canExecuteQuery, "AsyncTestCommand", log);

        It should_have_specified_name = () => command.Name.ShouldEqual("AsyncTestCommand");

        static AsyncRelayCommand command;
        static Func<Task> executeAction;
        static Func<bool> canExecuteQuery;
        static ILog log;
    }

    [Subject(typeof(AsyncRelayCommand), "Execution")]
    class when_executing_an_async_relay_command
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executed = false;
            executeAction = async () =>
            {
                await Task.Delay(10);
                executed = true;
            };
            command = new AsyncRelayCommand(executeAction);
        };

        Because of = () =>
        {
            command.Execute(null);
            // Give async execution time to complete
            Task.Delay(100).Wait();
        };

        It should_invoke_the_execute_action = () => executed.ShouldBeTrue();

        static AsyncRelayCommand command;
        static Func<Task> executeAction;
        static bool executed;
    }

    [Subject(typeof(AsyncRelayCommand), "Execution")]
    class when_async_command_is_executing
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            tcs = new TaskCompletionSource<bool>();
            executeAction = () => tcs.Task;
            command = new AsyncRelayCommand(executeAction);
        };

        Because of = () =>
        {
            command.Execute(null);
            canExecuteWhileRunning = command.CanExecute(null);
        };

        It should_not_be_executable = () => canExecuteWhileRunning.ShouldBeFalse();

        Cleanup after = () => tcs.TrySetResult(true);

        static AsyncRelayCommand command;
        static Func<Task> executeAction;
        static TaskCompletionSource<bool> tcs;
        static bool canExecuteWhileRunning;
    }

    [Subject(typeof(AsyncRelayCommand), "Execution")]
    class when_async_command_completes_execution
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executeAction = () => Task.CompletedTask;
            command = new AsyncRelayCommand(executeAction);
        };

        Because of = () =>
        {
            command.Execute(null);
            Task.Delay(100).Wait();
            canExecuteAfterCompletion = command.CanExecute(null);
        };

        It should_be_executable_again = () => canExecuteAfterCompletion.ShouldBeTrue();

        static AsyncRelayCommand command;
        static Func<Task> executeAction;
        static bool canExecuteAfterCompletion;
    }

    [Subject(typeof(AsyncRelayCommand), "Execution")]
    class when_executing_an_async_command_that_throws
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executeAction = () => Task.FromException(new InvalidOperationException("Test exception"));
            command = new AsyncRelayCommand(executeAction);
        };

        Because of = () =>
        {
            exception = Catch.Exception(() => command.Execute(null));
            Task.Delay(100).Wait();
        };

        It should_not_propagate_the_exception = () => exception.ShouldBeNull();
        It should_be_executable_again = () => command.CanExecute(null).ShouldBeTrue();

        static AsyncRelayCommand command;
        static Func<Task> executeAction;
        static Exception exception;
    }

    [Subject(typeof(AsyncRelayCommand), "CanExecuteChanged")]
    class when_async_command_starts_executing
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            eventRaisedCount = 0;
            tcs = new TaskCompletionSource<bool>();
            executeAction = () => tcs.Task;
            command = new AsyncRelayCommand(executeAction);
            command.CanExecuteChanged += (sender, args) => eventRaisedCount++;
        };

        Because of = () =>
        {
            command.Execute(null);
            Task.Delay(50).Wait();
        };

        It should_raise_can_execute_changed_at_least_once = () => eventRaisedCount.ShouldBeGreaterThanOrEqualTo(1);

        Cleanup after = () => tcs.TrySetResult(true);

        static AsyncRelayCommand command;
        static Func<Task> executeAction;
        static TaskCompletionSource<bool> tcs;
        static int eventRaisedCount;
    }

    [Subject(typeof(AsyncRelayCommand<>), "Construction")]
    class when_creating_a_generic_async_relay_command
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executeAction = (int x) => Task.CompletedTask;
            canExecuteQuery = (int x) => true;
        };

        Because of = () => command = new AsyncRelayCommand<int>(executeAction, canExecuteQuery);

        It should_not_be_null = () => command.ShouldNotBeNull();
        It should_have_default_name = () => command.Name.ShouldEqual("unnamed");

        static AsyncRelayCommand<int> command;
        static Func<int, Task> executeAction;
        static Func<int, bool> canExecuteQuery;
    }

    [Subject(typeof(AsyncRelayCommand<>), "Execution")]
    class when_executing_a_generic_async_command_with_correct_parameter
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            receivedValue = 0;
            executeAction = async (int x) =>
            {
                await Task.Delay(10);
                receivedValue = x;
            };
            command = new AsyncRelayCommand<int>(executeAction, _ => true);
        };

        Because of = () =>
        {
            command.Execute(42);
            Task.Delay(100).Wait();
        };

        It should_invoke_the_action_with_the_parameter = () => receivedValue.ShouldEqual(42);

        static AsyncRelayCommand<int> command;
        static Func<int, Task> executeAction;
        static int receivedValue;
    }

    [Subject(typeof(AsyncRelayCommand<>), "Execution")]
    class when_generic_async_command_is_executing
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            tcs = new TaskCompletionSource<bool>();
            executeAction = (int x) => tcs.Task;
            command = new AsyncRelayCommand<int>(executeAction, _ => true);
        };

        Because of = () =>
        {
            command.Execute(10);
            canExecuteWhileRunning = command.CanExecute(10);
        };

        It should_not_be_executable = () => canExecuteWhileRunning.ShouldBeFalse();

        Cleanup after = () => tcs.TrySetResult(true);

        static AsyncRelayCommand<int> command;
        static Func<int, Task> executeAction;
        static TaskCompletionSource<bool> tcs;
        static bool canExecuteWhileRunning;
    }

    [Subject(typeof(AsyncRelayCommand<>), "CanExecute")]
    class when_checking_can_execute_on_generic_async_command_with_predicate
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            command = new AsyncRelayCommand<int>(_ => Task.CompletedTask, x => x > 0);
        };

        Because of = () => result = command.CanExecute(42);

        It should_return_true = () => result.ShouldBeTrue();

        static AsyncRelayCommand<int> command;
        static bool result;
    }

    [Subject(typeof(AsyncRelayCommand<>), "CanExecute")]
    class when_can_execute_predicate_evaluates_to_false_on_async_command
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            command = new AsyncRelayCommand<int>(_ => Task.CompletedTask, x => x > 10);
        };

        Because of = () => result = command.CanExecute(5);

        It should_return_false = () => result.ShouldBeFalse();

        static AsyncRelayCommand<int> command;
        static bool result;
    }
}
