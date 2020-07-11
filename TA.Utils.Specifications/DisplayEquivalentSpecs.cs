
using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications
    {
    internal enum TestCases
        {
        [DisplayEquivalent("Equivalent Text")]
        CaseWithEquivalentText,
        CaseWithoutEquivalentText
        }

    [Subject(typeof(DisplayEquivalentAttribute))]
    internal class when_displaying_an_enum_with_equivalent_text
        {
        It should_have_equivalent_text_when_the_attribute_is_present = () =>
            TestCases.CaseWithEquivalentText.DisplayEquivalent().ShouldEqual("Equivalent Text");
        It should_use_the_field_name_when_no_attribute_is_present = () =>
            TestCases.CaseWithoutEquivalentText.DisplayEquivalent()
                .ShouldEqual(nameof(TestCases.CaseWithoutEquivalentText));
        }
    }