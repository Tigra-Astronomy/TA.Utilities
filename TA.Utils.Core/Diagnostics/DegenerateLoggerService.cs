﻿// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2015-2020 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: DegenerateLoggerService.cs  Last modified: 2020-07-14@09:02 by Tim Long

namespace TA.Utils.Core.Diagnostics
{
    /// <summary>
    ///     This is the default logging service used if non is supplied by the user. The service does
    ///     nothing and produces no output. It is essentially "logging disabled".
    /// </summary>
    public sealed class DegenerateLoggerService : ILog
    {
        private static readonly IFluentLogBuilder builder = new DegenerateLogBuilder();

        /// <inheritdoc />
        public IFluentLogBuilder Trace(int verbosity = 0, string sourceNameOverride = null)
        {
            return builder;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Debug(int verbosity = 0, string sourceNameOverride = null)
        {
            return builder;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Info(int verbosity = 0, string sourceNameOverride = null)
        {
            return builder;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Warn(int verbosity = 0, string sourceNameOverride = null)
        {
            return builder;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Error(int verbosity = 0, string sourceNameOverride = null)
        {
            return builder;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Fatal(int verbosity = 0, string sourceNameOverride = null)
        {
            return builder;
        }

        /// <inheritdoc />
        public IFluentLogBuilder Level(string levelName, int verbosity = 0, string sourceNameOverride = null) => builder;

        /// <inheritdoc />
        public void Shutdown() { }

        /// <inheritdoc />
        public ILog WithAmbientProperty(string name, object value) => this;

        /// <inheritdoc />
        public ILog WithName(string logSourceName) => this;
    }
}