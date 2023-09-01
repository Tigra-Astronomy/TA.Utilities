// This file is part of the TA.Utils project
// Copyright © 2015-2023 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: Program.cs  Last modified: 2023-09-01@10:58 by Tim Long

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TA.Utils.Core;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Logging.NLog.SampleConsoleApp;

/// <summary>
///     This program demonstrates how to use the <see cref="ILog" /> logging service
///     and examples of structured or semantic logging. Traditional logging usually
///     involves writing out a simple string, but with semantic logging we can
///     include all sorts of data in every log event. For example, when logging an
///     exception the full exception data including the stack trace is saved to the log.
///     In this example we are using the NLog back-end and sending the log entries
///     to the console (with color highlighting) as well as to a cloud log collector.
///     NLog is configured in the NLog.config file.
/// </summary>
internal class Program
{
    private static readonly List<int> SuperstitiousNumbers = new() { 13, 7, 666, 3, 8, 88, 888 };

    private static async Task Main(string[] args)
    {
        // Create a logging service with an ambient property "CorrelationId".
        // This ID will then appear in all log entries, and can be used to find all entries
        // related to a particular run of the program.
        // If this doesn't seem useful, try running two simultaneous instances of the program
        // and then try to sort out which log entry came from which instance.
        // The correlation ID makes this trivial.
        var options = LogServiceOptions.DefaultOptions.UseVerbosity();
        var log = new LoggingService(options).WithAmbientProperty("CorrelationId", Guid.NewGuid());
        log.Info()
            .Message("Application starting - version {Version}", GitVersion.GitInformationalVersion)
            .Property("SemVer", GitVersion.GitFullSemVer)
            .Property("GitCommit", GitVersion.GitCommitSha)
            .Property("CommitDate", GitVersion.GitCommitDate)
            .Write();
        var seed = DateTime.Now.Millisecond;
        var gameOfChance = new Random(seed);
        log.Debug().Message("Random seed {seed}", seed).Write();

        for (var i = 0; i < 1000; i++)
        {
            try
            {
                log.Debug(1).Message("Starting iteration {iteration}", i).Write();

                /*
                 * The program doesn't like numbers associated with superstition,
                 * and will flag them up as warnings.
                 */
                if (SuperstitiousNumbers.Contains(i))
                    throw new SuperstitiousNumberException($"Skipping {i} because it is a superstitious number");

                // There's a small chance of a random "failure"
                if (gameOfChance.Next(100) < 3)
                    throw new ApplicationException("Random failure");
            }
            catch (SuperstitiousNumberException ex)
            {
                log.Warn()
                    .Message("Superstitious looking number: {number}", i)
                    .Exception(ex)
                    .Property("SuperstitiousNumbers", SuperstitiousNumbers)
                    .Write();
            }
            catch (ApplicationException ae)
            {
                log.Error().Exception(ae).Message("Failed iteration {iteration}", i).Write();
            }

            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            log.Debug(1).Message("Finished iteration {iteration}", i).Write();
        }

        log.Info().Message("Program terminated").Write();
        log.Shutdown();
    }
}