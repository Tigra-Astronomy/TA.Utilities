// This file is part of the TA.Utils project
// Copyright © 2015-2023 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: ConsoleLoggerService.cs  Last modified: 2023-11-13@10:22 by Tim Long

using System;
using JetBrains.Annotations;

namespace TA.Utils.Core.Diagnostics
    {
        /// <summary>
        ///     A logging service that emits output directly to the console.
        ///     The service is completely self-contained and does not rely on any external logging back-end.
        ///     Useful as a light-weight solution for console applications.
        /// </summary>
        [UsedImplicitly]
    public class ConsoleLoggerService : ILog
        {
            private readonly ConsoleLoggerOptions options;

            /// <summary>
            ///     Initialize the instance and optionally inject options.
            ///     If no options are supplied, then <see cref="ConsoleLoggerOptions.DefaultOptions" /> is used.
            /// </summary>
            /// <param name="options">Options that configure the logging service.</param>
            public ConsoleLoggerService(ConsoleLoggerOptions options = null) =>
                this.options = options ?? ConsoleLoggerOptions.DefaultOptions;

            /// <inheritdoc />
        public IFluentLogBuilder Trace(int verbosity = 0, string sourceNameOverride = null)
        {
            return new ConsoleLogBuilder(options, ConsoleLogSeverity.Trace);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Debug(int verbosity = 0, string sourceNameOverride = null)
        {
            return new ConsoleLogBuilder(options, ConsoleLogSeverity.Debug);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Info(int verbosity = 0, string sourceNameOverride = null)
        {
            return new ConsoleLogBuilder(options, ConsoleLogSeverity.Info);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Warn(int verbosity = 0, string sourceNameOverride = null)
        {
            return new ConsoleLogBuilder(options, ConsoleLogSeverity.Warning);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Error(int verbosity = 0, string sourceNameOverride = null)
        {
            return new ConsoleLogBuilder(options, ConsoleLogSeverity.Error);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Fatal(int verbosity = 0, string sourceNameOverride = null)
        {
            return new ConsoleLogBuilder(options, ConsoleLogSeverity.Fatal);
        }

        /// <inheritdoc />
        public IFluentLogBuilder Level(string levelName, int verbosity = 0, string sourceNameOverride = null) =>
            throw new NotImplementedException();

        /// <inheritdoc />
        public void Shutdown() { }

        /// <inheritdoc />
        public ILog WithAmbientProperty(string name, object value)
        {
            return this;
        }

        /// <inheritdoc />
        public ILog WithName(string logSourceName)
            {
            throw new NotImplementedException();
            }
        }
    }