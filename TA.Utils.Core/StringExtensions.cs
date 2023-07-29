// This file is part of the TA.Utils project
// Copyright © 2016-2023 Tigra Astronomy, all rights reserved.
// File: StringExtensions.cs  Last modified: 2023-07-29@12:34 by Tim Long

using System;
using System.Linq;
using System.Text;

namespace TA.Utils.Core
    {
    /// <summary>
    ///     String extension methods.
    /// </summary>
    /// <remarks>
    ///     Useful string manipulation convenience methods.
    /// </remarks>
    internal static class StringExtensions
        {
        /// <summary>
        ///     Removes all unwanted characters from a string.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="clean">A list of the unwanted characters. All other characters will be preserved.</param>
        /// <returns>
        ///     A new string with all of the unwanted characters deleted. Returns <see cref="string.Empty" />
        ///     if all of the characters were deleted or if the source string was null or empty.
        /// </returns>
        /// <remarks>
        ///     Contrast with <see cref="Keep" />
        /// </remarks>
        /// <seealso cref="Keep" />
        public static string Clean(this string source, string clean)
            {
            if (string.IsNullOrEmpty(source))
                return string.Empty;
            var cleanString = new StringBuilder(source.Length);
            foreach (var ch in source)
                if (!clean.Contains(ch))
                    cleanString.Append(ch);
            return cleanString.ToString();
            }

        /// <summary>
        ///     Returns the specified number of characters from the head of a string.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="length">The number of characters to be returned, must not be greater than the length of the string.</param>
        /// <returns>The specified number of characters from the head of the source string, as a new string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the requested number of characters exceeds the string length.</exception>
        public static string Head(this string source, int length)
            {
            if (length > source.Length)
                throw new ArgumentOutOfRangeException("source",
                    "The specified length is greater than the length of the string.");
            return source.Substring(0, length);
            }

        /// <summary>
        ///     Keeps only the wanted (that is, removes all unwanted characters) from the string.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="keep">A list of the wanted characters. All other characters will be removed.</param>
        /// <returns>
        ///     A new string with all of the unwanted characters deleted. Returns <see cref="string.Empty" /> if all
        ///     the characters were deleted or if the source string was null or empty.
        /// </returns>
        /// <seealso cref="Clean" />
        public static string Keep(this string source, string keep)
            {
            if (string.IsNullOrEmpty(source))
                return string.Empty;
            var cleanString = new StringBuilder(source.Length);
            foreach (var ch in source)
                if (keep.Contains(ch))
                    cleanString.Append(ch);
            return cleanString.ToString();
            }

        /// <summary>
        ///     Remove the head of the string, leaving the tail.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="length">Number of characters to remove from the head.</param>
        /// <returns>A new string containing the old string with <paramref name="length" /> characters removed from the head.</returns>
        public static string RemoveHead(this string source, int length)
            {
            if (length < 1)
                return source;
            return source.Tail(source.Length - length);
            }

        /// <summary>
        ///     Remove the tail of the string, leaving the head.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="length">Number of characters to remove from the tail.</param>
        /// <returns>A new string containing the old string with <paramref name="length" /> characters removed from the tail.</returns>
        public static string RemoveTail(this string source, int length)
            {
            if (length < 1)
                return source;
            return source.Head(source.Length - length);
            }

        /// <summary>
        ///     Returns the specified number of characters from the tail of a string.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="length">The number of characters to be returned, must not be greater than the length of the string.</param>
        /// <returns>The specified number of characters from the tail of the source string, as a new string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the requested number of characters exceeds the string length.</exception>
        public static string Tail(this string source, int length)
            {
            var srcLength = source.Length;
            if (length > srcLength)
                throw new ArgumentOutOfRangeException("source",
                    "The specified length is greater than the length of the string.");
            return source.Substring(srcLength - length, length);
            }

        /// <summary>
        ///     Converts a tring to a hex representation, suitable for display in a debugger.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>A new string showing each character of the source string as a hex digit.</returns>
        public static string ToHex(this string source)
            {
            const string formatWithSeperator = ", {0,2:x}";
            const string formatNoSeperator = "{0,2:x}";
            var hexString = new StringBuilder(source.Length * 7);
            hexString.Append('{');
            var seperator = false;
            foreach (var ch in source)
                {
                hexString.AppendFormat(seperator ? formatWithSeperator : formatNoSeperator, (int)ch);
                seperator = true;
                }
            hexString.Append('}');
            return hexString.ToString();
            }
        }
    }