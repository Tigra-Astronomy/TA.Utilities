// This file is part of the TA.Utils project
// Copyright � 2016-2020 Tigra Astronomy, all rights reserved.
// File: NLogLogServiceSpecs.cs  Last modified: 2020-07-17@04:55 by Tim Long

using System.IO;
using System.Runtime.CompilerServices;
using Machine.Specifications;
using TA.Utils.Logging.NLog;

internal class with_caller_info_context
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

internal class when_building_a_default_logger : with_caller_info_context
    {
    Establish context = () => builder = (LogBuilder) new LoggingService().Info();
    It should_have_the_file_name = () => builder.Build().LoggerName.ShouldEqual(CallerFileNameWithoutExtenstion());
    static LogBuilder builder;
    }

internal class when_creating_a_named_logger : with_caller_info_context
    {
    Establish context = () => builder = (LogBuilder) new LoggingService().Info("Roger");
    It should_change_the_name = () => builder.Build().LoggerName.ShouldEqual("Roger");
    static LogBuilder builder;
    }

internal class when_creating_and_building_a_named_logger : with_caller_info_context
    {
    Establish context = () => builder = (LogBuilder) new LoggingService().Info("Roger");
    Because of = () =>  builder.LoggerName("Jim");
    It should_change_the_name = () => builder.Build().LoggerName.ShouldEqual("Jim");
    static LogBuilder builder;
    }
internal class when_building_a_named_logger : with_caller_info_context
    {
    Establish context = () => builder = (LogBuilder) new LoggingService().Info();
    Because of = () =>  builder.LoggerName("Jim");
    It should_change_the_name = () => builder.Build().LoggerName.ShouldEqual("Jim");
    static LogBuilder builder;
    }