using System.Collections.Immutable;
using System.Linq;
using Machine.Specifications;
using Microsoft.CodeAnalysis;

namespace TA.Utils.Logging.Analyzers.Specs;

class with_placeholder_count_analysis : with_message_template_analysis
{
    protected static void ShouldWarnAbout(string statement, string template, int placeholders, int arguments)
    {
        var diagnostics = AnalyzePlaceholders(statement);
        diagnostics.Length.ShouldEqual(1);
        var diagnostic = diagnostics.Single();
        diagnostic.Id.ShouldEqual(PlaceholderCountAnalyzer.DiagnosticId);
        diagnostic.Severity.ShouldEqual(DiagnosticSeverity.Warning);
        diagnostic.Location.SourceTree.GetText().ToString(diagnostic.Location.SourceSpan).ShouldEqual(template);
        diagnostic.GetMessage().ShouldContain($"{placeholders} placeholder(s)");
        diagnostic.GetMessage().ShouldContain($"{arguments} argument(s)");
    }

    protected static void ShouldNotWarnAbout(string statement) => AnalyzePlaceholders(statement).ShouldBeEmpty();

    protected static ImmutableArray<Diagnostic> AnalyzePlaceholders(string statement, bool referenceLoggingApi = true,
        string path = "Test.cs") =>
        Analyze(statement, referenceLoggingApi, path, new PlaceholderCountAnalyzer());
}

[Subject(typeof(PlaceholderCountAnalyzer), "Mismatched placeholder counts")]
class when_the_template_and_arguments_do_not_match : with_placeholder_count_analysis
{
    It should_warn_when_there_are_surplus_arguments = () =>
        ShouldWarnAbout("builder.Message(\"Tx: {command}\", message, value);", "\"Tx: {command}\"", 1, 2);

    It should_warn_when_arguments_are_missing = () =>
        ShouldWarnAbout("builder.Message(\"Tx: {first} {second}\", message);", "\"Tx: {first} {second}\"", 2, 1);

    It should_warn_when_the_single_argument_overload_has_placeholders = () =>
        ShouldWarnAbout("builder.Message(\"Tx: {command}\");", "\"Tx: {command}\"", 1, 0);

    It should_warn_when_a_placeholder_is_repeated = () =>
        ShouldWarnAbout("builder.Message(\"Tx: {command} {command}\", message);", "\"Tx: {command} {command}\"", 2, 1);

    It should_ignore_escaped_braces_when_counting = () =>
        ShouldWarnAbout("builder.Message(\"{{literal}} {command}\", message, value);", "\"{{literal}} {command}\"", 1, 2);

    It should_warn_for_the_provider_overload = () =>
        ShouldWarnAbout("builder.Message(CultureInfo.InvariantCulture, \"Tx: {first} {second}\", message);",
            "\"Tx: {first} {second}\"", 2, 1);

    It should_count_the_elements_of_an_explicit_array = () =>
        ShouldWarnAbout("builder.Message(\"Tx: {command}\", new object[] { message, value });", "\"Tx: {command}\"", 1, 2);

    It should_analyze_constant_concatenated_templates = () =>
        ShouldWarnAbout("builder.Message(\"Tx: \" + \"{command}\", message, value);", "\"Tx: \" + \"{command}\"", 1, 2);

    It should_analyze_concrete_implementations = () =>
        ShouldWarnAbout("concrete.Message(\"Tx: {first} {second}\", message);", "\"Tx: {first} {second}\"", 2, 1);

    It should_count_positional_placeholders_by_highest_index = () =>
        ShouldWarnAbout("builder.Message(\"Tx: {0} {1}\", message);", "\"Tx: {0} {1}\"", 2, 1);

    It should_warn_about_surplus_positional_arguments = () =>
        ShouldWarnAbout("builder.Message(\"Tx: {0}\", message, value);", "\"Tx: {0}\"", 1, 2);

    It should_not_let_properties_fill_positional_placeholders = () =>
        ShouldWarnAbout("builder.Message(\"Tx: {0} {1}\", message).Property(\"1\", value);", "\"Tx: {0} {1}\"", 2, 1);

    It should_warn_when_a_property_does_not_match_the_missing_placeholder = () =>
        ShouldWarnAbout("builder.Message(\"Tx: {first} {second}\", message).Property(\"first\", value);",
            "\"Tx: {first} {second}\"", 2, 1);

    It should_warn_when_the_only_property_has_a_different_name = () =>
        ShouldWarnAbout("builder.Message(\"Tx: {command}\").Property(\"other\", value);", "\"Tx: {command}\"", 1, 0);

    It should_warn_when_a_property_precedes_the_message_with_a_different_name = () =>
        ShouldWarnAbout("builder.Property(\"other\", value).Message(\"Tx: {command}\");", "\"Tx: {command}\"", 1, 0);

    It should_warn_about_surplus_arguments_even_with_properties = () =>
        ShouldWarnAbout("builder.Message(\"Tx: {command}\", message, value).Property(\"command\", value);",
            "\"Tx: {command}\"", 1, 2);
}

[Subject(typeof(PlaceholderCountAnalyzer), "Matching placeholder counts")]
class when_the_template_and_arguments_match : with_placeholder_count_analysis
{
    It should_allow_a_template_without_placeholders = () =>
        ShouldNotWarnAbout("builder.Message(\"Tx: done\");");

    It should_allow_one_argument_per_placeholder = () =>
        ShouldNotWarnAbout("builder.Message(\"Tx: {first} {second}\", message, value);");

    It should_allow_the_provider_overload = () =>
        ShouldNotWarnAbout("builder.Message(CultureInfo.InvariantCulture, \"Tx: {first} {second}\", message, value);");

    It should_allow_format_alignment_and_capture_operators = () =>
        ShouldNotWarnAbout("builder.Message(\"{first:N2} {@second,10} {$third}\", message, value, message);");

    It should_allow_escaped_braces = () =>
        ShouldNotWarnAbout("builder.Message(\"{{first}} {second}\", message);");

    It should_allow_positional_placeholders = () =>
        ShouldNotWarnAbout("builder.Message(\"{1} then {0}\", message, value);");

    It should_allow_repeated_positional_placeholders = () =>
        ShouldNotWarnAbout("builder.Message(\"{0} and {0}\", message);");

    It should_allow_an_explicit_array_of_the_right_length = () =>
        ShouldNotWarnAbout("builder.Message(\"{first} {second}\", new object[] { message, value });");
}

[Subject(typeof(PlaceholderCountAnalyzer), "Placeholders filled by properties")]
class when_properties_fill_the_missing_placeholders : with_placeholder_count_analysis
{
    It should_allow_a_matching_property = () =>
        ShouldNotWarnAbout("builder.Message(\"Ended: {Reason}\").Property(\"Reason\", message);");

    It should_allow_a_property_before_the_message = () =>
        ShouldNotWarnAbout("builder.Property(\"Reason\", message).Message(\"Ended: {Reason}\");");

    It should_allow_properties_after_arguments = () =>
        ShouldNotWarnAbout("builder.Message(\"{first} {second}\", message).Property(\"second\", value);");

    It should_allow_several_properties = () =>
        ShouldNotWarnAbout("builder.Message(\"{first} {second}\").Property(\"first\", message).Property(\"second\", value);");

    It should_ignore_capture_operators_when_matching_names = () =>
        ShouldNotWarnAbout("builder.Message(\"{@first} {$second}\").Property(\"first\", message).Property(\"second\", value);");

    It should_allow_a_repeated_placeholder_filled_by_one_property = () =>
        ShouldNotWarnAbout("builder.Message(\"{first} {first}\").Property(\"first\", message);");

    It should_not_guess_when_a_property_name_is_not_a_literal = () =>
        ShouldNotWarnAbout("builder.Message(\"{first}\").Property(message, value);");

    It should_not_guess_when_a_dictionary_of_properties_is_used = () =>
        ShouldNotWarnAbout("builder.Message(\"{first}\").Properties(null);");
}

[Subject(typeof(PlaceholderCountAnalyzer), "Expressions that cannot be analyzed")]
class when_the_placeholder_count_cannot_be_determined : with_placeholder_count_analysis
{
    It should_ignore_non_constant_templates = () =>
        ShouldNotWarnAbout("builder.Message(message, value);");

    It should_ignore_interpolated_templates = () =>
        ShouldNotWarnAbout("builder.Message($\"Tx: {message}\", value);");

    It should_ignore_arguments_passed_as_an_existing_array = () =>
        ShouldNotWarnAbout("builder.Message(\"{first} {second}\", (object[])value);");

    It should_ignore_malformed_templates = () =>
        ShouldNotWarnAbout("builder.Message(\"Tx: {first\", message, value);");

    It should_ignore_unrelated_message_methods = () =>
        ShouldNotWarnAbout("new OtherBuilder().Message(\"Tx: {command}\");");

    It should_handle_compilations_without_the_logging_api = () =>
        AnalyzePlaceholders("new OtherBuilder().Message(\"Tx: {command}\");", referenceLoggingApi: false).ShouldBeEmpty();

    It should_ignore_generated_code = () =>
        AnalyzePlaceholders("builder.Message(\"Tx: {command}\");", path: "Test.g.cs").ShouldBeEmpty();

    It should_respect_local_suppression = () =>
        ShouldNotWarnAbout("#pragma warning disable TAULOG002\nbuilder.Message(\"Tx: {command}\");");
}
