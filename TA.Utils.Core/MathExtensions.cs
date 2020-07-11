// This file is part of the TA.NexDome.AscomServer project
// Copyright © 2019-2019 Tigra Astronomy, all rights reserved.

using System;

namespace TA.Utils.Core
    {
    public static class MathExtensions
        {
        /// <summary>Clips (constrains) a value to within the specified range.</summary>
        /// <typeparam name="T">A type that implements <see cref="IComparable"/>.</typeparam>
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