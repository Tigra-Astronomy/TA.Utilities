using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace TA.Utils.Logging.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PlaceholderCountAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "TAULOG002";

    static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Match message template placeholders to values",
        "Message template has {0} placeholder(s) but {1} argument(s) were supplied",
        "Logging",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Placeholders without a value are not rendered, and surplus arguments are silently dropped. " +
                     "Named placeholders without an argument must be filled by a .Property(...) with the same name.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            var builderType = startContext.Compilation.GetTypeByMetadataName("TA.Utils.Core.Diagnostics.IFluentLogBuilder");
            if (builderType == null)
                return;

            var messageMethods = MessageCall.GetMessageMethods(builderType);
            startContext.RegisterOperationAction(
                operationContext => AnalyzeInvocation(operationContext, messageMethods, builderType),
                OperationKind.Invocation);
        });
    }

    static void AnalyzeInvocation(OperationAnalysisContext context, ImmutableArray<IMethodSymbol> messageMethods,
        INamedTypeSymbol builderType)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var call = MessageCall.Match(invocation, messageMethods);
        if (call == null || call.Template is IInterpolatedStringOperation)
            return;

        if (!(call.Template.ConstantValue.Value is string text))
            return;

        var template = ParsedTemplate.Parse(text);
        var argumentCount = CountArguments(invocation);
        if (template == null || argumentCount == null || template.ExpectedArguments == argumentCount)
            return;

        if (template.ExpectedArguments > argumentCount && PropertiesFillTheGap(template, argumentCount.Value, invocation, builderType))
            return;

        var diagnostic = Diagnostic.Create(Rule, call.Template.Syntax.GetLocation(), template.ExpectedArguments, argumentCount);
        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>Counts the template arguments, or returns null if they are not visible at the call site.</summary>
    static int? CountArguments(IInvocationOperation invocation)
    {
        var paramsArgument = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.IsParams == true);
        if (paramsArgument == null)
            return 0;

        var elements = (paramsArgument.Value as IArrayCreationOperation)?.Initializer?.ElementValues;
        return elements?.Length;
    }

    static bool PropertiesFillTheGap(ParsedTemplate template, int argumentCount, IInvocationOperation message,
        INamedTypeSymbol builderType)
    {
        if (template.IsPositional)
            return false;

        var propertyNames = CollectPropertyNames(message, builderType);
        if (propertyNames == null)
            return true;

        return template.PlaceholderNames.Skip(argumentCount).All(propertyNames.Contains);
    }

    /// <summary>Returns the literal names of properties in the fluent chain, or null if any are not knowable.</summary>
    static HashSet<string>? CollectPropertyNames(IInvocationOperation message, INamedTypeSymbol builderType)
    {
        var names = new HashSet<string>();
        foreach (var call in FluentChain(message).Where(call => IsBuilderMethod(call, builderType)))
        {
            if (call.TargetMethod.Name == "Properties")
                return null;
            if (call.TargetMethod.Name != "Property")
                continue;

            var name = call.Arguments.FirstOrDefault(argument => argument.Parameter?.Ordinal == 0)?.Value.ConstantValue;
            if (name == null || !name.Value.HasValue || !(name.Value.Value is string propertyName))
                return null;
            names.Add(propertyName);
        }

        return names;
    }

    static bool IsBuilderMethod(IInvocationOperation call, INamedTypeSymbol builderType)
    {
        var type = call.TargetMethod.ContainingType;
        return SymbolEqualityComparer.Default.Equals(type, builderType) ||
               type.AllInterfaces.Contains(builderType, SymbolEqualityComparer.Default);
    }

    static IEnumerable<IInvocationOperation> FluentChain(IInvocationOperation message)
    {
        var earlier = message.Instance as IInvocationOperation;
        while (earlier != null)
        {
            yield return earlier;
            earlier = earlier.Instance as IInvocationOperation;
        }

        var current = message;
        while (current.Parent is IInvocationOperation later && later.Instance == current)
        {
            yield return later;
            current = later;
        }
    }
}
