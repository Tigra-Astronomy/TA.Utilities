# Display Equivalence for Enumerated Types

The `[DisplayEquivalent("text")]` Attribute works with the `EnumExtensions.DisplayEquivalent()` extension method.
This can be useful for building drop-down lists and Combo box contents for enumerated types.
You can always get the equivalent human-readable display text for an enumerated value using `value.DisplayEquivalent()`.
This will return the display text if it has been set, or the name of the enum value otherwise.
Set the display text by dropping a `[DisplayEquivalent("text")]` attribute on each field of the enum.

```csharp
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
```
