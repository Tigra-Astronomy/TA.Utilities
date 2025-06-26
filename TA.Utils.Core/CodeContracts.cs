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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace TA.Utils.Core;

/// <summary>
///     Methods that provide a way to declare "code contracts" in code. These are really just convenient wrappers around
///     assertions.
///     Contracts make assertions about runtime state, e.g. parameter values, method entry and exit requirements, etc.
///     If any contract assertion fails, it will throw a <see cref="CodeContractViolationException" />.
///     It is recommended that this exception is never caught or handled in any way, except possibly to log it, as it an
///     unambiguous indication of a bug in the code and execution should not be allowed to continue after a contract
///     violation.
/// </summary>
public static class CodeContracts
{
    /// <summary>
    ///     Asserts that a specified condition is met for a given value. If the condition is not met,
    ///     a <see cref="CodeContractViolationException" /> is thrown with the provided message.
    /// </summary>
    /// <typeparam name="T">The type of the value being tested.</typeparam>
    /// <param name="value">The value to be tested against the condition.</param>
    /// <param name="condition">The predicate expression that defines the condition to be tested.</param>
    /// <param name="message">The message to include in the exception if the condition is not met.</param>
    /// <param name="caller">
    ///     The name of the caller member where the assertion is made. This parameter is optional and
    ///     will be automatically populated by the compiler if not provided.
    /// </param>
    /// <exception cref="CodeContractViolationException">
    ///     Thrown when the specified condition is not met for the given value.
    /// </exception>
    public static void ContractAssert<T>(this T       value, Expression<Func<T, bool>> condition, string message,
        [CallerMemberName]                    string? caller = null)
    {
        // condition() may throw an exception, so we need to catch it and rethrow as a CodeContractViolationException
        try
        {
            var predicate = condition.Compile(); // Compile the expression tree to a delegate for evaluation
            if (!predicate(value))
                ThrowContractException(value?.ToString() ?? "null", condition, message, caller);
        }
        catch (Exception ex) when (ex is not CodeContractViolationException)
        {
            throw new CodeContractViolationException(
                "An error occurred while checking a code contract - see inner exception for details", ex);
        }
    }

    /// <summary>
    ///     Asserts that a reference type is not null.
    /// </summary>
    /// <param name="value">A reference.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    public static void ContractAssertNotNull<T>(this T value, [CallerMemberName] string? caller = null) where T : class =>
        value.ContractAssert(p => p != null, "Reference type must not be null.", caller);

    /// <summary>
    ///     Asserts that a nullable value type has a value.
    /// </summary>
    /// <param name="value">A nullable struct.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    public static void ContractAssertNotNull<T>(this T? value, [CallerMemberName] string? caller = null) where T : struct =>
        value.ContractAssert(p => !p.HasValue, "nullable value type must have a value.", caller);

    /// <summary>
    ///     Asserts that a collection is not empty (A null reference is empty by definition).
    /// </summary>
    /// <param name="collection">The collection being tested.</param>
    public static void ContractAssertNotEmpty(this ICollection collection, [CallerMemberName] string? caller = null)
    {
        collection.ContractAssertNotNull(caller);
        collection.ContractAssert(p => p.Count > 0, "Collection must not be empty", caller);
    }

    /// <summary>
    ///     Asserts that a collection is not empty (A null reference is empty by definition).
    /// </summary>
    /// <param name="collection">The collection being tested.</param>
    public static void ContractAssertNotEmpty<T>(this IEnumerable<T> collection, [CallerMemberName] string? caller = null) =>
        collection.ContractAssert(p => p != null && p.Any(), "Enumerable must not be empty", caller);

    /// <summary>
    ///     Asserts that a source enumerable sequence contains the specified item at least once. Note: potentially expensive
    ///     operation O(n)
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="item">The required item.</param>
    /// <typeparam name="TItem">The type of items in the collection.</typeparam>
    public static void ContractAssertContains<TItem>(this IEnumerable<TItem> source, TItem item,
        [CallerMemberName]                                string?            caller = null) =>
        source.ContractAssert(p => p.Contains(item), "The source collection must contain the specified item", caller);

    /// <summary>
    ///     Asserts that a source enumerable sequence does not contain the specified item. Note: potentially expensive
    ///     operation O(n).
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="item">The disallowed item.</param>
    /// <typeparam name="TItem">The type of items in the collection.</typeparam>
    public static void ContractAssertDoesNotContain<TItem>(this IEnumerable<TItem> source, TItem item,
        [CallerMemberName]                                      string?            caller = null) =>
        source.ContractAssert(p => !p.Contains(item), "The source collection must not contain the specified item", caller);

    private static void ThrowContractException<T>(string? value, Expression<Func<T, bool>> condition, string message,
        string?                                           caller)
    {
        var ex = new CodeContractViolationException(message);
        ex.Data["Condition"] = condition.ToString(); // this should serialize the expression tree.
        ex.Data["Value"] = value;
        ex.Data["Caller"] = caller ?? "(unknown caller)";
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