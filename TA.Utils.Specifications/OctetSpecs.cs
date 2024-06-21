// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: OctetSpecs.cs  Last modified: 2020-07-13@02:11 by Tim Long

using System;
using System.Linq;
using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications;

[Subject(typeof(Octet), "construction")]
class when_constructing_an_octet
{
    It should_be_zero = () => Zero.ShouldEqual((byte) Octet.Zero);
    It should_implicitly_convert_to_a_byte = () => (Zero == Octet.Zero).ShouldBeTrue();
    It should_implicitly_convert_to_and_from_integers = () => ((int) (Octet) 0xaa).ShouldEqual(0xAA);
    const byte Zero = 0;
}

[Subject(typeof(Octet), "Bit Manipulation")]
public class when_manipulating_bits
{
    It should_set_bit_0 = () => Octet.Zero.WithBitSet(0).ShouldEqual((Octet) 0b00000001);
    It should_set_bit_1 = () => Octet.Zero.WithBitSet(1).ShouldEqual((Octet) 0b00000010);
    It should_set_bit_2 = () => Octet.Zero.WithBitSet(2).ShouldEqual((Octet) 0b00000100);
    It should_set_bit_3 = () => Octet.Zero.WithBitSet(3).ShouldEqual((Octet) 0b00001000);
    It should_set_bit_4 = () => Octet.Zero.WithBitSet(4).ShouldEqual((Octet) 0b00010000);
    It should_set_bit_5 = () => Octet.Zero.WithBitSet(5).ShouldEqual((Octet) 0b00100000);
    It should_set_bit_6 = () => Octet.Zero.WithBitSet(6).ShouldEqual((Octet) 0b01000000);
    It should_set_bit_7 = () => Octet.Zero.WithBitSet(7).ShouldEqual((Octet) 0b10000000);
    It should_clear_bit_0 = () => Octet.Max.WithBitClear(0).ShouldEqual((Octet) 0b11111110);
    It should_clear_bit_1 = () => Octet.Max.WithBitClear(1).ShouldEqual((Octet) 0b11111101);
    It should_clear_bit_2 = () => Octet.Max.WithBitClear(2).ShouldEqual((Octet) 0b11111011);
    It should_clear_bit_3 = () => Octet.Max.WithBitClear(3).ShouldEqual((Octet) 0b11110111);
    It should_clear_bit_4 = () => Octet.Max.WithBitClear(4).ShouldEqual((Octet) 0b11101111);
    It should_clear_bit_5 = () => Octet.Max.WithBitClear(5).ShouldEqual((Octet) 0b11011111);
    It should_clear_bit_6 = () => Octet.Max.WithBitClear(6).ShouldEqual((Octet) 0b10111111);
    It should_clear_bit_7 = () => Octet.Max.WithBitClear(7).ShouldEqual((Octet) 0b01111111);
}

[Subject(typeof(Octet), "Bit setting")]
public class when_setting_a_bit_in_an_octet
{
    Establish context = () => octet = Octet.Zero;
    Because of = () => result = Octet.Zero.WithBitSet(3);
    It should_return_a_new_octet = () => result.ShouldNotEqual(octet);
    It should_set_the_specified_bit_to_one = () => result[bitNumber].ShouldBeTrue();

    It should_not_change_other_bits = () =>
    {
        for (var i = 0; i < 8; i++)
            if (i != bitNumber)
                result[i].ShouldEqual(octet[i]);
    };

    static Octet octet;
    const int bitNumber = 3;
    static Octet result;
}

[Subject(typeof(Octet), "Bit setting, invalid bit number")]
public class when_setting_a_bit_outside_the_valid_range
{
    Because of = () => exception = Catch.Exception(() => Octet.Zero.WithBitSet(8));
    It should_throw_an_argument_out_of_range_exception = () => exception.ShouldBeOfExactType<ArgumentOutOfRangeException>();
    static Exception exception;
}

[Subject(typeof(Octet), "Bit clearing")]
public class when_clearing_a_bit_in_an_octet
{
    Establish context = () => octet = Octet.Max;
    Because of = () => result = octet.WithBitClear(3);
    It should_return_a_new_octet = () => result.ShouldNotEqual(octet);
    It should_clear_the_specified_bit_to_zero = () => result[bitNumber].ShouldBeFalse();
    It should_not_change_other_bits = () =>
        Enumerable.Range(0, 8)
            .Where(i => i != bitNumber)
            .All(i => result[i] == octet[i])
            .ShouldBeTrue();
    static Octet octet;
    const int bitNumber = 3;
    static Octet result;
}

[Subject(typeof(Octet), "Bit setting, invalid bit number")]
public class when_clearing_a_bit_outside_the_valid_range
{
    Because of = () => exception = Catch.Exception(() => Octet.Zero.WithBitClear(8));
    It should_throw_an_argument_out_of_range_exception = () => exception.ShouldBeOfExactType<ArgumentOutOfRangeException>();
    static Exception exception;
}

[Subject(typeof(Octet), "Immutability")]
public class when_mutating_an_octet
{
    // ReSharper disable once EqualExpressionComparison
    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
    It should_intern_well_known_instances = () => ReferenceEquals(Octet.Zero, Octet.Zero).ShouldBeTrue();
    It should_be_immutable = () => ReferenceEquals(immutable.WithBitSetTo(3, false), immutable).ShouldBeFalse();
    static Octet immutable = Octet.Max;
}

[Subject(typeof(Octet), "Conversion")]
public class when_converting_from_an_integer
{
    It should_truncate_negative_int = () => ((Octet) int.MinValue).ShouldEqual(Octet.Zero);
    It should_truncate_positive_int = () => ((Octet) int.MaxValue).ShouldEqual(Octet.Max);
    It should_truncate_unsigned_int = () => ((Octet) uint.MaxValue).ShouldEqual(Octet.Max);
}

[Subject(typeof(Octet), "Conversion")]
public class when_converting_to_an_integer
{
    It should_produce_a_positive_signed_int = () => ((int) Octet.Max).ShouldEqual(0b11111111);
    It should_produce_a_small_unsigned_int = () => ((uint) Octet.Max).ShouldEqual(0b11111111u);
}

[Subject(typeof(Octet), "Serialization")]
public class when_converting_to_a_string
{
    It should_produce_exactly = () => Octet.FromUnsignedInt(0x4a).ToString().ShouldEqual("01001010b 0x4A 74");
}