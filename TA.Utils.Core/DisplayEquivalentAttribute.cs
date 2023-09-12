// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: DisplayEquivalentAttribute.cs  Last modified: 2020-07-13@02:11 by Tim Long

using System;

namespace TA.Utils.Core
    {
    /// <summary>
    ///     When applied to an enum member or field, specifies a string that should be used for display
    ///     purposes instead of the identifier name. This can be useful within code that must render HTML
    ///     markup from an enumerated type.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    /// <seealso cref="EnumExtensions.DisplayEquivalent" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public sealed class DisplayEquivalentAttribute : Attribute
        {
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:TA.Utils.Core.DisplayEquivalentAttribute" /> class.
        /// </summary>
        /// <param name="text">The text to be shown on the display instead of the value name.</param>
        public DisplayEquivalentAttribute(string text) => Value = text;

        /// <summary>Gets the display text value.</summary>
        /// <value>The display text to be used instead of the enumerated value name.</value>
        public string Value { get; }
        }
    }