// This file is part of the TA.Utils project
// Copyright © 2025 Tigra Astronomy, all rights reserved.

using System;
using Machine.Specifications;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Core.MVVM;

namespace TA.Utils.Specifications.MVVM
{
    [Subject(typeof(RelayCommand), "Construction")]
    class when_creating_a_relay_command_with_minimal_parameters
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executeAction = () => { };
            canExecuteQuery = () => true;
        };

        Because of = () => command = new RelayCommand(executeAction, canExecuteQuery);

        It should_not_be_null = () => command.ShouldNotBeNull();
        It should_have_default_name = () => command.Name.ShouldEqual("unnamed");
        It should_be_executable = () => command.CanExecute(null).ShouldBeTrue();

        static RelayCommand command;
        static Action executeAction;
        static Func<bool> canExecuteQuery;
    }

    [Subject(typeof(RelayCommand), "Construction")]
    class when_creating_a_relay_command_with_all_parameters
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executeAction = () => { };
            canExecuteQuery = () => true;
            log = new DegenerateLoggerService();
        };

        Because of = () => command = new RelayCommand(executeAction, canExecuteQuery, "TestCommand", log);

        It should_have_specified_name = () => command.Name.ShouldEqual("TestCommand");

        static RelayCommand command;
        static Action executeAction;
        static Func<bool> canExecuteQuery;
        static ILog log;
    }

    [Subject(typeof(RelayCommand), "Construction")]
    class when_creating_a_relay_command_with_null_can_execute
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executeAction = () => { };
        };

        Because of = () => command = new RelayCommand(executeAction, null);

        It should_always_be_executable = () => command.CanExecute(null).ShouldBeTrue();

        static RelayCommand command;
        static Action executeAction;
    }

    [Subject(typeof(RelayCommand), "Execution")]
    class when_executing_a_relay_command
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executed = false;
            executeAction = () => executed = true;
            command = new RelayCommand(executeAction, () => true);
        };

        Because of = () => command.Execute(null);

        It should_invoke_the_execute_action = () => executed.ShouldBeTrue();

        static RelayCommand command;
        static Action executeAction;
        static bool executed;
    }

    [Subject(typeof(RelayCommand), "Execution")]
    class when_executing_a_relay_command_that_throws
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executeAction = () => throw new InvalidOperationException("Test exception");
            command = new RelayCommand(executeAction, () => true);
        };

        Because of = () => exception = Catch.Exception(() => command.Execute(null));

        It should_not_propagate_the_exception = () => exception.ShouldBeNull();

        static RelayCommand command;
        static Action executeAction;
        static Exception exception;
    }

    [Subject(typeof(RelayCommand), "CanExecute")]
    class when_can_execute_returns_true
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            command = new RelayCommand(() => { }, () => true);
        };

        Because of = () => result = command.CanExecute(null);

        It should_return_true = () => result.ShouldBeTrue();

        static RelayCommand command;
        static bool result;
    }

    [Subject(typeof(RelayCommand), "CanExecute")]
    class when_can_execute_returns_false
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            command = new RelayCommand(() => { }, () => false);
        };

        Because of = () => result = command.CanExecute(null);

        It should_return_false = () => result.ShouldBeFalse();

        static RelayCommand command;
        static bool result;
    }

    [Subject(typeof(RelayCommand), "CanExecute")]
    class when_can_execute_throws_exception
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            command = new RelayCommand(() => { }, () => throw new InvalidOperationException("Test exception"));
        };

        Because of = () => result = command.CanExecute(null);

        It should_return_false = () => result.ShouldBeFalse();

        static RelayCommand command;
        static bool result;
    }

    [Subject(typeof(RelayCommand), "CanExecuteChanged")]
    class when_raising_can_execute_changed
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            command = new RelayCommand(() => { }, () => true);
            eventRaised = false;
            command.CanExecuteChanged += (sender, args) => eventRaised = true;
        };

        Because of = () => command.RaiseCanExecuteChanged();

        It should_raise_the_event = () => eventRaised.ShouldBeTrue();

        static RelayCommand command;
        static bool eventRaised;
    }

    [Subject(typeof(RelayCommand<>), "Construction")]
    class when_creating_a_generic_relay_command
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executeAction = (int x) => { };
            canExecuteQuery = (int x) => true;
        };

        Because of = () => command = new RelayCommand<int>(executeAction, canExecuteQuery);

        It should_not_be_null = () => command.ShouldNotBeNull();
        It should_have_default_name = () => command.Name.ShouldEqual("unnamed");

        static RelayCommand<int> command;
        static Action<int> executeAction;
        static Func<int, bool> canExecuteQuery;
    }

    [Subject(typeof(RelayCommand<>), "Execution")]
    class when_executing_a_generic_relay_command_with_correct_parameter_type
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            receivedValue = 0;
            executeAction = (int x) => receivedValue = x;
            command = new RelayCommand<int>(executeAction, _ => true);
        };

        Because of = () => command.Execute(42);

        It should_invoke_the_action_with_the_parameter = () => receivedValue.ShouldEqual(42);

        static RelayCommand<int> command;
        static Action<int> executeAction;
        static int receivedValue;
    }

    [Subject(typeof(RelayCommand<>), "Execution")]
    class when_executing_a_generic_relay_command_with_wrong_parameter_type
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            executed = false;
            executeAction = (int x) => executed = true;
            command = new RelayCommand<int>(executeAction, _ => true);
        };

        Because of = () => command.Execute("wrong type");

        It should_not_execute_the_action = () => executed.ShouldBeFalse();

        static RelayCommand<int> command;
        static Action<int> executeAction;
        static bool executed;
    }

    [Subject(typeof(RelayCommand<>), "CanExecute")]
    class when_checking_can_execute_on_generic_command_with_correct_parameter_type
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            command = new RelayCommand<int>(_ => { }, x => x > 0);
        };

        Because of = () => result = command.CanExecute(42);

        It should_return_true = () => result.ShouldBeTrue();

        static RelayCommand<int> command;
        static bool result;
    }

    [Subject(typeof(RelayCommand<>), "CanExecute")]
    class when_checking_can_execute_on_generic_command_with_wrong_parameter_type
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            command = new RelayCommand<int>(_ => { }, x => true);
        };

        Because of = () => result = command.CanExecute("wrong type");

        It should_return_false = () => result.ShouldBeFalse();

        static RelayCommand<int> command;
        static bool result;
    }

    [Subject(typeof(RelayCommand<>), "CanExecute")]
    class when_can_execute_predicate_evaluates_to_false_for_parameter
    {
        Establish context = () =>
        {
            UiThreadDispatcherContext.SetDispatcher(new CurrentThreadDispatcher());
            command = new RelayCommand<int>(_ => { }, x => x > 10);
        };

        Because of = () => result = command.CanExecute(5);

        It should_return_false = () => result.ShouldBeFalse();

        static RelayCommand<int> command;
        static bool result;
    }
}
