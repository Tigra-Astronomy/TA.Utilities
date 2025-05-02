// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: Maybe.cs  Last modified: 2020-07-13@02:11 by Tim Long

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;

namespace TA.Utils.Core
    {
    /// <summary>
    ///     Represents an object that may or may not have a value (strictly, a collection of zero or one
    ///     elements). Use LINQ expression <c>maybe.Any()</c> to determine if there is a value. Use LINQ
    ///     expression <c>maybe.Single()</c> to retrieve the value.
    /// </summary>
    /// <typeparam name="T">The type of the item in the collection.</typeparam>
    /// <remarks>
    ///     This type almost completely eliminates any need to return <c>null</c> or deal with possibly
    ///     null references, which makes code cleaner and more clearly expresses the intent of 'no value'
    ///     versus 'error'.  The value of a Maybe cannot be <c>null</c>, because <c>null</c> really means
    ///     'no value' and that is better expressed by using <see cref="P:TA.Utils.Core.Maybe{T}.Empty" />.
    /// </remarks>
    [ImmutableObject(true)]
    public sealed class Maybe<T> : IEnumerable<T>
        {
        private static readonly Maybe<T> EmptyInstance = new Maybe<T>();

        private readonly IEnumerable<T> values;

        /// <summary>Initializes a new instance of the <see cref="Maybe{T}" /> with no value.</summary>
        private Maybe()
            {
            values = Array.Empty<T>();
            }

        /// <summary>Initializes a new instance of the <see cref="Maybe{T}" /> with a value.</summary>
        /// <param name="value">The value.</param>
        private Maybe([NotNull] T value)
            {
            Debug.Assert(value != null);
            values = new[] { value };
            }

        /// <summary>Gets an instance that does not contain a value.</summary>
        /// <value>The empty instance.</value>
        public static Maybe<T> Empty
            {
            get
                {
                Contract.Ensures(Contract.Result<Maybe<T>>() != null);
                return EmptyInstance;
                }
            }

        /// <summary>Gets a value indicating whether this <see cref="Maybe{T}" /> is empty (has no value).</summary>
        /// <value><c>true</c> if none; otherwise, <c>false</c>.</value>
        [Obsolete("Use IsEmpty instead.")]
        public bool None => ReferenceEquals(this, Empty) || !values.Any();

        /// <summary>Gets a value indicating whether this <see cref="Maybe{T}" /> is empty (has no value).</summary>
        /// <value><c>true</c> if there is no value; otherwise, <c>false</c>.</value>
        public bool IsEmpty => ReferenceEquals(this, Empty) || !values.Any();

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through
        ///     the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
            {
            Contract.Ensures(Contract.Result<IEnumerator<T>>() != null);
            return values.GetEnumerator();
            }

        IEnumerator IEnumerable.GetEnumerator()
            {
            Contract.Ensures(Contract.Result<IEnumerator>() != null);
            return GetEnumerator();
            }

            /// <summary>Creates a new <see cref="Maybe{T}" /> from an instance of <typeparamref name="T" />.</summary>
            /// <param name="source">The source instance to wrap in a Maybe.</param>
            /// <returns>A new <see cref="Maybe{T}" /> containing the source item.</returns>
            public static Maybe<T> From(T? source)
            {
            if (source == null)
                return Empty;
            return new Maybe<T>(source);
            }

        [ContractInvariantMethod]
        private void ObjectInvariant()
            {
            Contract.Invariant(values != null);
            }

        /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        [System.Diagnostics.Contracts.Pure]
        public override string ToString() =>
            //Contract.Ensures(Contract.Result<string>() != null);
            this.SingleOrDefault()?.ToString() ?? "{no value}";
        }

    /// <summary>Helper methods for working with <see cref="Maybe{T}" /></summary>
    public static class MaybeExtensions
        {
        /// <summary>Expresses an instance of <typeparamref name="T" /> into a <see cref="Maybe{T}" />.</summary>
        /// <typeparam name="T">The reference type being wrapped (usually inferred from usage).</typeparam>
        /// <param name="source">The source object.</param>
        public static Maybe<T> AsMaybe<T>(this T source) where T : class => Maybe<T>.From(source);

        /// <summary>Returns <c>true</c> if the <see cref="Maybe{T}" /> has no value.</summary>
        public static bool None<T>(this Maybe<T> maybe) => !maybe.Any();
        }
    }