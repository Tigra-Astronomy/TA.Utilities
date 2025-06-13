// This file is part of the BeaconFinalTest project
// Copyright © Ocean Signal Limited, all rights reserved.
// 
// Company Confidential
// 
// File: KeyValueTextFileReader.cs  Last modified: 2023-11-06@12:49 by Tim Long

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace TA.Utils.Core.PropertyBinding;

/// <summary>
///     A reader that can read key-value formatted text from a stream into a collection of KeyValueDataRecord records.
/// </summary>
public class KeyValueReader : IDisposable
{
    private readonly Stream input;
    private static readonly char[] DefaultDelimiters = { ':', '=', '#' };
    private static readonly char[] DefaultCommentChars = { '#' };
    private readonly char[] delimiters; // key/value delimiters
    private readonly char[] commentChars; // Opens a comment if the first non-whitespace character on a line.

    /// <summary>
    ///     Initializes a new instance of the <see cref="KeyValueReader" /> class.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <param name="delimiters">The characters used to delimit the keys and values. Optional; default = { ':', '=', '#' }</param>
    /// <param name="commentChars">
    ///     Characters that will be treated as opening a comment line if they are the first
    ///     non-whitespace character on a line. Optional; default= {'#'}. Comment lines are ignored.
    /// </param>
    public KeyValueReader(Stream input, char[]? delimiters = null, char[]? commentChars = null)
    {
        this.delimiters = delimiters ?? DefaultDelimiters;
        this.commentChars = commentChars ?? DefaultCommentChars;
        this.input = input;
    }

    /// <summary>
    ///     Gets an enumerable collection of key-value pairs from the supplied stream.
    ///     Lines in the input file that cannot be transformed into a key-value pair are simply ignored.
    /// </summary>
    /// <returns><see cref="IEnumerable{KeyValueDataRecord}" />.</returns>
    public IEnumerable<KeyValueDataRecord> KeyValueDataRecords()
    {
        using var reader = new StreamReader(input);
        do
        {
            var line = reader.ReadLine();
            if (line == null)
                yield break; // end of file reached.
            var trimmedLine = line.Trim();
            if (trimmedLine.Length < 3)
                continue; // Nothing left after trimming! Must have at least one char for each of key, delimiter, value.
            if (commentChars.Contains(trimmedLine[0])) // comment line - ignore
                continue;
            var parts = line.Split(delimiters); // split keys from values
            if (parts.Length >= 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                    continue; // We must have data in both the key and the value.
                var keyValuePair = new KeyValueDataRecord(key, value);
                yield return keyValuePair;
            }
        } while (true);
    }

    /// <summary>
    ///     Disposes resources, including the input stream.
    /// </summary>
    public void Dispose()
    {
        input.Dispose();
    }
}