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
// File: ILog.cs  Last modified: 2020-07-14@06:59 by Tim Long

using System.Runtime.CompilerServices;

namespace TA.Utils.Core.Diagnostics
{
    /// <summary>
    /// Logging service interface
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Creates a log builder for a log entry with Trace severity.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Trace([CallerFilePath] string callerFilePath = null);

        /// <summary>
        /// Creates a log builder for a log entry with Debug severity.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Debug([CallerFilePath] string callerFilePath = null);

        /// <summary>
        /// Creates a log builder for a log entry with Information severity.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Info([CallerFilePath] string callerFilePath = null);

        /// <summary>
        /// Creates a log builder for a log entry with Warning severity.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Warn([CallerFilePath] string callerFilePath = null);

        /// <summary>
        /// Creates a log builder for a log entry with Error severity.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Error([CallerFilePath] string callerFilePath = null);

        /// <summary>
        /// Creates a log builder for a log entry with Fatal severity.
        /// Writing a Fatal log entry also terminates the process.
        /// </summary>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <returns>IFluentLogBuilder.</returns>
        IFluentLogBuilder Fatal([CallerFilePath] string callerFilePath = null);

        /// <summary>
        /// Instructs the logging service to shut down.
        /// This should flush any buffered log entries and close any open files or streams.
        /// It is best practice to call <c>Shutdown</c> before exiting from the program.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Sets an "ambient property" that should be included in all log events.
        /// Once added, the property persists for the lifetim of the instance.
        /// Useful for loggers that support semantic logging.
        /// </summary>
        ILog WithAmbientProperty(string name, object value);
    }
}