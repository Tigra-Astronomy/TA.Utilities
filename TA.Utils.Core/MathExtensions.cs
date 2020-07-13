// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: MathExtensions.cs  Last modified: 2020-07-13@02:11 by Tim Long

using System;

namespace TA.Utils.Core
    {
    /// <summary>Helper methods for mathematical constants, operations and algorithms</summary>
    public static class MathExtensions
        {
        /// <summary>Clips (constrains) a value to within the specified range.</summary>
        /// <typeparam name="T">A type that implements <see cref="IComparable" />.</typeparam>
        /// <param name="input">The input value.</param>
        /// <param name="minimum">The minimum allowed value.</param>
        /// <param name="maximum">The maximum allowed value.</param>
        /// <returns>The input value clipped to the specified range.</returns>
        public static T Clip<T>(this T input, T minimum, T maximum)
            where T : IComparable
            {
            if (input.CompareTo(maximum) > 0)
                return maximum;
            if (input.CompareTo(minimum) < 1)
                return minimum;
            return input;
            }
        }
    }