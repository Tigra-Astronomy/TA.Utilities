// This file is part of the TA.Utils project
// Copyright © 2015-2025 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: CodeContracts.cs  Last modified: 2025-05-02@15:05 by Tim

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TA.Utils.Core;

/// <summary>
///     Methods that provide a way to declare "code contracts" in code.
///     Contracts make assertions about runtime state, e.g. parameter values, during debugging.
///     They can be used to validate entry and exit conditions of methods,
///     The methods are only effective in Debug builds so code should not rely on the outcome of these methods.
///     If any contract assertion fails, it will throw a <see cref="CodeContractViolationException" />.
/// </summary>
public static class CodeContracts
{
    /// <summary>
    ///     Asserts that a specified condition is met for a given value. If the condition is not met,
    ///     a <see cref="CodeContractViolationException" /> is thrown with the provided message.
    /// </summary>
    /// <typeparam name="T">The type of the value being tested.</typeparam>
    /// <param name="value">The value to be tested against the condition.</param>
    /// <param name="condition">The predicate that defines the condition to be tested.</param>
    /// <param name="message">The message to include in the exception if the condition is not met.</param>
    /// <param name="source">
    ///     The name of the caller member where the assertion is made. This parameter is optional and
    ///     will be automatically populated by the compiler if not provided.
    /// </param>
    /// <exception cref="CodeContractViolationException">
    ///     Thrown when the specified condition is not met for the given value.
    /// </exception>
    public static void ContractAssert<T>(this T value, Predicate<T> condition, string message,
        [CallerMemberName] string? source = null)
    {
        if (!condition.Invoke(value))
            ThrowException(value?.ToString() ?? "null", condition, message, source ?? string.Empty);
    }

    /// <summary>
    ///     Asserts that a reference type is not null.
    /// </summary>
    /// <param name="value">A reference.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    public static void ContractAssertNotNull<T>(this T value) where T : class =>
        value.ContractAssert(p => p is not null, "Reference type cannot be null.");

    /// <summary>
    ///     Asserts that a nullable value type has a value.
    /// </summary>
    /// <param name="value">A nullable struct.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    public static void ContractAssertNotNull<T>(this T? value) where T : struct =>
        value.ContractAssert(p => !p.HasValue, "nullable value type must have a value.");

    /// <summary>
    ///     Asserts that a collection is not empty (A null reference is empty by definition).
    /// </summary>
    /// <param name="collection">The collection being tested.</param>
    public static void ContractAssertNotEmpty(this ICollection collection)
    {
        collection.ContractAssertNotNull();
        collection.ContractAssert(p => p.Count > 0, "Collection must not be empty");
    }

    /// <summary>
    ///     Asserts that a collection is not empty (A null reference is empty by definition).
    /// </summary>
    /// <param name="collection">The collection being tested.</param>
    public static void ContractAssertNotEmpty<T>(this IEnumerable<T> collection)
    {
        collection.ContractAssert(p => p is not null && p.Any(), "Enumerable must not be empty");
    }

    /// <summary>
    ///     Asserts that a source enumerable sequence contains the specified item at least once. Note: potentially expensive
    ///     operation O(n)
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="item">The required item.</param>
    /// <typeparam name="TItem">The type of items in the collection.</typeparam>
    public static void ContractAssertContains<TItem>(this IEnumerable<TItem> source, TItem item) =>
        source.ContractAssert(p => p.Contains(item), "The source collection must contain the specified item");

    /// <summary>
    ///     Asserts that a source enumerable sequence does not contain the specified item. Note: potentially expensive
    ///     operation O(n).
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="item">The disallowed item.</param>
    /// <typeparam name="TItem">The type of items in the collection.</typeparam>
    public static void ContractAssertDoesNotContain<TItem>(this IEnumerable<TItem> source, TItem item) =>
        source.ContractAssert(p => !p.Contains(item), "The source collection must not contain the specified item");

    private static void ThrowException<T>(string? value, Predicate<T> condition, string message, string source)
    {
        var ex = new CodeContractViolationException(message);
        ex.Data["Condition"] = condition.ToString();
        ex.Data["Value"] = value;
        ex.Data["Caller"] = source;
        throw ex;
    }
}

/// <summary>
///     Represents an exception that is thrown when a code contract is violated. The occurrence of this exception is an
///     unambiguous indication of a bug. The data dictionary will contain additional details on which contract was violated
///     and the values that did not meet the preconditions.
/// </summary>
public sealed class CodeContractViolationException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CodeContractViolationException" /> class
    ///     with a default error message indicating a code contract violation.
    /// </summary>
    public CodeContractViolationException() : base("A code contract violation occurred.") { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CodeContractViolationException" /> class with a specified error
    ///     message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CodeContractViolationException(string message) : base(message) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CodeContractViolationException" /> class with a specified error
    ///     message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">
    ///     The exception that is the cause of the current exception, or a <c>null</c> reference if no inner exception is
    ///     specified.
    /// </param>
    public CodeContractViolationException(string message, Exception inner) : base(message, inner) { }
}