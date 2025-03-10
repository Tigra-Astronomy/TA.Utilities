// This file is part of the TA.Utils project
// Copyright © 2015-2024 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: PropertyBinderSpecs.cs  Last modified: 2024-7-17@16:41 by tim.long

using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Machine.Specifications;
using TA.Utils.Core.PropertyBinding;

namespace TA.Utils.Specifications;

[Subject(typeof(PropertyBinder), "Reading Key-Value pairs")]
class when_reading_key_value_pairs_from_a_stream
{
    const string inputFile = @"
# Comments = are ignored.
Name = Tim
ChineseHoroscope # Dragon
    Whitespace, Is ignored # so are comments
";

    static readonly List<KeyValueDataRecord> ExpectedKeyValuePairs = new()
    {
        new KeyValueDataRecord("Name", "Tim"),
        new KeyValueDataRecord("ChineseHoroscope", "Dragon"),
        new KeyValueDataRecord("Whitespace", "Is ignored")
    };

    Establish context = () =>
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(inputFile);
        writer.Flush();
        stream.Position = 0;
        reader = new KeyValueReader(stream);
    };

    Because of = () => keyValuePairs = reader.KeyValueDataRecords().ToList();

    It should_contain_the_expected_key_value_pairs = () => keyValuePairs.SequenceEqual(ExpectedKeyValuePairs);
    static KeyValueReader reader;
    static List<KeyValueDataRecord> keyValuePairs;
}

[Subject(typeof(PropertyBinder), "Simple Case")]
class when_binding_simple_properties_by_name
{
    Establish context = () => binder = new PropertyBinder();
    Because of = () => result = binder.BindProperties<TargetClass>(InputData);
    It should_bind_the_integer_property = () => result.IntegerProperty.ShouldEqual(ExpectedIntegerValue);
    It should_bind_the_string_property = () => result.StringProperty.ShouldEqual(ExpectedStringValue);

    static readonly List<KeyValueDataRecord> InputData = new()
    {
        new KeyValueDataRecord("IntegerProperty", ExpectedIntegerValue.ToString()),
        new KeyValueDataRecord("StringProperty", ExpectedStringValue)
    };

    static PropertyBinder binder;
    static TargetClass result;
    const int ExpectedIntegerValue = 10;
    const string ExpectedStringValue = "The cat sat on the mat.";

    class TargetClass
    {
        public int IntegerProperty { get; [UsedImplicitly] set; }
        public string StringProperty { get; [UsedImplicitly] set; }
    }
}

[Subject(typeof(PropertyBinder), "Names from Attributes")]
class when_binding_simple_properties_by_attributes
{
    Establish context = () => binder = new PropertyBinder();
    Because of = () => result = binder.BindProperties<TargetClass>(InputData);
    It should_bind_the_integer_property = () => result.Count.ShouldEqual(ExpectedIntegerValue);
    It should_bind_the_string_property = () => result.Sentence.ShouldEqual(ExpectedStringValue);

    static readonly List<KeyValueDataRecord> InputData = new()
    {
        new KeyValueDataRecord("IntegerValue", ExpectedIntegerValue.ToString()),
        new KeyValueDataRecord("StringValue", ExpectedStringValue)
    };

    static PropertyBinder binder;
    static TargetClass result;
    const int ExpectedIntegerValue = 10;
    const string ExpectedStringValue = "The cat sat on the mat.";

    class TargetClass
    {
        [DataKey("IntegerValue")] public int Count { get; [UsedImplicitly] set; }
        [DataKey("StringValue")] public string Sentence { get; [UsedImplicitly] set; }
    }
}

[Subject(typeof(PropertyBinder), "Binding Collections")]
class when_binding_to_a_collection
{
    Establish context = () => binder = new PropertyBinder();
    Because of = () => result = binder.BindProperties<TargetClass>(InputData);
    It should_collect_the_dog = () => result.CollectionOfThings.ShouldContain("Dog");
    It should_not_collect_the_rose = () => result.CollectionOfThings.ShouldNotContain("Rose");

    static readonly List<KeyValueDataRecord> InputData = new()
    {
        new KeyValueDataRecord("Plant", "Rose"),
        new KeyValueDataRecord("Animal", "Dog"),
    };

    static PropertyBinder binder;
    static TargetClass result;
    const int ExpectedIntegerValue = 10;
    const string ExpectedStringValue = "The cat sat on the mat.";

    class TargetClass
    {
        [DataKey("Animal")] public List<string> CollectionOfThings { get; [UsedImplicitly] set; } = new();
    }
}