// This file is part of the TA.Utils project
// Copyright © 2015-2024 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: ConsoleLoggerOptions.cs  Last modified: 2024-2-6@11:37 by tim.long

namespace TA.Utils.Core.Diagnostics;

/// <summary>
///     Options that affect the operation of the <see cref="ConsoleLoggerService" />.
/// </summary>
public class ConsoleLoggerOptions
{
    private const string VerbosityDefaultPropertyName = "verbosity";
    internal bool VerbosityEnabled;
    internal string VerbosityPropertyName = VerbosityDefaultPropertyName;
    internal bool renderProperties = true;

    private ConsoleLoggerOptions() { } // prevent use of the constructor except within the class itself.

    /// <summary>
    ///     Returns an instance loaded with default options, which can then be modified.
    ///     This is the primary creation mechanism for options.
    /// </summary>
    public static ConsoleLoggerOptions DefaultOptions => new();

    /// <summary>
    ///     Enables the use of "verbosity".
    ///     Causes a property to be added to each log entry containing a verbosity level, which defaults to 0.
    ///     The verbosity of each entry can be set when creating a log entry, by passing it as a parameter to the severity
    ///     builder method.
    ///     <example>
    ///         To create a log entry with severity Info and verbosity 2, proceed as follows:
    ///         <c>Log.Info(2).Message("sets verbosity 2").Write();</c>
    ///     </example>
    /// </summary>
    /// <param name="verbosityPropertyName">The name of the verbosity property.. Optional; defaults to "verbosity".</param>
    /// <returns>The <see cref="ConsoleLoggerOptions" /> instance.</returns>
    public ConsoleLoggerOptions UseVerbosity(string verbosityPropertyName = VerbosityDefaultPropertyName)
    {
        VerbosityEnabled = true;
        VerbosityPropertyName = verbosityPropertyName;
        return this;
    }

    /// <summary>
    ///     Controls the rendering of properties to the console output.
    ///     This feature is on by default, but can be a bit overwhelming as it produces a lot of output.
    ///     Disable with <c>.RenderProperties(false)</c>.
    /// </summary>
    /// <param name="enable">Whether to enable (true) or disable (false) property rendering.</param>
    /// <returns>The updated <see cref="ConsoleLoggerOptions" /> instance.</returns>
    public ConsoleLoggerOptions RenderProperties(bool enable = true)
    {
        renderProperties = enable;
        return this;
    }
}
