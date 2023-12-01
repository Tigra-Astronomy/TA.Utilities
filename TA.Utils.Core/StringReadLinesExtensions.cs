// This file is part of the TA.Utils project
// Copyright © 2015-2023 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: StringReadLinesExtensions.cs  Last modified: 2023-11-13@17:19 by Tim Long

using System.Collections.Generic;
using System.IO;

namespace TA.Utils.Core;

/// <summary>
///     Extensions for splitting newline-separated text into an enumerable collection of lines.
/// </summary>
public static class StringReadLinesExtension
{
    /// <summary>
    ///     Splits a string into lines at newline characters and returns an enumerable collection of the lines.
    /// </summary>
    /// <remarks>
    ///     A line is defined as a sequence of characters followed by a carriage return (0x000d), a line feed (0x000a), a
    ///     carriage return followed by a line feed, <c>Environment.NewLine</c>, or the end of the string. The string that
    ///     is returned does not contain the terminating carriage return or line feed.
    /// </remarks>
    /// <param name="text">The source string containing zero or more lines of text.</param>
    /// <returns>An <see cref="IEnumerable{String}" /> containing the lines of text.</returns>
    public static IEnumerable<string> GetLines(this string text)
        {
        return GetLines(new StringReader(text));
        }

    /// <summary>
    ///     Parses a stream of text into lines (broken on newline characters) and returns the lines as an enumerable collection
    ///     of strings.
    /// </summary>
    /// <remarks>
    ///     A line is defined as a sequence of characters followed by a carriage return (0x000d), a line feed (0x000a), a
    ///     carriage return followed by a line feed, <c>Environment.NewLine</c>, or the end-of-stream marker. The string that
    ///     is returned does not contain the terminating carriage return or line feed.
    /// </remarks>
    /// <param name="stm">The readable source stream.</param>
    /// <returns>An <see cref="IEnumerable{String}" /> containing lines of text from the source stream.</returns>
    public static IEnumerable<string> GetLines(this Stream stm)
        {
        return GetLines(new StreamReader(stm));
        }

    /// <summary>
    ///     Gets an <see cref="IEnumerable{String}" /> that returns each line in the original text as a separate string.
    /// </summary>
    /// <remarks>
    ///     A line is defined as a sequence of characters followed by a carriage return (0x000d), a line feed (0x000a), a
    ///     carriage return followed by a line feed, <c>Environment.NewLine</c>, or the end-of-stream marker. The string that
    ///     is returned does not contain the terminating carriage return or line feed.
    /// </remarks>
    /// <param name="reader">A <see cref="TextReader" /> that will be used to read the source content.</param>
    /// <returns>An enumerable collection of lines of text.</returns>
    public static IEnumerable<string> GetLines(this TextReader reader)
        {
        string line;
        while ((line = reader.ReadLine()) != null)
            yield return line;
        reader.Dispose();
        }
    }