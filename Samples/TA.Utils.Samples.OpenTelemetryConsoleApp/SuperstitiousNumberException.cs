// This file is part of the TA.Utils project
// Copyright © 2015-2026 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: SuperstitiousNumberException.cs  Last modified: 2026-02-23 by tim.long

namespace TA.Utils.Samples.OpenTelemetryConsoleApp;

/// <summary>
///     Thrown when a number associated with superstition is encountered.
/// </summary>
public class SuperstitiousNumberException : Exception
{
    public SuperstitiousNumberException() { }

    public SuperstitiousNumberException(string message) : base(message) { }

    public SuperstitiousNumberException(string message, Exception inner) : base(message, inner) { }
}
