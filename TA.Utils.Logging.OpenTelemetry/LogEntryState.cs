// This file is part of the TA.Utils project
// Copyright © 2015-2026 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: LogEntryState.cs  Last modified: 2026-02-23 by tim.long

using System.Collections;
using System.Text.RegularExpressions;

namespace TA.Utils.Logging.OpenTelemetry;

/// <summary>
///     Structured log state that implements <see cref="IReadOnlyList{T}" /> of
///     <see cref="KeyValuePair{TKey,TValue}" />.
///     OpenTelemetry's log processor reads properties from this collection.
///     The special <c>{OriginalFormat}</c> key holds the message template.
/// </summary>
internal sealed partial class LogEntryState : IReadOnlyList<KeyValuePair<string, object?>>
{
    private const string OriginalFormatKey = "{OriginalFormat}";
    private readonly List<KeyValuePair<string, object?>> entries = [];
    private string messageTemplate = string.Empty;
    private IFormatProvider? formatProvider;

    /// <summary>
    ///     Sets the message template. Named placeholders (<c>{Name}</c>) are preserved for structured logging;
    ///     the OpenTelemetry exporter will pair them with the matching property values.
    /// </summary>
    internal void SetMessageTemplate(string template)
    {
        messageTemplate = template;
    }

    /// <summary>
    ///     Sets the format provider used when rendering the message.
    /// </summary>
    internal void SetFormatProvider(IFormatProvider provider)
    {
        formatProvider = provider;
    }

    /// <summary>
    ///     Adds a named property to the state.
    /// </summary>
    internal void AddProperty(string name, object? value)
    {
        entries.Add(new KeyValuePair<string, object?>(name, value));
    }

    /// <summary>
    ///     Renders the message by substituting named template placeholders with their values.
    /// </summary>
    internal string RenderMessage()
    {
        if (string.IsNullOrEmpty(messageTemplate))
            return string.Empty;

        var rendered = messageTemplate;
        foreach (var entry in entries)
        {
            if (entry.Key == OriginalFormatKey)
                continue;

            var formattedValue = entry.Value is IFormattable formattable
                ? formattable.ToString(null, formatProvider)
                : entry.Value?.ToString() ?? string.Empty;

            // Replace both {Name} and {@Name} / {$Name} variants
            rendered = Regex.Replace(
                rendered,
                @"\{[@$]?" + Regex.Escape(entry.Key) + @"\}",
                formattedValue);
        }

        return rendered;
    }

    /// <summary>
    ///     Builds the final snapshot including the <c>{OriginalFormat}</c> entry expected by OpenTelemetry.
    ///     Call this once, immediately before passing the state to <c>ILogger.Log</c>.
    /// </summary>
    internal LogEntryState Freeze()
    {
        // Ensure {OriginalFormat} is present so that the OTel exporter records the template
        if (!string.IsNullOrEmpty(messageTemplate))
            entries.Add(new KeyValuePair<string, object?>(OriginalFormatKey, messageTemplate));

        return this;
    }

    /// <inheritdoc />
    public override string ToString() => RenderMessage();

    // --- IReadOnlyList<KeyValuePair<string, object?>> implementation ---

    /// <inheritdoc />
    public int Count => entries.Count;

    /// <inheritdoc />
    public KeyValuePair<string, object?> this[int index] => entries[index];

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => entries.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
