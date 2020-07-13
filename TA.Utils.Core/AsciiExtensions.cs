// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: AsciiExtensions.cs  Last modified: 2020-07-13@02:11 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace TA.Utils.Core
    {
    /// <summary>Helper methods for manipulating strings of ASCII-encoded text.</summary>
    public static class AsciiExtensions
        {
        /// <summary>Expands non-printable ASCII characters into mnemonic human-readable form.</summary>
        /// <returns>Returns a new string with non-printing characters replaced by human-readable mnemonics.</returns>
        public static string ExpandAscii(this string text)
            {
            Contract.Requires(text != null);
            Contract.Ensures(Contract.Result<string>() != null);
            var expanded = new StringBuilder();
            foreach (char c in text)
                {
                byte b = (byte) c;
                string strAscii = Enum.GetName(typeof(AsciiSymbols), b);
                if (strAscii != null)
                    expanded.Append("<" + strAscii + ">");
                else
                    expanded.Append(c);
                }

            return expanded.ToString();
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
    }