// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: NLogLogServiceSpecs.cs  Last modified: 2020-07-19@20:23 by Tim Long

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Machine.Specifications;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Logging.NLog;

// ReSharper disable UnusedMember.Local

namespace TA.Utils.Specifications;

#region Context base classes

class with_caller_info_context
{
    protected static string CallerFileNameWithoutExtenstion([CallerFilePath] string callerFilePath = null)
    {
        const string unknown = "(unknown)";
        if (string.IsNullOrWhiteSpace(callerFilePath))
            return unknown;
        var fileName = Path.GetFileNameWithoutExtension(callerFilePath);
        return string.IsNullOrWhiteSpace(fileName) ? unknown : fileName;
    }
}

#endregion

class when_building_a_default_logger : with_caller_info_context
{
    Establish context = () => builder = (LogBuilder)new LoggingService().Info();
    It should_have_the_class_name_as_source = () => builder.Build().LoggerName.ShouldEqual(nameof(when_building_a_default_logger));
    static LogBuilder builder;
}

class when_creating_a_named_logger : with_caller_info_context
{
    Establish context = () => builder = (LogBuilder)new LoggingService().WithName("Roger").Info();
    It should_override_the_default_name = () => builder.Build().LoggerName.ShouldEqual("Roger");
    static LogBuilder builder;
}

class when_overriding_the_log_source_name_for_one_log_event : with_caller_info_context
{
    Establish context = () => builder = (LogBuilder)new LoggingService().Info(sourceNameOverride: "Roger");
    Because of = () => builder.LoggerName("Jim");
    It should_override_the_logger_name = () => builder.Build().LoggerName.ShouldEqual("Jim");
    static LogBuilder builder;
}

class when_building_a_named_logger : with_caller_info_context
{
    Establish context = () => builder = (LogBuilder)new LoggingService().Info();
    Because of = () => builder.LoggerName("Jim");
    It should_change_the_name = () => builder.Build().LoggerName.ShouldEqual("Jim");
    static LogBuilder builder;
}

[Subject(typeof(LoggingService), "ambient properties")]
class when_creating_a_logger_with_ambient_properties
{
    Establish context = () => log = new LoggingService().WithAmbientProperty(PropertyName, true);
    Because of = () => builder = log.Debug() as LogBuilder;
    It should_add_the_ambient_properties = () => builder.Build().Properties.ContainsKey(PropertyName).ShouldBeTrue();
    static LogBuilder builder;
    static ILog log;
    const string PropertyName = "TestProperty";
}

class when_adding_a_property_with_a_conflicting_name
{
    Establish context = () => logService = new LoggingService();
    Because of = () => builder = logService.Info().Property("conflictedName", 1).Property("conflictedName", 2) as LogBuilder;
    It should_keep_the_original_property_name = () => builder.PeekLogEvent.Properties.ContainsKey("conflictedName").ShouldBeTrue();
    It should_keep_the_original_property_value = () => builder.PeekLogEvent.Properties["conflictedName"].ShouldEqual(1);
    It should_deconflict_the_new_property_name = () => builder.PeekLogEvent.Properties.ContainsKey("conflictedName1").ShouldBeTrue();
    It should_add_the_new_value = () => builder.PeekLogEvent.Properties["conflictedName1"].ShouldEqual(2);
    static LogBuilder builder;
    static LoggingService logService;
}

class when_adding_many_conflicting_properties
{
    Because of = () => exception = Catch.Exception(AddLotsOfConflictingProperrties);

    It should_eventually_throw = () => exception.ShouldBeOfExactType<ArgumentException>();
    static Exception exception;

    static void AddLotsOfConflictingProperrties()
    {
        var builder = new LoggingService().Info();
        for (var i = 0; i < 1000; ++i) builder.Property("conflictedName", 99);
    }
}