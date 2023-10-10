// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: Octet.cs  Last modified: 2020-07-13@02:11 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace TA.Utils.Core
    {
    /// <summary>
    ///     An immutable representation of an 8 bit byte, with each bit individually addressable. In most
    ///     cases an Octet is interchangeable with a <see cref="byte" /> (implicit conversion operators are
    ///     provided). An Octet can be explicitly converted (cast) to or from an integer. Take care when
    ///     converting to and from negative integers.
    /// </summary>
    public sealed class Octet : IEquatable<Octet>
        {
        private readonly bool[] bits;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Octet" /> struct from an array of at least 8
        ///     booleans.
        /// </summary>
        /// <param name="bits">The bits; there must be exactly 8.</param>
        private Octet(bool[] bits)
            {
            Contract.Requires(bits != null);
            Contract.Requires(bits.Length == 8);
            this.bits = bits;
            }

        /// <summary>
        ///     Prevents a default instance of the <see cref="Octet" /> class from being created. Use one of
        ///     the static factory methods or conversion operators instead.
        /// </summary>
        /// <seealso cref="Max" />
        /// <seealso cref="Zero" />
        /// <seealso cref="FromInt" />
        /// <seealso cref="FromUnsignedInt" />
        /// <seealso cref="op_Implicit(byte)" />
        /// <seealso cref="op_Explicit(int)" />
        /// <seealso cref="op_Explicit(uint)" />
        private Octet() { }

        /// <summary>Gets an Octet with all the bits set to zero.</summary>
        public static Octet Zero { get; } = FromInt(0);

        /// <summary>Gets an Octet set to the maximum value (i.e. all the bits set to one).</summary>
        public static Octet Max { get; } = FromInt(0xFF);

        /// <summary>Gets the <see cref="T:System.Boolean" /> value of the the specified bit.</summary>
        /// <param name="bit">
        ///     The bit position, where 0 is the least significant bit and 7 is the most
        ///     significant bit.
        /// </param>
        /// <returns><c>true</c> if the bit at the specified position is 1, <c>false</c> otherwise.</returns>
        public bool this[int bit]
            {
            get
                {
                Contract.Requires(bit >= 0 && bit < 8);
                return bits[bit];
                }
            }

        /// <summary>Indicates whether this octet is equal to another octet, using value semantics.</summary>
        /// <returns>
        ///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise,
        ///     false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Octet other)
            {
            if (other is null) return false; // equality to a null object is false by definition
            for (var i = 0; i < bits.Length; i++)
                if (bits[i] != other[i])
                    return false;
            return true;
            }

        [ContractInvariantMethod]
        private void ObjectInvariant()
            {
            Contract.Invariant(bits != null);
            Contract.Invariant(bits.Length == 8, "Consider using Octet.FromInt() instead of new Octet()");
            }

        /// <summary>
        ///     Factory method: create an Octet from an integer. The integer two's complement binary value is
        ///     simply truncated and the resultant octet will be the least significant byte. Take care with
        ///     negative values as this may not produce the expected result. For example,
        ///     <c>Octet.FromInt(int.Min)</c> is <c>Octet.Zero</c> whereas <c>Octet.FromInt(int.Max)</c> is
        ///     <c>Octet.Max</c>
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Octet.</returns>
        public static Octet FromInt(int source)
            {
            var bits = new bool[8];
            for (var i = 0; i < 8; i++)
                {
                var bit = source & 0x01;
                bits[i] = bit != 0;
                source >>= 1;
                }

            return new Octet(bits);
            }

        /// <summary>Factory method: create an Octet from an unsigned integer.</summary>
        /// <param name="source">The source.</param>
        /// <returns>Octet.</returns>
        [CLSCompliant(false)]
        public static Octet FromUnsignedInt(uint source)
            {
            return FromInt((int)source);
            }

        /// <summary>
        ///     Returns a new octet with the specified bit number set to the specified value. Other bits are
        ///     duplicated.
        /// </summary>
        /// <param name="bit">The bit number to be modified.</param>
        /// <param name="value">The value of the specified bit number.</param>
        /// <returns>A new octet instance with the specified bit number set to the specified value.</returns>
        [CLSCompliant(false)]
        [Obsolete("Use WithBitSet() or WithBitClear() instead")]
        public Octet WithBitSetTo(ushort bit, bool value)
            {
            Contract.Requires(bit < 8);
            var newBits = new bool[8];
            bits.CopyTo(newBits, 0);
            newBits[bit] = value;
            return new Octet(newBits);
            }

        /// <summary>
        ///     Returns a new Octet instance with the specified bit set to 1. Other bits are duplicated from the current instance.
        /// </summary>
        /// <param name="bitNumber">
        ///     The bit position to be set, where 0 is the least significant bit and 7 is the most significant
        ///     bit.
        /// </param>
        /// <returns>A new Octet instance with the specified bit set to 1.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when bitNumber is less than 0 or greater than 7.</exception>
        public Octet WithBitSet(int bitNumber)
            {
            Contract.Requires(bitNumber >= 0 && bitNumber < 8);
            if (bitNumber < 0 || bitNumber > 7)
                throw new ArgumentOutOfRangeException(nameof(bitNumber), bitNumber,
                    $"{bitNumber} is not a valid bit number. Must be [0..7]");
            var newBits = new bool[8];
            bits.CopyTo(newBits, 0);
            newBits[bitNumber] = true;
            return new Octet(newBits);
            }

        /// <summary>
        ///     Returns a new Octet instance with the specified bit number cleared (set to false). Other bits are unchanged.
        /// </summary>
        /// <param name="bitNumber">The bit number to be cleared. Must be between 0 and 7 inclusive.</param>
        /// <returns>A new Octet instance with the specified bit number cleared.</returns>
        public Octet WithBitClear(int bitNumber)
            {
            Contract.Requires(bitNumber >= 0 && bitNumber < 8);
            if (bitNumber < 0 || bitNumber > 7)
                throw new ArgumentOutOfRangeException(nameof(bitNumber), bitNumber,
                    $"{bitNumber} is not a valid bit number. Must be [0..7]");
            var newBits = new bool[8];
            bits.CopyTo(newBits, 0);
            newBits[bitNumber] = false;
            return new Octet(newBits);
            }

        /// <summary>
        ///     Returns a new octet with the specified bit number set to the specified value. Other bits are
        ///     duplicated.
        /// </summary>
        /// <param name="bitNumber">The bit number to be modified.</param>
        /// <param name="value">The value of the specified bit number.</param>
        /// <returns>A new octet instance with the specified bit number set to the specified value.</returns>
        public Octet WithBitSetTo(int bitNumber, bool value)
            {
            return value ? WithBitSet(bitNumber) : WithBitClear(bitNumber);
            }

        /// <summary>Returns a <see cref="T:System.String" /> that represents the octet instance.</summary>
        /// <returns>A <see cref="T:System.String" /> that represents this instance.</returns>
        public override string ToString()
            {
            var builder = new StringBuilder();
            for (var i = 7; i >= 0; i--)
                {
                    builder.Append(bits[i] ? '1' : '0');
                    builder.Append(' ');
                }

            builder.Length -= 1;
            return builder.ToString();
            }

        /// <summary>
        ///     Performs an explicit conversion from <see cref="uint" /> to <see cref="Octet" />. This
        ///     conversion is explicit because there is potential loss of information.
        /// </summary>
        /// <param name="integer">The integer.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        public static explicit operator Octet(uint integer)
            {
            return FromUnsignedInt(integer);
            }

        /// <summary>
        ///     Performs an explicit conversion from <see cref="int" /> to <see cref="Octet" />. This
        ///     conversion is explicit because there is potential loss of information.
        /// </summary>
        /// <param name="integer">The integer.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Octet(int integer)
            {
            return FromInt(integer);
            }

        /// <summary>Performs an implicit conversion from <see cref="Octet" /> to <see cref="byte" />.</summary>
        /// <param name="octet">The octet.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator byte(Octet octet)
            {
            var sum = 0;
            for (var i = 0; i < 8; i++)
                if (octet[i])
                    sum += 1 << i;
            return (byte)sum;
            }

        /// <summary>Performs an implicit conversion from <see cref="byte" /> to <see cref="Octet" />.</summary>
        /// <param name="b">The input byte.</param>
        /// <returns>The result of the conversion in a new Octet.</returns>
        public static implicit operator Octet(byte b)
            {
            return FromUnsignedInt(b);
            }

        /// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to this instance.</summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance;
        ///     otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object other)
            {
            if (ReferenceEquals(null, other)) return false;
            return other is Octet && Equals((Octet)other);
            }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like
        ///     a hash table.
        /// </returns>
        public override int GetHashCode()
            {
            return bits.GetHashCode();
            }

        /// <summary>Tests two octets for equality.</summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns><c>true</c> if the octets are equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(Octet left, Octet right)
            {
            if (left is null) return false;
            if (right is null) return false;
            return left.Equals(right);
            }

        /// <summary>Tests two octets for inequality.</summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns><c>true</c> if the octets are not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(Octet left, Octet right)
            {
            if (left is null) return true; // Cannot be equal if either instance is null
            if (right is null) return true;
            return !left.Equals(right);
            }

        /// <summary>Performs a bitwise logical AND on two octets and produces a third containing the result.</summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>A new octet containing the result of the bitwise logical AND operation.</returns>
        public static Octet operator &(Octet left, Octet right)
            {
            var result = (bool[])left.bits.Clone();
            for (var i = 0; i < 8; i++) result[i] &= right[i];
            return new Octet(result);
            }

        /// <summary>Performs a bitwise logical OR on two octets and produces a third containing the result.</summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>A new octet containing the result of the bitwise logical OR operation.</returns>
        public static Octet operator |(Octet left, Octet right)
            {
            var result = (bool[])left.bits.Clone();
            for (var i = 0; i < 8; i++) result[i] |= right[i];
            return new Octet(result);
            }
        }
    }