using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace TA.Utils.Logging.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MessageTemplateAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "TAULOG001";

    static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Keep logging message templates constant",
        "Use a constant message template with named placeholders and pass values as arguments or via .Property(...)",
        "Logging",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Interpolated, formatted, or concatenated variable data creates a different event type for each value in structured logging sinks.");

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
                operationContext => AnalyzeInvocation(operationContext, messageMethods),
                OperationKind.Invocation);
        });
    }

    static void AnalyzeInvocation(OperationAnalysisContext context, ImmutableArray<IMethodSymbol> messageMethods)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var call = MessageCall.Match(invocation, messageMethods);
        if (call == null || !IsDynamicTemplate(call.Template))
            return;

        var diagnostic = Diagnostic.Create(Rule, call.Template.Syntax.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    static bool IsDynamicTemplate(IOperation value)
    {
        if (value is IInterpolatedStringOperation)
            return true;

        if (value is IInvocationOperation invocation)
        {
            var method = invocation.TargetMethod;
            return method.Name == "Format" && method.IsStatic &&
                   method.ContainingType.SpecialType == SpecialType.System_String;
        }

        return value is IBinaryOperation binary &&
               binary.OperatorKind == BinaryOperatorKind.Add &&
               binary.Type?.SpecialType == SpecialType.System_String &&
               !binary.ConstantValue.HasValue;
    }
}
