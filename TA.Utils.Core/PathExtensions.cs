// This file is part of the TA.Utils project
// Copyright (c) 2016-2026 Timtek Systems Limited, all rights reserved.
// File: PathExtensions.cs  Last modified: 2026-01-27 by Tim Long

using System;
using System.IO;

namespace TA.Utils.Core
{
    /// <summary>
    ///     Extension methods and utilities for working with file system paths.
    /// </summary>
    public static class PathExtensions
    {
        /// <summary>
        ///     Expands a path string, supporting environment variables, PowerShell-style
        ///     tilde for the current user home directory, and relative paths.
        /// </summary>
        /// <param name="path">The path string to expand.</param>
        /// <returns>The fully expanded and normalized absolute path.</returns>
        public static string ExpandPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            var expanded = Environment.ExpandEnvironmentVariables(path);

            if (expanded == "~")
                return Path.GetFullPath(GetUserHomeDirectory());

            if (IsPathRelativeToUserHome(expanded))
            {
                var home = GetUserHomeDirectory();
                var remainder = expanded.Substring(2);
                var combined = string.IsNullOrEmpty(remainder) ? home : Path.Combine(home, remainder);
                return Path.GetFullPath(combined);
            }

            if (!Path.IsPathRooted(expanded))
            {
                var cwd = Directory.GetCurrentDirectory();
                return Path.GetFullPath(Path.Combine(cwd, expanded));
            }

            return Path.GetFullPath(expanded);
        }

        /// <summary>
        ///     Gets the current user home directory path.
        /// </summary>
        public static string GetUserHomeDirectory() =>
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        /// <summary>
        ///     Determines whether the path is relative to the current user home directory.
        /// </summary>
        public static bool IsPathRelativeToUserHome(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Length < 2)
                return false;

            if (path[0] != '~')
                return false;

            var next = path[1];
            return next == Path.DirectorySeparatorChar || next == Path.AltDirectorySeparatorChar;
        }
    }
}
