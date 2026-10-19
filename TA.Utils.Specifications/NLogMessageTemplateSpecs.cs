using System;
using System.Collections.Generic;
using System.Globalization;
using Machine.Specifications;
using NLog;
using NLog.Config;
using NLog.Targets;
using TA.Utils.Logging.NLog;

namespace TA.Utils.Specifications;

class with_nlog_message_template
{
    Establish context = () =>
    {
        factory = new LogFactory();
        target = new MemoryTarget { Layout = "${message}" };
        var configuration = new LoggingConfiguration();
        configuration.AddRuleForAllLevels(target);
        factory.Configuration = configuration;
        builder = new LogBuilder(factory.GetLogger("TemplateSpecs"), LogLevel.Info);
    };
    Cleanup after = () => factory.Dispose();
    protected static LogBuilder builder;
    protected static MemoryTarget target;
    protected static LogFactory factory;
}

class when_nlog_writes_a_property_backed_template : with_nlog_message_template
{
    Because of = () => builder.Message("Ended: {Reason}").Property("Reason", "UserClosed").Write();
    It should_render_the_property = () => target.Logs[0].ShouldEqual("Ended: UserClosed");
    It should_keep_the_property = () => builder.PeekLogEvent.Properties["Reason"].ShouldEqual("UserClosed");
    It should_keep_the_original_template = () => builder.PeekLogEvent.Message.ShouldEqual("Ended: {Reason}");
}

class when_nlog_property_precedes_the_message : with_nlog_message_template
{
    Because of = () => builder.Property("Reason", "UserClosed").Message("Ended: {Reason}").Write();
    It should_render_the_property = () => target.Logs[0].ShouldEqual("Ended: UserClosed");
}

class when_nlog_writes_a_positional_template_argument : with_nlog_message_template
{
    Because of = () => builder.Message("Ended: {Reason}", "UserClosed").Write();
    It should_render_the_argument = () => target.Logs[0].ShouldEqual("Ended: UserClosed");
    It should_capture_the_property = () => builder.PeekLogEvent.Properties["Reason"].ShouldEqual("UserClosed");
}

class when_nlog_writes_a_numeric_template : with_nlog_message_template
{
    Because of = () => builder.Message("{1} then {0}", "first", "second").Property("Other", 1).Write();
    It should_keep_numeric_argument_order = () => target.Logs[0].ShouldEqual("second then first");
}

class when_nlog_writes_an_unmatched_template_hole : with_nlog_message_template
{
    Because of = () => builder.Message("Ended: {Reason}").Property("Other", "value").Write();
    It should_leave_the_hole_unchanged = () => target.Logs[0].ShouldEqual("Ended: {Reason}");
}

class when_nlog_writes_partially_matched_property_holes : with_nlog_message_template
{
    Because of = () => builder.Message("{Missing:N2} / {Reason} / {@Absent} / {Reason} / {$Unknown,10}")
        .Property("Reason", "UserClosed").Write();
    It should_render_only_matched_holes = () =>
        target.Logs[0].ShouldEqual("{Missing:N2} / UserClosed / {@Absent} / UserClosed / {$Unknown,10}");
    It should_not_capture_missing_properties = () => builder.PeekLogEvent.Properties.ContainsKey("Missing").ShouldBeFalse();
    It should_keep_the_original_template = () => builder.PeekLogEvent.Message.ShouldEqual(
        "{Missing:N2} / {Reason} / {@Absent} / {Reason} / {$Unknown,10}");
}

class when_nlog_mixes_arguments_and_properties : with_nlog_message_template
{
    Because of = () => builder.Message("{First} / {Second}", "argument").Property("Second", "property").Write();
    It should_preserve_argument_positions = () => target.Logs[0].ShouldEqual("argument / property");
    It should_keep_the_named_property = () => builder.PeekLogEvent.Properties["Second"].ShouldEqual("property");
}

class when_nlog_mixes_properties_before_the_message_and_arguments : with_nlog_message_template
{
    Because of = () => builder.Property("Second", "property").Message("{First} / {Second}", "argument").Write();
    It should_preserve_argument_positions = () => target.Logs[0].ShouldEqual("argument / property");
}

class when_nlog_formats_property_values : with_nlog_message_template
{
    Because of = () => builder.Message(CultureInfo.InvariantCulture, "{Amount,12:N2} / {Amount,-12:N2}")
        .Property("Amount", 1234.5m).Write();
    It should_apply_format_and_alignment = () => target.Logs[0].ShouldEqual("    1,234.50 / 1,234.50    ");
}

class when_nlog_uses_capture_operators_with_properties : with_nlog_message_template
{
    Establish context = () =>
    {
        value = new[] { 1, 2 };
        expected = new LogEventInfo(LogLevel.Info, "Expected", "{@Value} / {$Value}")
        {
            Parameters = new object[] { value, value }
        }.FormattedMessage;
    };
    Because of = () => builder.Message("{@Value} / {$Value}").Property("Value", value).Write();
    It should_use_nlog_capture_semantics = () => target.Logs[0].ShouldEqual(expected);
    It should_keep_the_original_property_value = () => builder.PeekLogEvent.Properties["Value"].ShouldBeTheSameAs(value);
    static int[] value;
    static string expected;
}

class when_nlog_writes_escaped_braces : with_nlog_message_template
{
    Because of = () => builder.Message("{{Reason}} / {{{Reason}}}").Property("Reason", "UserClosed").Write();
    It should_not_bind_escaped_holes = () => target.Logs[0].ShouldEqual("{Reason} / {UserClosed}");
}

class when_nlog_writes_a_null_property : with_nlog_message_template
{
    Because of = () => builder.Message("Ended: {Reason}").Property("Reason", null).Write();
    It should_render_null_using_nlog = () => target.Logs[0].ShouldEqual("Ended: NULL");
    It should_keep_the_null_property = () => builder.PeekLogEvent.Properties.ContainsKey("Reason").ShouldBeTrue();
}

class when_nlog_writes_dictionary_properties : with_nlog_message_template
{
    Because of = () => builder.Message("Ended: {Reason}")
        .Properties(new Dictionary<string, object> { ["Reason"] = "UserClosed" }).Write();
    It should_render_the_property = () => target.Logs[0].ShouldEqual("Ended: UserClosed");
}

class when_nlog_writes_ambient_properties : with_nlog_message_template
{
    Establish context = () => builder = new LogBuilder(factory.GetLogger("Ambient"), LogLevel.Info,
        new Dictionary<string, object> { ["Reason"] = "UserClosed" });
    Because of = () => builder.Message("Ended: {Reason}").Write();
    It should_render_the_property = () => target.Logs[0].ShouldEqual("Ended: UserClosed");
}

class when_nlog_writes_conditionally_with_a_boolean : with_nlog_message_template
{
    Because of = () => builder.Message("Ended: {Reason}").Property("Reason", "UserClosed").WriteIf(true);
    It should_render_the_property = () => target.Logs[0].ShouldEqual("Ended: UserClosed");
}

class when_nlog_writes_conditionally_with_a_predicate : with_nlog_message_template
{
    Because of = () => builder.Message("Ended: {Reason}").Property("Reason", "UserClosed").WriteIf(() => true);
    It should_render_the_property = () => target.Logs[0].ShouldEqual("Ended: UserClosed");
}

class when_nlog_skips_a_conditional_message : with_nlog_message_template
{
    Because of = () =>
    {
        builder.Message("Ended: {Reason}").Property("Reason", "UserClosed");
        builder.WriteIf(false);
        builder.WriteIf(() => false);
        builder.WriteIf((Func<bool>)null);
    };
    It should_not_write = () => target.Logs.Count.ShouldEqual(0);
    It should_not_bind_parameters = () => builder.PeekLogEvent.Parameters.ShouldBeNull();
}
