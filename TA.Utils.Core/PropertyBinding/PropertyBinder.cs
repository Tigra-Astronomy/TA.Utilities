// This file is part of the TA.ObjectOrientedAstronomy project
// 
// Copyright © 2015-2016 Tigra Astronomy, all rights reserved.
// 
// File: PropertyBinder.cs  Last modified: 2016-10-13@01:03 by Tim Long

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TA.Utils.Core.Diagnostics;

namespace TA.Utils.Core.PropertyBinding;

/// <summary>
///     Interface IPropertyBinder - an object capable of populating the properties of an object from a data source.
/// </summary>
public interface IPropertyBinder
{
    /// <summary>
    ///     Given an <see cref="IEnumerable{KeyValueDataRecord}" /> containing data as key-value pairs,
    ///     attempts to create an object of type <typeparamref name="TOut" /> with its properties set to values obtained from
    ///     the data. Properties are matched with data records in one of several ways:
    ///     <list type="number">
    ///         <item>
    ///             <term>
    ///                 using the key name specified in a <see cref="DataKeyAttribute" /> applied to the property.
    ///             </term>
    ///             <description>
    ///                 The property is examined using reflection to see if it has been decorated with one or more
    ///                 <see cref="DataKeyAttribute" /> attributes. If found, the Keyword value from each attribute is tried in
    ///                 turn to search the data records for a match. The value from the first matching record is used to
    ///                 set the property value.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Property name</term>
    ///             <description>
    ///                 The name of the target property is used to search the keys in the data records. The
    ///                 value from the first matching record is used to set the property value.
    ///                 A case-insensitive search is used, so that "Thing" property will match a "thing" keyword.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Property implementing <see cref="IList" /> (complex property)</term>
    ///             <description>
    ///                 In the case of a 'complex' property that implements <see cref="IList" />, use of the
    ///                 <see cref="DataKeyAttribute" /> is mandatory. Multiple attributes may be applied, and for each
    ///                 attribute, values from all matching data records will be added to the collection property.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     If no matching records are found, or if the process fails in any way, the property is left uninitialized and will
    ///     have its default value.
    /// </summary>
    /// <typeparam name="TOut">
    ///     The type of the target object. Constraint: reference type with a public parameterless
    ///     constructor.
    /// </typeparam>
    /// <param name="dataRecords">An enumerable collection of <see cref="KeyValueDataRecord" />.</param>
    /// <returns>
    ///     A new instance of <typeparamref name="TOut" />, with its properties initialized from the values in the
    ///     data records.
    /// </returns>
    TOut BindProperties<TOut>(IEnumerable<KeyValueDataRecord> dataRecords) where TOut : new();
}

/// <summary>
///     Provides a mechanism for binding values in key-value pair records to properties in an arbitrary reference type.
///     Binding is based on the property names of the target type, and the key values in the data records.
///     It is also possible to override the default matching strategy by applying <see cref="DataKeyAttribute" />
///     attributes to the properties being bound. The key names specified in the attribute will then take precedence over
///     the property names.
/// </summary>
public class PropertyBinder : IPropertyBinder
{
    private readonly ILog log;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyBinder" /> class.
    /// </summary>
    /// <param name="log">A logging service (optional).</param>
    public PropertyBinder(ILog? log = null)
    {
        this.log = log ?? new DegenerateLoggerService();
    }


    /// <summary>
    ///     Given an <see cref="IEnumerable{KeyValueDataRecord}" /> containing FITS header metadata,
    ///     attempts to create a data transfer object of type <typeparamref name="TOut" />
    ///     with its properties set to values obtained from the FITS metadata.
    ///     Properties are matched with keywords in the FITS header records in one of several ways:
    ///     <list type="number">
    ///         <item>
    ///             <term>
    ///                 using the keyword specified in a <see cref="DataKeyAttribute" /> applied to the property.
    ///             </term>
    ///             <description>
    ///                 The property is examined using reflection to see if it has been decorated with one or more
    ///                 <see cref="DataKeyAttribute" /> attributes. If found, the Keyword value from each attribute is tried in
    ///                 turn to search the data records for a match. The value from the first matching record is used to
    ///                 set the property value.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Property name</term>
    ///             <description>
    ///                 The name of the target property is used to search the FITS header records for a match. The
    ///                 value from the first matching header record is used to set the property value.
    ///                 A case-insensitive search is used, so that "Thing" property will match a "thing" keyword.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Property implementing <see cref="IList" /> (complex property)</term>
    ///             <description>
    ///                 In the case of a 'complex' property that implements <see cref="IList" />, use of the
    ///                 <see cref="DataKeyAttribute" /> is mandatory. Multiple attributes may be applied, and for each
    ///                 attribute, values from all matching data records will be added to the collection property.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     If no matching records are found, or if the process fails in any way, the property is left uninitialized and will
    ///     have its default value.
    /// </summary>
    /// <typeparam name="TOut">The type of the target data transfer object.</typeparam>
    /// <param name="dataRecords">An enumerable collection of <see cref="KeyValueDataRecord" /> records.</param>
    /// <returns>
    ///     A new instance of <typeparamref name="TOut" />, with its properties initialized from the values in the
    ///     data records.
    /// </returns>
    public TOut BindProperties<TOut>(IEnumerable<KeyValueDataRecord> dataRecords) where TOut : new()
    {
        var type = typeof(TOut);
        var targetProperties = type.GetProperties();
        var target = new TOut();
        object targetObject = target; // This ensures that if the target type is a struct, then it is boxed.
        log.Debug().Message("Beginning property binding into type {type}", type.Name).Write();
        var keyValueDataList = dataRecords.ToList();
        foreach (var property in targetProperties)
            BindProperty(keyValueDataList, property, targetObject);
        target = (TOut) targetObject; // This unboxes the target if it is a struct.
        return target;
    }

    /// <summary>
    ///     Binds a single property.
    /// </summary>
    /// <param name="keyValueRecords">The source dictionary containing the key/value pairs to be bound.</param>
    /// <param name="property">The property being bound.</param>
    /// <param name="target">The target object.</param>
    private void BindProperty(IEnumerable<KeyValueDataRecord> keyValueRecords, PropertyInfo property,
        object target)
    {
        property.ContractAssertNotNull();
        target.ContractAssertNotNull();
        try
        {
            var targetType = PropertyTypeOrUnderlyingCollectionType(property);
            var sourceKeywords = GetKeywordNamesFromPropertyNameOrAttributes(property);
            log.Debug()
                .Message("Binding property {propertyName} of type {type} using source keywords {keywords}",
                    property.Name, targetType.Name, sourceKeywords)
                .Write();
            var sourceRecords = from keyValueRecord in keyValueRecords
                where sourceKeywords.Contains(keyValueRecord.Key,
                    StringComparer.InvariantCultureIgnoreCase)
                select keyValueRecord;
            var deserializedValues = DeserializedValues(sourceRecords, targetType).ToList();
            log.Debug().Message("Obtained the following values: {values}", deserializedValues).Write();
            if (IsAssignableToList(property))
            {
                // Collection properties can accept the entire collection of values.
                PopulateTargetCollection(target, property, deserializedValues);
                return;
            }

            PopulateTargetProperty(target, property, deserializedValues);
        }

        catch (InvalidOperationException ex)
        {
            // We don't fail, the property is just left unset. But we should log it as a warning.
            log.Error()
                .Message("Type conversion failed for {propertyName} property", property.Name)
                .Exception(ex)
                .Write();
        }
    }

    /// <summary>
    ///     Gets the type of a 'simple' or collection property.
    ///     If the property is assignable to <c>IList</c> or <c>IList{T}</c>, then it is a collection.
    ///     For generic collections, the type is the generic type paramter of the collection.
    ///     For non-generic collections, the type is <c>object</c>.
    ///     For simple properties, the type is the property type.
    /// </summary>
    /// <param name="property">An instance of <see cref="PropertyInfo" />.</param>
    /// <returns>The underlying type of the property.</returns>
    private Type PropertyTypeOrUnderlyingCollectionType(PropertyInfo property)
    {
        property.ContractAssertNotNull();
        var propertyType = property.PropertyType;
        if (IsAssignableToList(property))
        {
            var genericTypeArgs = propertyType.GetGenericArguments();
            if (genericTypeArgs.Length < 1)
                return typeof(object);
            var targetType = genericTypeArgs[0];
            return targetType;
        }

        return property.PropertyType;
    }

    private bool IsAssignableToList(PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        var isGenericTypeAssignableToList = propertyType.IsGenericType &&
                                            typeof(IList<>).IsAssignableFrom(propertyType.GetGenericTypeDefinition());
        var isSimpleTypeAssignableToList = typeof(IList).IsAssignableFrom(propertyType);
        return isSimpleTypeAssignableToList || isGenericTypeAssignableToList;

    }

    /// <summary>
    ///     Use reflection to populate a collection property on an object instance with values from a value collection.
    ///     The property being populated must be assignable to <see cref="IList" /> or it will nto be set.
    /// </summary>
    /// <param name="target">The target object instance.</param>
    /// <param name="property">The <see cref="PropertyInfo" /> for the property to be set.</param>
    /// <param name="values">The collection of data values to initialise the property with.</param>
    private void PopulateTargetCollection(object target, PropertyInfo property,
        IEnumerable<object> values)
    {
        target.ContractAssertNotNull();
        property.ContractAssertNotNull();
        var propertyType = property.PropertyType;
        IList collection;
        if (propertyType.IsInterface)
        {
            var genericType = propertyType.GetGenericTypeDefinition();
            Type listType = typeof(List<>);
            Type specificListType = listType.MakeGenericType(genericType); // replace typeof(string) with the type you want
            collection = (IList)Activator.CreateInstance(specificListType);
        }
        else
         collection = (IList)Activator.CreateInstance(propertyType);
        property.SetValue(target, collection, null);
        //var targetCollection = collection as IList;
        if (collection == null)
        {
            log.Warn()
                .Message("Unable to populate collection property {propertyName} because it could not be downcast to IList",
                    property.Name)
                .Write();
            return; // No point in continuing if we can't save the results anywhere.
        }

        foreach (var value in values)
            collection.Add(value);
    }

    /// <summary>
    ///     Use reflection to set a property of an object instance with the first available value in a collection of values.
    /// </summary>
    /// <param name="target">The target object instance.</param>
    /// <param name="property">The <see cref="PropertyInfo" /> for the property to be set.</param>
    /// <param name="values">The converted values.</param>
    private void PopulateTargetProperty(object target, PropertyInfo property,
        IEnumerable<object> values)
    {
        Contract.Requires(values != null);
        if (!values.Any())
            return; // If there are no values, make no attempt to set the target property.
        property.SetValue(target, values.First());
    }

    /// <summary>
    ///     Deserializes values from a collection of <see cref="KeyValueDataRecord" />
    ///     converting each value to type <paramref name="targetType" />
    ///     and returns the successfully converted values in an enumerable collection.
    /// </summary>
    /// <param name="keyValueRecords">The source dictionary of key/value pairs.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>System.Object containing the converted type, or null if the conversion failed, or the key did not exist.</returns>
    private IEnumerable<object> DeserializedValues(IEnumerable<KeyValueDataRecord> keyValueRecords, Type targetType)
    {
        Contract.Requires(keyValueRecords != null);
        var deserializedValues = from keyValueRecord in keyValueRecords
            let deserializedValue =
                Maybe<object>.From(DeserializeToType(keyValueRecord.Value, targetType))
            where deserializedValue.Any()
            select deserializedValue.Single();
        return deserializedValues;
    }

    /// <summary>
    ///     Attempt to deserialize a string value into a target type.
    ///     Returns a populated instance of the target type, or null.
    /// </summary>
    /// <param name="valueString">String containing a serialized data value.</param>
    /// <param name="targetType">Target type to attempt to deserialize into.</param>
    /// <returns>A populated new instance of the target type, or null if deserialization failed.</returns>
    private object? DeserializeToType(string valueString, Type targetType)
    {
        valueString.ContractAssertNotEmpty();
        targetType.ContractAssertNotNull();
        // Special handling for strings, just return the trimmed string.
        if (targetType == typeof(string))
            return valueString.Trim(); // Remove leading and trailing white space.
        var convertedValue = ConvertStringToScalarObject(valueString, targetType);
        return convertedValue;
    }

    /// <summary>
    ///     Gets a set of candidate data key names for a given property.
    ///     This will be determined by the presence of one or more <see cref="DataKeyAttribute" /> attributes, if any are
    ///     present. If no attribute is present, then the name of the property itself will be used.
    /// </summary>
    /// <param name="property">The <see cref="PropertyInfo" /> for property being examined.</param>
    /// <returns>A collection of candidate data key names.</returns>
    private IEnumerable<string> GetKeywordNamesFromPropertyNameOrAttributes(PropertyInfo property)
    {
        property.ContractAssertNotNull();
        var attributes = property.GetCustomAttributes(typeof(DataKeyAttribute), false);
        if (attributes.Length == 0)
            return new List<string> { property.Name };
        return attributes.Cast<DataKeyAttribute>().OrderBy(p => p.Sequence).Select(p => p.Keyword);
    }

    /// <summary>
    ///     Attempt to deserialize a scalar type from a string.
    ///     This technique is borrowed in part from the ASP.net MVC ModelBinder subsystem.
    /// </summary>
    /// <param name="value">The serialized value string.</param>
    /// <param name="destinationType">Type of the destination scalar type.</param>
    /// <returns>A new <see cref="object" /> containing the value, or null if conversion failed.</returns>
    private object? ConvertStringToScalarObject(string value, Type destinationType)
    {
        value.ContractAssertNotNull();
        destinationType.ContractAssertNotNull();
        // Shortcut the case where the target type is a string.
        if (destinationType.IsInstanceOfType(value))
            return value;
        // In case of a Nullable object, we extract the underlying type and try to convert it.
        var underlyingType = Nullable.GetUnderlyingType(destinationType);
        if (underlyingType != null)
            destinationType = underlyingType;
        // look for a type converter that can convert from string to the specified type.
        var converter = TypeDescriptor.GetConverter(destinationType);
        var canConvertFrom = converter.CanConvertFrom(value.GetType());
        if (!canConvertFrom)
            converter = TypeDescriptor.GetConverter(value.GetType());
        var culture = CultureInfo.InvariantCulture;
        try
        {
            var convertedValue = canConvertFrom
                ? converter.ConvertFrom(null /* context */, culture, value)
                : converter.ConvertTo(null /* context */, culture, value, destinationType);
            return convertedValue;
        }
        catch (Exception ex)
        {
            log.Error().Message("Conversion from string to type {destinationType} failed", destinationType.FullName)
                .Exception(ex);
            throw new InvalidOperationException(
                $"Conversion from string to type '{destinationType.FullName}' failed. See the inner exception for more information.",
                ex);
        }
    }
}