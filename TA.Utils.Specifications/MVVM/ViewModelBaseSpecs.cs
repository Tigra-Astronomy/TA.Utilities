// This file is part of the TA.Utils project
// Copyright © 2025 Tigra Astronomy, all rights reserved.

using System.ComponentModel;
using Machine.Specifications;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Core.MVVM;

namespace TA.Utils.Specifications.MVVM
{
    [Subject(typeof(ViewModelBase), "Construction")]
    class when_creating_a_view_model_base_with_default_parameters
    {
        Because of = () => viewModel = new ViewModelBase();

        It should_not_be_null = () => viewModel.ShouldNotBeNull();
        It should_implement_INotifyPropertyChanged = () => viewModel.ShouldBeAssignableTo<INotifyPropertyChanged>();

        static ViewModelBase viewModel;
    }

    [Subject(typeof(ViewModelBase), "Construction")]
    class when_creating_a_view_model_base_with_logger
    {
        Establish context = () => log = new DegenerateLoggerService();

        Because of = () => viewModel = new ViewModelBase(log);

        It should_not_be_null = () => viewModel.ShouldNotBeNull();

        static ViewModelBase viewModel;
        static ILog log;
    }

    [Subject(typeof(ViewModelBase), "PropertyChanged")]
    class when_setting_a_field_with_set_field
    {
        Establish context = () =>
        {
            viewModel = new TestViewModel();
            propertyChangedRaised = false;
            propertyName = null;
            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                propertyName = args.PropertyName;
            };
        };

        Because of = () => viewModel.TestProperty = 42;

        It should_return_true_from_set_field = () => viewModel.LastSetFieldResult.ShouldBeTrue();
        It should_raise_property_changed = () => propertyChangedRaised.ShouldBeTrue();
        It should_raise_event_with_correct_property_name = () => propertyName.ShouldEqual("TestProperty");
        It should_update_the_field_value = () => viewModel.TestProperty.ShouldEqual(42);

        static TestViewModel viewModel;
        static bool propertyChangedRaised;
        static string propertyName;
    }

    [Subject(typeof(ViewModelBase), "PropertyChanged")]
    class when_setting_a_field_to_the_same_value
    {
        Establish context = () =>
        {
            viewModel = new TestViewModel();
            viewModel.TestProperty = 42;
            propertyChangedRaised = false;
            viewModel.PropertyChanged += (sender, args) => propertyChangedRaised = true;
        };

        Because of = () => viewModel.TestProperty = 42;

        It should_return_false_from_set_field = () => viewModel.LastSetFieldResult.ShouldBeFalse();
        It should_not_raise_property_changed = () => propertyChangedRaised.ShouldBeFalse();

        static TestViewModel viewModel;
        static bool propertyChangedRaised;
    }

    [Subject(typeof(ViewModelBase), "PropertyChanged")]
    class when_calling_on_property_changed_directly
    {
        Establish context = () =>
        {
            viewModel = new TestViewModel();
            propertyChangedRaised = false;
            propertyName = null;
            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                propertyName = args.PropertyName;
            };
        };

        Because of = () => viewModel.RaisePropertyChanged("SomeProperty");

        It should_raise_property_changed = () => propertyChangedRaised.ShouldBeTrue();
        It should_raise_event_with_specified_property_name = () => propertyName.ShouldEqual("SomeProperty");

        static TestViewModel viewModel;
        static bool propertyChangedRaised;
        static string propertyName;
    }

    [Subject(typeof(ViewModelBase), "Disposal")]
    class when_disposing_a_view_model
    {
        Establish context = () => viewModel = new TestViewModel();

        Because of = () => viewModel.Dispose();

        It should_not_throw = () => true.ShouldBeTrue();

        static TestViewModel viewModel;
    }

    [Subject(typeof(ViewModelBase), "Disposal")]
    class when_disposing_a_view_model_multiple_times
    {
        Establish context = () =>
        {
            viewModel = new TestViewModel();
            viewModel.Dispose();
        };

        Because of = () => exception = Catch.Exception(() => viewModel.Dispose());

        It should_not_throw = () => exception.ShouldBeNull();

        static TestViewModel viewModel;
        static System.Exception exception;
    }

    class TestViewModel : ViewModelBase
    {
        private int testProperty;
        private bool lastSetFieldResult;

        public int TestProperty
        {
            get => testProperty;
            set => lastSetFieldResult = SetField(ref testProperty, value);
        }

        public bool LastSetFieldResult => lastSetFieldResult;

        public void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }
}
