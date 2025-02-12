﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Formats.Nrbf;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Private.Windows.BinaryFormat;

internal static class BinaryFormatTestExtensions
{
    /// <summary>
    ///  Serializes the object using the <see cref="BinaryFormatter"/> and reads it into a <see cref="SerializationRecord"/>.
    /// </summary>
    public static SerializationRecord SerializeAndDecode(this object source)
    {
        using Stream stream = source.Serialize();
        return NrbfDecoder.Decode(stream);
    }

    /// <summary>
    ///  Serializes the object using the <see cref="BinaryFormatter"/>.
    /// </summary>
    public static Stream Serialize(this object source)
    {
        MemoryStream stream = new();
        using BinaryFormatterScope formatterScope = new(enable: true);
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        BinaryFormatter formatter = new();
#pragma warning restore SYSLIB0011
        formatter.Serialize(stream, source);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    ///  Returns `true` if the <see cref="Type"/> would use the <see cref="BinaryFormatter"/> for the purposes
    ///  of designer serialization (either through Resx or IPropertyBag for ActiveXImpl).
    /// </summary>
    public static bool IsBinaryFormatted(this Type type)
    {
        bool iSerializable = type.IsAssignableTo(typeof(ISerializable));
#pragma warning disable SYSLIB0050 // Type or member is obsolete
        bool serializable = type.IsSerializable;
#pragma warning restore SYSLIB0050

        if (!iSerializable && !serializable)
        {
            return false;
        }

        TypeConverter converter;
        try
        {
            converter = TypeDescriptor.GetConverter(type);
        }
        catch (Exception)
        {
            // No valid type converter.
            return true;
        }

        return !((converter.CanConvertFrom(typeof(string)) && converter.CanConvertTo(typeof(string)))
            || (converter.CanConvertFrom(typeof(byte[])) && converter.CanConvertTo(typeof(byte[]))));
    }
}
