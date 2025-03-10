// This file is part of the TA.Utils project
// Copyright © 2015-2023 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: LogServiceOptions.cs  Last modified: 2023-09-01@20:23 by Tim Long

namespace TA.Utils.Logging.NLog
{
    /// <summary>
    ///     Options that affect the operation of the <see cref="LoggingService" />.
    /// </summary>
    public class LogServiceOptions
    {
        internal const string VerbosityDefaultPropertyName   = "verbosity";
        internal const string CustomLevelDefaultPropertyName = "CustomLevel";
        internal       bool   VerbosityEnabled;
        internal       string VerbosityPropertyName = VerbosityDefaultPropertyName;

        private LogServiceOptions() { } // prevent use of the  except within the class itself.

        /// <summary>
        ///     Returns an instance loaded with default options, which can then be modified.
        ///     This is the primary creation mechanism for options.
        /// </summary>
        public static LogServiceOptions DefaultOptions => new LogServiceOptions();

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
        /// <returns>The <see cref="LogServiceOptions" /> instance.</returns>
        public LogServiceOptions UseVerbosity(string verbosityPropertyName = VerbosityDefaultPropertyName)
        {
            VerbosityEnabled = true;
            VerbosityPropertyName = verbosityPropertyName;
            return this;
        }

        /// <summary>
        ///     Set the name of the log property that will be used for custom severity levels.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public LogServiceOptions CustomSeverityPropertyName(string propertyName = CustomLevelDefaultPropertyName)
        {
            CustomLevelPropertyName = propertyName;
            return this;
        }

        /// <summary>
        ///     The name of the property that will hold any custom severity level.
        /// </summary>
        public string CustomLevelPropertyName { get; set; } = CustomLevelDefaultPropertyName;
    }
}