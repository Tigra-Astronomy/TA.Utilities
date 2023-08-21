// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: Program.cs  Last modified: 2020-07-16@19:24 by Tim Long

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TA.Utils.Core;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Logging.NLog.SampleConsoleApp
    {
    /// <summary>
    /// This program demonstrates how to use the <see cref="ILog"/> logging service
    /// and examples of structured or semantic logging. Traditional logging usually
    /// involves writing out a simple string, but with semantic logging we can
    /// include all sorts of data in every log event. For example, when logging an
    /// exception the full exception data including the stack trace is saved to the log.
    /// In this example we are using the NLog back-end and sending the log entries
    /// to the console (with color highlighting) as well as to a cloud log collector.
    /// NLog is configured in the NLog.config file.
    /// </summary>
    class Program
        {
        static readonly List<int> SuperstitiousNumbers = new List<int> {13, 7, 666, 3, 8, 88, 888};

        static async Task Main(string[] args)
            {
            var log = new LoggingService();
            log.Info()
                .Message("Application stating - version {Version}", GitVersion.GitInformationalVersion)
                .Property("SemVer", GitVersion.GitFullSemVer)
                .Property("GitCommit", GitVersion.GitCommitSha)
                .Property("CommitDate", GitVersion.GitCommitDate)
                .Write();
            var seed = DateTime.Now.Millisecond;
            var gameOfChance = new Random(seed);
            log.Debug().Property("seed",seed).Write();

            for (int i = 0; i < 1000; i++)
                {
                try
                    {
                    log.Debug().Message("Starting iteration {iteration}", i).Write();

                    /*
                     * The program doesn't like numbers associated with superstition,
                     * and will flag them up as warnings.
                     */
                    if (SuperstitiousNumbers.Contains(i))
                        {
                        throw new SuperstitiousNumberException($"Skipping {i} because it is a superstitious number");
                        }

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
                log.Debug().Message("Finished iteration {iteration}", i).Write();
                }
            log.Info().Message("Program terminated").Write();
            log.Shutdown();
            }
        }
    }