// This file is part of the TA.Utils project
// Copyright © 2015-2023 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: ConsoleLogBuilder.cs  Last modified: 2023-11-13@15:28 by Tim Long

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TA.Utils.Core.Diagnostics;

/// <summary>
///     A fluent log builder for the <see cref="ConsoleLoggerService" />.
/// </summary>
public sealed class ConsoleLogBuilder : IFluentLogBuilder
{
    private const string NoMessage = "(no message)";
    private const string PropertyTemplatePattern = @"\{[@$]?(?<propertyName>\w+)\}";

    private const RegexOptions PropertyTemplateOptions =
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

    private static readonly Regex PropertyNameMatcher = new(PropertyTemplatePattern, PropertyTemplateOptions);
    private readonly ConsoleLoggerOptions options;
    private readonly string severity;
    private Maybe<Exception> maybeException = Maybe<Exception>.Empty;
    private string messageTemplate = NoMessage;
    internal readonly IDictionary<string, object> properties = new Dictionary<string, object>();
    private Maybe<DateTime> maybeTimeStamp = Maybe<DateTime>.Empty;
    private Maybe<StackTrace> maybeStackTrace = Maybe<StackTrace>.Empty;
    private IFormatProvider formatter = CultureInfo.CurrentCulture;

    internal Task<string> RenderTask { get; private set; }

    /// <summary>
    ///     Create and initialize a new instance.
    /// </summary>
    /// <param name="options">Console logger options.</param>
    /// <param name="severity"></param>
    public ConsoleLogBuilder(ConsoleLoggerOptions options, ConsoleLogSeverity severity) : this(options, severity.ToString()) { }

    /// <summary>
    ///     Create and initialize a new instance.
    /// </summary>
    /// <param name="options">Console logger options.</param>
    /// <param name="severity"></param>
    public ConsoleLogBuilder(ConsoleLoggerOptions options, string severity)
    {
        this.options = options;
        this.severity = severity;
        Console.OpenStandardOutput();
    }

    /// <inheritdoc />
    public IFluentLogBuilder Exception(Exception exception)
    {
        maybeException = exception.AsMaybe();
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder LoggerName(string loggerName)
    {
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder Message(string message)
    {
        messageTemplate = message;
        return this;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    ///     Thrown if the number of arguments supplied does not match the number of message
    ///     template placeholders.
    /// </exception>
    public IFluentLogBuilder Message(string format, params object[] args)
    {
        messageTemplate = format;
        var names = GetPropertyNamesFromMessageTemplate(format);
        var namesCount = names.Count;
        var argsCount = args.Length;
        if (namesCount != argsCount)
            throw new ArgumentException(
                $"Mismatched arguments. Format has {namesCount} placeholders but there are {argsCount} arguments, the two should match.");
        for (var i = 0; i < argsCount; i++) properties[names[i]] = args[i];
        return this;
    }

    private List<string> GetPropertyNamesFromMessageTemplate(string format)
    {
        var matches = PropertyNameMatcher.Matches(format);
        var nameList = new List<string>();
        foreach (Match match in matches) nameList.Add(match.Groups["propertyName"].Value);
        return nameList;
    }

    /// <inheritdoc />
    public IFluentLogBuilder Message(IFormatProvider provider, string format, params object[] args)
    {
        formatter = provider;
        return Message(format, args);
    }

    /// <inheritdoc />
    public IFluentLogBuilder Property(string name, object value)
    {
        if (properties.ContainsKey(name))
            throw new ArgumentException($"Duplicate property {name}");
        properties[name] = value;
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder Properties(IDictionary<string, object> properties)
    {
        foreach (var property in properties) Property(property.Key, property.Value);
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder TimeStamp(DateTime timeStamp)
    {
        maybeTimeStamp = Maybe<DateTime>.From(timeStamp.ToUniversalTime());
        return this;
    }

    /// <inheritdoc />
    public IFluentLogBuilder StackTrace(StackTrace stackTrace, int userStackFrame)
    {
        maybeStackTrace = stackTrace.AsMaybe();
        return this;
    }

    /// <inheritdoc />
    public void Write(string callerMemberName = null, string callerFilePath = null, int callerLineNumber = default)
    {
        RenderTask = RenderLogEntry();
    }

    /// <inheritdoc />
    public void WriteIf(Func<bool> condition, string callerMemberName = null, string callerFilePath = null,
        int callerLineNumber = default)
    {
        if (condition())
            RenderTask = RenderLogEntry();
    }

    /// <inheritdoc />
    public void WriteIf(bool condition, string callerMemberName = null, string callerFilePath = null,
        int callerLineNumber = default)
    {
        if (condition) RenderTask = RenderLogEntry();
    }

    private async Task<string> RenderLogEntry()
    {
        var builder = new StringBuilder();
        await RenderMessageTemplateAsync(builder).ContinueOnAnyThread();
        await RenderException(builder).ContinueOnAnyThread();
        await RenderStackTrace(builder).ContinueOnAnyThread();
        if (options.renderProperties)
            await RenderPropertiesAsync(builder).ContinueOnAnyThread();
        var renderedLogEntry = builder.ToString();
        Console.WriteLine(renderedLogEntry);
        return renderedLogEntry;
    }

    private async Task RenderStackTrace(StringBuilder builder)
    {
        if (maybeStackTrace.IsEmpty)
            return;
        var trace = maybeStackTrace.Single();
        builder.AppendLine("  Stack trace:");
        foreach (var frame in trace.GetFrames()) builder.Append("    ").AppendLine(frame.ToString());
        await Task.CompletedTask.ContinueOnAnyThread();
    }

    private async Task RenderException(StringBuilder builder)
    {
        if (maybeException.IsEmpty)
            return;
        builder.Append("  Exception: ")
            .AppendLine(maybeException.Single().Message);
        await Task.CompletedTask.ContinueOnAnyThread();
    }

    private async Task RenderMessageTemplateAsync(StringBuilder builder)
    {
        var timestamp = maybeTimeStamp.Any() ? maybeTimeStamp.Single() : DateTime.UtcNow;
        builder
            .Append(timestamp)
            .Append(" ")
            .Append('[').Append(severity).Append(']').Append(' ');
        if (properties.Any())
            // If there are any properties available, try to render the message template using them.
            await RenderMessageTemplateArgumentsAsync(builder).ContinueOnAnyThread();
        else
            // If there are no properties available, include the message template verbatim.
            builder.AppendLine(messageTemplate);
    }

    private async Task RenderPropertiesAsync(StringBuilder builder)
    {
        foreach (var property in properties)
            builder.Append("  ")
                .Append(property.Key).Append(": ")
                .AppendFormat(formatter, "{0}", property.Value)
                .AppendLine();
        await Task.CompletedTask.ContinueOnAnyThread();
    }

    private async Task RenderMessageTemplateArgumentsAsync(StringBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(messageTemplate))
            return;
        var matches = PropertyNameMatcher.Matches(messageTemplate);
        if (matches.Count == 0)
        {
            builder.AppendLine(messageTemplate);
            return;
        }

        // Message template has argument placeholders - try to populate them.
        var cursor = 0; // position in the message template.
        foreach (Match match in matches)
        {
            var property = match.Groups["propertyName"].Value;
            if (string.IsNullOrWhiteSpace(property))
                continue;
            if (properties.ContainsKey(property))
            {
                var valueObject = properties[property];
                if (match.Index > cursor)
                {
                    // Copy in the preceding template literal text followed by the property value.
                    var head = messageTemplate.Substring(cursor, match.Index - cursor);
                    builder.Append(head);
                }

                // Copy in the substituted property value and move the cursor up to the end of the match.
                builder.AppendFormat(formatter, "{0}", valueObject);
                cursor = match.Index + match.Length;
            }

            else
            {
                // There is no matching property value, so render it as literal text.
                builder.Append('{').Append(property).Append('}');
                cursor = match.Index + match.Length;
            }
        }

        if (cursor < messageTemplate.Length)
            builder.Append(messageTemplate.Substring(cursor));
        builder.AppendLine(); // Add a final newline.
        await Task.CompletedTask.ContinueOnAnyThread();
    }
}
