// This file is part of the TA.Utils project
// Copyright © 2015-2024 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: DataKeyAttribute.cs  Last modified: 2024-7-17@16:11 by tim.long


using System;

namespace TA.Utils.Core.PropertyBinding;

/// <summary>
///     Class DataKeyAttribute. This class cannot be inherited. Used with property binding to identify the
///     data file keyword that should provide the data for a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class DataKeyAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataKeyAttribute" /> class.
    /// </summary>
    /// <param name="keyword">
    ///     The name of the key-value pair that will supply the data for
    ///     the decorated property.
    /// </param>
    public DataKeyAttribute(string keyword)
    {
        Sequence = 0;
        Keyword = keyword;
    }

    /// <summary>
    ///     Gets the FITS keyword.
    /// </summary>
    /// <value>The keyword.</value>
    public string Keyword { get; }

    /// <summary>
    ///     Gets or sets the sequence number of this attribute. In situations where multiple attributes are applied
    ///     to the same property, this sequence number determines the order in which the attributes are considered.
    ///     Bindings occur in ascending order of sequence numbers. Attributes where the sequence number is not
    ///     specified will have a default of zero. For properties with equal sequence numbers, the order is
    ///     undefined and those properties may be used in any order relative to each other. Therefore, it is
    ///     recommended that distinct sequence numbers are always used when multiple attributes are applied to a property.
    /// </summary>
    /// <value>The sequence number.</value>
    public int Sequence { get; set; }
}