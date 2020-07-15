// This file is part of the TA.Ascom.ReactiveCommunications project
//
// Copyright © 2015-2020 Tigra Astronomy, all rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
//
// File: DegenerateLogBuilder.cs  Last modified: 2020-07-14@09:03 by Tim Long

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TA.Utils.Core.Diagnostics
    {
    public sealed class DegenerateLogBuilder : IFluentLogBuilder
        {
        /// <inheritdoc />
        public IFluentLogBuilder Exception(Exception exception) => this;

        /// <inheritdoc />
        public IFluentLogBuilder LoggerName(string loggerName) => this;

        /// <inheritdoc />
        public IFluentLogBuilder Message(string message) => this;

        /// <inheritdoc />
        public IFluentLogBuilder Message(string format, params object[] args) => this;

        /// <inheritdoc />
        public IFluentLogBuilder Message(IFormatProvider provider, string format, params object[] args) => this;

        /// <inheritdoc />
        public IFluentLogBuilder Property(string name, object value) => this;

        /// <inheritdoc />
        public IFluentLogBuilder Properties(IDictionary<string, object> properties) => this;

        /// <inheritdoc />
        public IFluentLogBuilder TimeStamp(DateTime timeStamp) => this;

        /// <inheritdoc />
        public IFluentLogBuilder StackTrace(StackTrace stackTrace, int userStackFrame) => this;

        /// <inheritdoc />
        public void Write(string callerMemberName = null, string callerFilePath = null,
            int callerLineNumber = default) { }

        /// <inheritdoc />
        public void WriteIf(Func<bool> condition, string callerMemberName = null, string callerFilePath = null,
            int callerLineNumber = default) { }

        /// <inheritdoc />
        public void WriteIf(bool condition, string callerMemberName = null, string callerFilePath = null,
            int callerLineNumber = default) { }
        }
    }