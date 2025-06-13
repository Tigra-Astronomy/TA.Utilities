// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: AsciiExtensions.cs  Last modified: 2020-07-13@02:11 by Tim Long

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace TA.Utils.Core;

/// <summary>Helper methods for manipulating strings of ASCII-encoded text.</summary>
public static class AsciiExtensions
{
    /// <summary>Expands non-printable ASCII characters into mnemonic human-readable form.</summary>
    /// <returns>Returns a new string with non-printing characters replaced by human-readable mnemonics.</returns>
    public static string ExpandAscii(this string? text) => new(ExpandAscii(text.AsEnumerable()).ToArray());

    /// <summary>
    ///     Expands non-printable ASCII characters in the given sequence into mnemonic human-readable form.
    /// </summary>
    /// <param name="sequence">The sequence of characters to process. Can be <c>null</c>.</param>
    /// <returns>
    ///     An <see cref="IEnumerable{T}" /> of characters where non-printable ASCII characters are replaced
    ///     by their human-readable mnemonics enclosed in angle brackets (&lt; and &gt;).
    /// </returns>
    /// <remarks>
    ///     If the input sequence is <c>null</c>, the method returns an empty sequence.
    ///     Printable characters in the input sequence are returned unchanged.
    /// </remarks>
    public static IEnumerable<char> ExpandAscii(this IEnumerable<char>? sequence)
    {
        if (sequence is null) yield break;
        foreach (var c in sequence)
        {
            var b = (byte)c;
            var strAscii = Enum.GetName(typeof(AsciiSymbols), b);
            if (strAscii != null)
            {
                yield return '<';
                foreach (var expandedChar in strAscii) yield return expandedChar;

                yield return '>';
            }
            else
            {
                yield return c;
            }
        }
    }


    /// <summary>
    ///     Expands a non-printable ASCII character into mnemonic human-readable form. If the character is
    ///     printable, then the character is returned as a string.
    /// </summary>
    /// <returns>Returns a new string with non-printing characters replaced by human-readable mnemonics.</returns>
    /// <remarks>To convert a whole string on one go, use the overload that accepts a string.</remarks>
    public static string ExpandAscii(this char c)
    {
        Contract.Ensures(Contract.Result<string>() != null);
        return c.ToString().ExpandAscii();
    }
}