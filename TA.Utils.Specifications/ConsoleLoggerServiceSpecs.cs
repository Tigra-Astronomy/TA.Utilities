using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Machine.Specifications;
using TA.Utils.Core;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Specifications;

#region context base classes

class with_console_logger_context
{
    Establish context = () =>
    {
        logService = new ConsoleLoggerService();
        builder = logService.Info() as ConsoleLogBuilder;
    };

    static List<string> renderCache;

    protected static string RenderResult => builder.RenderTask.Result;

    protected static IList<string> RenderedLines
    {
        get
        {
            if (renderCache is null)
                renderCache = RenderResult.GetLines().ToList();
            return renderCache;
        }
    }

    protected static ConsoleLogBuilder builder;
    static ConsoleLoggerService logService;
}

#endregion

[Subject(typeof(ConsoleLoggerService), "Rendering")]
class when_building_a_log_entry_with_one_property_at_info_severity : with_console_logger_context
{
    const int ExpectedPropertyValue = 17;
    const string ExpectedPropertyName = "myValue";
    const string ExpectedRenderedProperty = "  myValue: 17";
    Because of = () => { builder.Property(ExpectedPropertyName, ExpectedPropertyValue).Write(); };
    It should_render_empty_message_in_line_1 = () => RenderedLines.First().ShouldContain("(no message");
    It should_render_the_property_value_in_line_2 = () => RenderedLines.Skip(1).Take(1).ShouldContain(ExpectedRenderedProperty);
}

[Subject(typeof(ConsoleLoggerService), "Rendering")]
class when_building_a_log_entry_with_one_property_and_message_template_at_info_severity : with_console_logger_context
{
    const int ExpectedPropertyValue = 17;
    const string ExpectedPropertyName = "myValue";

    Because of = () =>
    {
        builder
            .Message("Got value {myValue}", ExpectedPropertyValue)
            .Write();
    };

    It should_render_the_message_template = () => RenderResult.ShouldContain($"{ExpectedPropertyValue}");
    It should_render_the_property_name = () => RenderResult.ShouldContain($"{ExpectedPropertyValue}");
}

[Subject(typeof(ConsoleLoggerService), "Mismatched template arguments")]
class when_building_a_log_entry_with_too_few_template_arguments : with_console_logger_context
{
    const int ExpectedPropertyValue = 17;
    const string ExpectedPropertyName = "myValue";

    Because of = () =>
    {
        exception = Catch.Exception(() =>
                                        builder
                                            .Message("Got value {myValue1} and {myValue2}", ExpectedPropertyValue)
                                            .Write()
                                   );
    };

    It should_throw = () => exception.ShouldBeOfExactType<ArgumentException>();
    It should_mention_mismatched_arguments = () => exception.Message.ShouldContain("Mismatched arguments");
    static Exception exception;
}

[Subject(typeof(ConsoleLoggerService), "Mismatched template arguments")]
class when_building_a_log_entry_with_too_many_template_arguments : with_console_logger_context
{
    const int ExpectedPropertyValue = 17;
    const string ExpectedPropertyName = "myValue";

    Because of = () =>
    {
        exception = Catch.Exception(() =>
                                        builder
                                            .Message("Got value {myValue1} and {myValue2}", ExpectedPropertyValue, 4, 5)
                                            .Write()
                                   );
    };

    It should_throw = () => exception.ShouldBeOfExactType<ArgumentException>();
    It should_mention_mismatched_arguments = () => exception.Message.ShouldContain("Mismatched arguments");
    static Exception exception;
}

[Subject(typeof(ConsoleLoggerService), "custom formatting")]
class when_building_a_log_entry_with_a_custom_formatter : with_console_logger_context
{
    Because of = () =>
        exception = Catch.Exception(() =>
                                        builder
                                            .Message(CultureInfo.GetCultureInfo(1033),
                                                     "At {timestamp} something happened",
                                                     new DateTime(2000, 12, 30, 0, 0, 0))
                                            .Write()
                                   );

    It should_render_usa_date = () => RenderResult.ShouldContain("At 12/30/2000");
    static Exception exception;
}

class when_logging_with_a_complex_message_template : with_console_logger_context
{
    const string expectedOutput = "[Info] Source[1]: Subscribe()";

    Because of = () => builder
            .Message("{source}[{id}]: {action}({content})",
            "Source",
            1,
            "Subscribe",
            string.Empty)
            .Write();

    It should_produce_the_expected_output = () => RenderResult.ShouldContain(expectedOutput);
}