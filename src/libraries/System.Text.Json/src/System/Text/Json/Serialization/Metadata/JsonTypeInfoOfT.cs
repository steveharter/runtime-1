// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Metadata
{
    /// <summary>
    /// Provides JSON serialization-related metadata about a type.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class JsonTypeInfo<T> : JsonTypeInfo
    {
        internal JsonTypeInfo(ConverterStrategy strategy, JsonSerializerOptions options) : base(typeof(T), options, strategy)
        {
        }

        internal void SetCreateObjectFunc(Func<T>? createObjectFunc)
        {
            if (createObjectFunc != null)
            {
                CreateObject = () => createObjectFunc();
            }
        }
    }
}
