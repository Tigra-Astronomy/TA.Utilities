// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: OctetSpecs.cs  Last modified: 2020-07-13@02:11 by Tim Long

using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications
    {
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
        It should_set_bit_0 = () => Octet.Zero.WithBitSetTo(0, true).ShouldEqual((Octet) 0b00000001);
        It should_set_bit_1 = () => Octet.Zero.WithBitSetTo(1, true).ShouldEqual((Octet) 0b00000010);
        It should_set_bit_2 = () => Octet.Zero.WithBitSetTo(2, true).ShouldEqual((Octet) 0b00000100);
        It should_set_bit_3 = () => Octet.Zero.WithBitSetTo(3, true).ShouldEqual((Octet) 0b00001000);
        It should_set_bit_4 = () => Octet.Zero.WithBitSetTo(4, true).ShouldEqual((Octet) 0b00010000);
        It should_set_bit_5 = () => Octet.Zero.WithBitSetTo(5, true).ShouldEqual((Octet) 0b00100000);
        It should_set_bit_6 = () => Octet.Zero.WithBitSetTo(6, true).ShouldEqual((Octet) 0b01000000);
        It should_set_bit_7 = () => Octet.Zero.WithBitSetTo(7, true).ShouldEqual((Octet) 0b10000000);
        It should_clear_bit_0 = () => Octet.Max.WithBitSetTo(0, false).ShouldEqual((Octet) 0b11111110);
        It should_clear_bit_1 = () => Octet.Max.WithBitSetTo(1, false).ShouldEqual((Octet) 0b11111101);
        It should_clear_bit_2 = () => Octet.Max.WithBitSetTo(2, false).ShouldEqual((Octet) 0b11111011);
        It should_clear_bit_3 = () => Octet.Max.WithBitSetTo(3, false).ShouldEqual((Octet) 0b11110111);
        It should_clear_bit_4 = () => Octet.Max.WithBitSetTo(4, false).ShouldEqual((Octet) 0b11101111);
        It should_clear_bit_5 = () => Octet.Max.WithBitSetTo(5, false).ShouldEqual((Octet) 0b11011111);
        It should_clear_bit_6 = () => Octet.Max.WithBitSetTo(6, false).ShouldEqual((Octet) 0b10111111);
        It should_clear_bit_7 = () => Octet.Max.WithBitSetTo(7, false).ShouldEqual((Octet) 0b01111111);
        }

    [Subject(typeof(Octet), "Immutability")]
    public class when_mutating_an_octet
        {
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
    }