// This file is part of the TA.Utils project
// Copyright © 2016-2023 Timtek Systems Limited, all rights reserved.
// File: StringExtensionSpecs.cs  Last modified: 2023-08-14@03:15 by Tim Long

using System;
using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications;

[Subject(typeof(StringExtensions), "Cleaning")]
public class when_cleaning_a_string
    {
    private It should_remove_unwanted_characters = () => "Hello, World!".Clean(",!").ShouldEqual("Hello World");
    }

[Subject(typeof(StringExtensions))]
public class when_cleaning_an_empty_string
    {
    private It should_return_empty_string = () => string.Empty.Clean(",!").ShouldEqual(string.Empty);
    }

[Subject(typeof(StringExtensions))]
public class when_cleaning_a_string_with_empty_clean_string
    {
    private It should_return_original_string = () => "Hello, World!".Clean(string.Empty).ShouldEqual("Hello, World!");
    }

[Subject(typeof(StringExtensions))]
public class when_keeping_characters_in_a_string
    {
    private It should_keep_only_specified_characters = () => "Hello, World!".Keep("loWrd").ShouldEqual("lloWorld");
    }

[Subject(typeof(StringExtensions))]
public class when_keeping_characters_in_an_empty_string
    {
    private It should_return_empty_string = () => string.Empty.Keep("loWrd").ShouldEqual(string.Empty);
    }

[Subject(typeof(StringExtensions))]
public class when_keeping_characters_with_no_matching_characters
    {
    private It should_return_empty_string = () => "Hello, World!".Keep("xyz").ShouldEqual(string.Empty);
    }

[Subject(typeof(StringExtensions))]
public class when_getting_tail_of_a_string
    {
    private It should_return_last_n_characters = () => "Hello, World!".Tail(6).ShouldEqual("World!");
    }

[Subject(typeof(StringExtensions))]
public class when_getting_tail_with_zero_length
    {
    private It should_return_empty_string = () => "Hello, World!".Tail(0).ShouldEqual(string.Empty);
    }

[Subject(typeof(StringExtensions))]
public class when_getting_tail_with_length_greater_than_string_length
    {
    private It should_throw_argument_out_of_range_exception = () =>
        Catch.Exception(() => "Hello, World!".Tail(14)).ShouldBeOfExactType<ArgumentOutOfRangeException>();
    }

[Subject(typeof(StringExtensions))]
public class when_getting_head_of_a_string
    {
    private It should_return_first_n_characters = () => "Hello, World!".Head(5).ShouldEqual("Hello");
    }

[Subject(typeof(StringExtensions))]
public class when_getting_head_with_zero_length
    {
    private It should_return_empty_string = () => "Hello, World!".Head(0).ShouldEqual(string.Empty);
    }

[Subject(typeof(StringExtensions))]
public class when_getting_head_with_length_greater_than_string_length
    {
    private It should_throw_argument_out_of_range_exception = () =>
        Catch.Exception(() => "Hello, World!".Head(14)).ShouldBeOfExactType<ArgumentOutOfRangeException>();
    }

[Subject(typeof(StringExtensions))]
public class when_removing_head_of_a_string
    {
    private It should_remove_first_n_characters = () => "Hello, World!".RemoveHead(6).ShouldEqual(" World!");
    }

[Subject(typeof(StringExtensions))]
public class when_removing_head_with_zero_length
    {
    private It should_return_original_string = () => "Hello, World!".RemoveHead(0).ShouldEqual("Hello, World!");
    }

[Subject(typeof(StringExtensions))]
public class when_removing_head_with_length_greater_than_string_length
    {
    private It should_return_empty_string = () => "Hello, World!".RemoveHead(13).ShouldEqual(string.Empty);
    }

[Subject(typeof(StringExtensions))]
public class when_removing_tail_of_a_string
    {
    private It should_remove_last_n_characters = () => "Hello, World!".RemoveTail(6).ShouldEqual("Hello, ");
    }

[Subject(typeof(StringExtensions))]
public class when_removing_tail_with_zero_length
    {
    private It should_return_original_string = () => "Hello, World!".RemoveTail(0).ShouldEqual("Hello, World!");
    }

[Subject(typeof(StringExtensions))]
public class when_removing_tail_with_length_greater_than_string_length
    {
    private It should_return_empty_string = () => "Hello, World!".RemoveTail(13).ShouldEqual(string.Empty);
    }

[Subject(typeof(StringExtensions))]
public class when_converting_string_to_hex
    {
    private It should_return_hex_representation = () => "Hello".ToHex().ShouldEqual("{48, 65, 6c, 6c, 6f}");
    }

[Subject(typeof(StringExtensions))]
public class when_converting_empty_string_to_hex
    {
    private It should_return_empty_braces = () => "".ToHex().ShouldEqual("{}");
    }

[Subject(typeof(StringExtensions))]
public class when_converting_null_string_to_hex
    {
    private It should_throw_null_reference_exception =
        () => Catch.Exception(() => ((string)null).ToHex()).ShouldBeOfExactType<NullReferenceException>();
    }