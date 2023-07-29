// This file is part of the TA.Utils project
// Copyright © 2016-2023 Tigra Astronomy, all rights reserved.
// File: StringExtensionSpecs.cs  Last modified: 2023-07-29@13:26 by Tim Long

using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications;

[Subject(typeof(StringExtensions), "Head")]
internal class when_taking_the_head_of_a_string
    {
     It should_extract_the_head = () => inputString.Head(19).ShouldEqual("The quick brown fox");
     It should_extract_the_dog_tail = () => inputString.Tail(8).ShouldEqual("lazy dog");
     It should_keep_the_vowels = () => inputString.Keep("aeiou").ShouldEqual("euioouoeeao");
     It should_remove_the_vowels = () => inputString.Clean("aeiou").ShouldEqual("Th qck brwn fx jmps vr th lzy dg");
     It should_remove_the_fox_head = () => inputString.RemoveHead(20).ShouldEqual("jumps over the lazy dog");
     It should_remove_the_lazy_dog = () => inputString.RemoveTail(9).ShouldEqual("The quick brown fox jumps over the");
     It should_convert_ascii_to_hex = () => "123".ToHex().ShouldEqual("{31, 32, 33}");
    //                                            1         2         3         4
    //                                  0123456789012345678901234567890123456789012
    private const string inputString = "The quick brown fox jumps over the lazy dog";
    }