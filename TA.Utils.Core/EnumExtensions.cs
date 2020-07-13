// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: EnumExtensions.cs  Last modified: 2020-07-13@02:11 by Tim Long

using System;
using System.Linq;
using System.Reflection;

namespace TA.Utils.Core
    {
    /// <summary>Helper methods for use with enumerated types.</summary>
    public static class EnumExtensions
        {
        /// <summary>
        ///     Returns the equivalent display text for a given Enum value. Specifiy display text by placing
        ///     the <see cref="DisplayEquivalentAttribute" /> attribute on the enumeration's member fields.
        /// </summary>
        /// <remarks>Inspired by: http://blogs.msdn.com/b/abhinaba/archive/2005/10/21/483337.aspx</remarks>
        /// <param name="en">The enumerated value.</param>
        /// <returns>Text suitable for use in a display or user interface.</returns>
        /// <seealso cref="DisplayEquivalentAttribute" />
        public static string DisplayEquivalent(this Enum en)
            {
            string enumValueName = en.ToString();
            var type = en.GetType();
            var fields = type.GetTypeInfo().DeclaredFields;
            var fieldInfo = fields.Single(p => p.Name == enumValueName);
            var attribute =
                (DisplayEquivalentAttribute) fieldInfo?.GetCustomAttribute(typeof(DisplayEquivalentAttribute));
            if (fieldInfo == null || attribute == null)
                return enumValueName;
            string displayEquivalent = attribute.Value;
            return string.IsNullOrWhiteSpace(displayEquivalent) ? enumValueName : displayEquivalent;
            }
        }
    }