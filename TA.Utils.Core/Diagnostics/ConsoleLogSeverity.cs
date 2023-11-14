// This file is part of the TA.Utils project
// Copyright © 2015-2023 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: ConsoleLogSeverity.cs  Last modified: 2023-11-13@11:37 by Tim Long

namespace TA.Utils.Core.Diagnostics
    {
    /// <summary>
    ///     The severity of a console log entry.
    /// </summary>
    public enum ConsoleLogSeverity
        {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        Fatal
        }
    }