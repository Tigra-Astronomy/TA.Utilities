// This file is part of the BeaconFinalTest project
// Copyright © Ocean Signal Limited, all rights reserved.
// 
// Company Confidential
// 
// File: KeyValueDataRecord.cs  Last modified: 2023-11-06@09:16 by Tim Long

namespace TA.Utils.Core.PropertyBinding;

/// <summary>
///     Represents a single key-value pair typically read from an INI-style text file.
///     Values are deserialized as strings and will require further parsing to get usable types.
///     Intended for use by <see cref="PropertyBinder" />
/// </summary>
public record KeyValueDataRecord(string Key, string Value)
{
    /// <summary>
    ///     The data key name.
    /// </summary>
    public string Key { get; } = Key;

    /// <summary>
    ///     The data value.
    /// </summary>
    public string Value { get; } = Value;
}