// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: MathExtensionSpecs.cs  Last modified: 2020-07-13@02:11 by Tim Long

using JetBrains.Annotations;
using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications
    {
    #region Context base classes
    class with_clip_test_context
        {
        [UsedImplicitly] protected static readonly SemanticVersion MaximumVersion = new("9.8.7");
        [UsedImplicitly] protected static readonly SemanticVersion MinimumVersion = new("3.2.1");
        }
    #endregion

    [Subject(typeof(MathExtensions))]
    internal class when_clipping_a_value_above_maximum : with_clip_test_context
        {
        It should_clip_at_maximum = () => testCase.Clip(MinimumVersion, MaximumVersion).ShouldEqual(MaximumVersion);
        static SemanticVersion testCase = new("9.8.8");
        }

    [Subject(typeof(MathExtensions))]
    internal class when_clipping_a_value_below_minimum : with_clip_test_context
        {
        It should_clip_at_minimum = () => testCase.Clip(MinimumVersion, MaximumVersion).ShouldEqual(MinimumVersion);
        static SemanticVersion testCase = new("1.0.0");
        }

    [Subject(typeof(MathExtensions))]
    internal class when_clipping_a_value_within_range : with_clip_test_context
        {
        It should_not_clip = () => testCase.Clip(MinimumVersion, MaximumVersion).ShouldEqual(testCase);
        static SemanticVersion testCase = new("4.5.6");
        }
    }