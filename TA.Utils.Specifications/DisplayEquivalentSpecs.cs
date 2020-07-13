// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: DisplayEquivalentSpecs.cs  Last modified: 2020-07-13@02:11 by Tim Long

using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications
    {
    [Subject(typeof(DisplayEquivalentAttribute))]
    internal class when_displaying_an_enum_with_equivalent_text
        {
        It should_have_equivalent_text_when_the_attribute_is_present = () =>
            TestCases.CaseWithEquivalentText.DisplayEquivalent().ShouldEqual("Equivalent Text");
        It should_use_the_field_name_when_no_attribute_is_present = () =>
            TestCases.CaseWithoutEquivalentText.DisplayEquivalent()
                .ShouldEqual(nameof(TestCases.CaseWithoutEquivalentText));
        }

    internal enum TestCases
        {
        [DisplayEquivalent("Equivalent Text")] CaseWithEquivalentText,
        CaseWithoutEquivalentText
        }
    }