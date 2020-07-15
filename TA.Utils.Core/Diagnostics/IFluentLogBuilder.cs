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
// File: IFluentLogBuilder.cs  Last modified: 2020-07-14@07:01 by Tim Long

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TA.Utils.Core.Diagnostics
    {
    public interface IFluentLogBuilder
        {
        IFluentLogBuilder Exception(Exception exception);

        IFluentLogBuilder LoggerName(string loggerName);

        IFluentLogBuilder Message(string message);

        IFluentLogBuilder Message(string format, params object[] args);

        IFluentLogBuilder Message(IFormatProvider provider, string format, params object[] args);

        IFluentLogBuilder Property(string name, object value);

        IFluentLogBuilder Properties(IDictionary<string,object> properties);

        IFluentLogBuilder TimeStamp(DateTime timeStamp);

        IFluentLogBuilder StackTrace(StackTrace stackTrace, int userStackFrame);

        void Write([CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = default);

        void WriteIf(Func<bool> condition, [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = default);

        void WriteIf(bool condition, [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = default);
        }
    }