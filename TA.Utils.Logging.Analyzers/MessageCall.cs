using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace TA.Utils.Logging.Analyzers;

/// <summary>A call to one of the <c>IFluentLogBuilder.Message</c> overloads, or an implementation of one.</summary>
sealed class MessageCall
{
    MessageCall(IInvocationOperation invocation, IOperation template)
    {
        Invocation = invocation;
        Template = template;
    }

    public IInvocationOperation Invocation { get; }

    /// <summary>The message template argument, with parentheses and conversions removed.</summary>
    public IOperation Template { get; }

    public static ImmutableArray<IMethodSymbol> GetMessageMethods(INamedTypeSymbol builderType) =>
        builderType.GetMembers("Message").OfType<IMethodSymbol>().ToImmutableArray();

    public static MessageCall? Match(IInvocationOperation invocation, ImmutableArray<IMethodSymbol> messageMethods)
    {
        var target = invocation.TargetMethod;
        if (target.Name != "Message")
            return null;

        foreach (var messageMethod in messageMethods)
        {
            var implementation = target.ContainingType.FindImplementationForInterfaceMember(messageMethod);
            if (!SymbolEqualityComparer.Default.Equals(target.OriginalDefinition, messageMethod) &&
                !SymbolEqualityComparer.Default.Equals(target.OriginalDefinition, implementation?.OriginalDefinition))
                continue;

            var templateParameter = messageMethod.Parameters.First(parameter => parameter.Type.SpecialType == SpecialType.System_String);
            var argument = invocation.Arguments.FirstOrDefault(value => value.Parameter?.Ordinal == templateParameter.Ordinal);
            return argument == null ? null : new MessageCall(invocation, Unwrap(argument.Value));
        }

        return null;
    }

    static IOperation Unwrap(IOperation value)
    {
        while (true)
        {
            if (value is IConversionOperation conversion)
                value = conversion.Operand;
            else if (value is IParenthesizedOperation parenthesized)
                value = parenthesized.Operand;
            else
                return value;
        }
    }
}
