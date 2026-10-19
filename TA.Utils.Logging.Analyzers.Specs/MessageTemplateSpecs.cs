using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Machine.Specifications;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Logging.Analyzers.Specs;

class with_message_template_analysis
{
    static readonly MetadataReference[] PlatformReferences = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
        .Split(Path.PathSeparator)
        .Select(path => MetadataReference.CreateFromFile(path))
        .ToArray();

    protected static void ShouldWarn(string statement, string expression)
    {
        var diagnostics = Analyze(statement);
        diagnostics.Length.ShouldEqual(1);
        var diagnostic = diagnostics.Single();
        diagnostic.Id.ShouldEqual(MessageTemplateAnalyzer.DiagnosticId);
        diagnostic.Severity.ShouldEqual(DiagnosticSeverity.Warning);
        diagnostic.Location.SourceTree.GetText().ToString(diagnostic.Location.SourceSpan).ShouldEqual(expression);
        diagnostic.GetMessage().ShouldContain("named placeholders");
        diagnostic.GetMessage().ShouldContain(".Property(...)");
    }

    protected static void ShouldNotWarn(string statement)
    {
        var diagnostics = Analyze(statement);
        diagnostics.ShouldBeEmpty();
    }

    protected static ImmutableArray<Diagnostic> Analyze(string statement, bool referenceLoggingApi = true, string path = "Test.cs",
        DiagnosticAnalyzer analyzer = null)
    {
        var source = $$"""
            using System;
            using System.Globalization;
            using SystemString = System.String;
            {{(referenceLoggingApi ? "using TA.Utils.Core.Diagnostics;" : "")}}
            class OtherBuilder
            {
                public void Message(string message) { }
                public static string Format(string template, object value) => template;
            }
            class Test
            {
                const string Prefix = "Tx: ";
                const string ConstantValue = "command";
                void Run({{(referenceLoggingApi ? "IFluentLogBuilder builder, ConsoleLogBuilder concrete, ILog log," : "")}} string message, object value)
                {
                    {{statement}}
                }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(source, path: path);
        var references = referenceLoggingApi
            ? PlatformReferences.Append(MetadataReference.CreateFromFile(typeof(IFluentLogBuilder).Assembly.Location))
            : PlatformReferences;
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var compilation = CSharpCompilation.Create("AnalyzerTest", new[] { tree }, references, options);
        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        var errorMessages = string.Join(Environment.NewLine, errors);
        errorMessages.ShouldEqual("");

        var analyzers = ImmutableArray.Create(analyzer ?? new MessageTemplateAnalyzer());
        var analysis = compilation.WithAnalyzers(analyzers);
        return analysis.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
    }
}

[Subject(typeof(MessageTemplateAnalyzer), "Dynamic message templates")]
class when_analyzing_dynamic_message_templates : with_message_template_analysis
{
    It should_warn_for_the_original_fluent_logging_example = () =>
        ShouldWarn("log.Debug().Message($\"Tx: {message}\").Write();", "$\"Tx: {message}\"");

    It should_warn_for_interpolation = () =>
        ShouldWarn("builder.Message($\"Tx: {message}\");", "$\"Tx: {message}\"");

    It should_warn_for_verbatim_interpolation = () =>
        ShouldWarn("builder.Message($@\"Tx: {message}\");", "$@\"Tx: {message}\"");

    It should_warn_for_raw_interpolation = () =>
        ShouldWarn("builder.Message($\"\"\"Tx: {message}\"\"\");", "$\"\"\"Tx: {message}\"\"\"");

    It should_warn_for_constant_interpolation = () =>
        ShouldWarn("builder.Message($\"Tx: {ConstantValue}\");", "$\"Tx: {ConstantValue}\"");

    It should_warn_for_string_format = () =>
        ShouldWarn("builder.Message(string.Format(\"Tx: {0}\", message));", "string.Format(\"Tx: {0}\", message)");

    It should_warn_for_string_format_with_a_provider = () =>
        ShouldWarn("builder.Message(string.Format(CultureInfo.InvariantCulture, \"Tx: {0}\", message));",
            "string.Format(CultureInfo.InvariantCulture, \"Tx: {0}\", message)");

    It should_resolve_string_aliases = () =>
        ShouldWarn("builder.Message(SystemString.Format(\"Tx: {0}\", message));", "SystemString.Format(\"Tx: {0}\", message)");

    It should_warn_for_concatenation = () =>
        ShouldWarn("builder.Message(\"Tx: \" + message);", "\"Tx: \" + message");

    It should_warn_for_concatenation_with_an_object = () =>
        ShouldWarn("builder.Message(\"Tx: \" + value);", "\"Tx: \" + value");

    It should_warn_for_nested_concatenation = () =>
        ShouldWarn("builder.Message(Prefix + message + \"!\");", "Prefix + message + \"!\"");

    It should_warn_for_parenthesized_templates = () =>
        ShouldWarn("builder.Message(((\"Tx: \" + message)));", "\"Tx: \" + message");

    It should_warn_for_explicitly_cast_templates = () =>
        ShouldWarn("builder.Message((string)$\"Tx: {message}\");", "$\"Tx: {message}\"");

    It should_analyze_the_params_overload = () =>
        ShouldWarn("builder.Message($\"Tx: {message}\", value);", "$\"Tx: {message}\"");

    It should_analyze_the_provider_overload = () =>
        ShouldWarn("builder.Message(CultureInfo.InvariantCulture, $\"Tx: {message}\", value);", "$\"Tx: {message}\"");

    It should_map_reordered_named_arguments_to_the_template = () =>
        ShouldWarn("builder.Message(args: new object[] { value }, format: $\"Tx: {message}\", provider: CultureInfo.InvariantCulture);",
            "$\"Tx: {message}\"");

    It should_map_the_named_single_argument = () =>
        ShouldWarn("builder.Message(message: $\"Tx: {message}\");", "$\"Tx: {message}\"");

    It should_analyze_concrete_implementations = () =>
        ShouldWarn("concrete.Message($\"Tx: {message}\");", "$\"Tx: {message}\"");

    It should_analyze_concrete_provider_overloads = () =>
        ShouldWarn("concrete.Message(CultureInfo.InvariantCulture, \"Tx: \" + message, value);", "\"Tx: \" + message");
}

[Subject(typeof(MessageTemplateAnalyzer), "Valid templates and unrelated calls")]
class when_analyzing_other_logging_expressions : with_message_template_analysis
{
    It should_allow_literal_templates = () =>
        ShouldNotWarn("builder.Message(\"Tx: {command}\");");

    It should_allow_named_template_arguments = () =>
        ShouldNotWarn("builder.Message(\"Tx: {command}\", message);");

    It should_allow_explicit_properties = () =>
        ShouldNotWarn("builder.Message(\"Tx: {command}\").Property(\"command\", message);");

    It should_allow_interpolated_property_values = () =>
        ShouldNotWarn("builder.Message(\"Tx: {command}\", $\"{message}!\");");

    It should_allow_formatted_property_values = () =>
        ShouldNotWarn("builder.Message(CultureInfo.InvariantCulture, \"Tx: {command}\", string.Format(\"{0}\", message));");

    It should_allow_concatenated_property_values = () =>
        ShouldNotWarn("builder.Message(\"Tx: {command}\").Property(\"command\", Prefix + message);");

    It should_allow_constant_concatenation = () =>
        ShouldNotWarn("builder.Message(\"Tx: \" + \"{command}\", message);");

    It should_allow_concatenated_constant_identifiers = () =>
        ShouldNotWarn("builder.Message(Prefix + ConstantValue);");

    It should_allow_constant_and_variable_references = () =>
        ShouldNotWarn("builder.Message(Prefix); builder.Message(message);");

    It should_allow_unrelated_message_methods = () =>
        ShouldNotWarn("new OtherBuilder().Message($\"Tx: {message}\");");

    It should_ignore_other_format_methods = () =>
        ShouldNotWarn("builder.Message(OtherBuilder.Format(\"Tx: {command}\", message));");

    It should_handle_compilations_without_the_logging_api = () =>
        Analyze("new OtherBuilder().Message($\"Tx: {message}\");", referenceLoggingApi: false).ShouldBeEmpty();

    It should_ignore_generated_code = () =>
        Analyze("builder.Message($\"Tx: {message}\");", path: "Test.g.cs").ShouldBeEmpty();

    It should_respect_local_suppression = () =>
        ShouldNotWarn("#pragma warning disable TAULOG001\nbuilder.Message($\"Tx: {message}\");");
}
